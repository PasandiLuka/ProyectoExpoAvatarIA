using System.Diagnostics;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Microsoft.JSInterop;

namespace AvatarExpo.Services;

public class TrackingService : IAsyncDisposable
{
    private ClientWebSocket? _ws;
    private CancellationTokenSource? _cts;
    private Task? _receiveTask;
    private Task? _heartbeatTask;
    private readonly string _serverUrl;
    private readonly JsonSerializerOptions _jsonOptions;

    private int _reconnectAttempt;
    private const int MaxReconnectAttempts = 10;
    private const int HeartbeatIntervalMs = 5000;
    private const int HeartbeatTimeoutMs = 3000;

    private double _currentRttMs;
    private Stopwatch _rttStopwatch = new();

    private readonly Queue<double> _rttSamples = new();
    private const int RttSampleWindow = 10;

    public bool IsConnected => _ws?.State == WebSocketState.Open;
    public bool IsDemoMode { get; private set; } = true;
    public int CurrentFps { get; private set; } = 30;

    public event Action<ProcessedLandmarks>? OnLandmarksReceived;
    public event Action<string>? OnStatusChanged;
    public event Action<int>? OnFpsChanged;
    public event Action<bool>? OnTrackingStateChanged;
    public event Action<bool, string>? OnScreenshotResult;
    public event Action<bool>? OnConnectionChanged;
    public event Action<string, bool, string>? OnDriveSyncResult;

    private CameraService? _cameraService;
    private CalibrationOffset? _calibration;
    private readonly IJSRuntime? _js;

    public TrackingService(string serverUrl, IJSRuntime? js = null)
    {
        _serverUrl = serverUrl;
        _js = js;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    public void SetCameraService(CameraService cameraService)
    {
        _cameraService = cameraService;
    }

    public async Task Connect()
    {
        _reconnectAttempt = 0;
        await TryConnect();
    }

    private async Task TryConnect()
    {
        Console.WriteLine($"[TrackingService] Conectando a {_serverUrl}...");
        try
        {
            _cts?.Cancel();
            _cts = new CancellationTokenSource();

            _ws?.Dispose();
            _ws = new ClientWebSocket();
            await _ws.ConnectAsync(new Uri(_serverUrl), _cts.Token);

            _reconnectAttempt = 0;
            IsDemoMode = false;
            OnTrackingStateChanged?.Invoke(true);
            OnConnectionChanged?.Invoke(true);
            OnStatusChanged?.Invoke("Conectado");

            _receiveTask = Task.Run(() => ReceiveLoop(_cts.Token));
            _heartbeatTask = Task.Run(() => HeartbeatLoop(_cts.Token));

            Console.WriteLine("[TrackingService] Conectado, esperando landmarks...");
        }
        catch (Exception ex)
        {
            IsDemoMode = true;
            OnTrackingStateChanged?.Invoke(false);
            OnConnectionChanged?.Invoke(false);
            OnStatusChanged?.Invoke($"Error: {ex.Message}");
            _ = Task.Run(() => ReconnectLoop());
        }
    }

    private async Task ReceiveLoop(CancellationToken ct)
    {
        var buffer = new byte[8192];
        var frameCount = 0;

        while (!ct.IsCancellationRequested && _ws?.State == WebSocketState.Open)
        {
            try
            {
                var result = await _ws.ReceiveAsync(new ArraySegment<byte>(buffer), ct);

                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await _ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "", ct);
                    break;
                }

                if (result.MessageType == WebSocketMessageType.Text)
                {
                    var json = Encoding.UTF8.GetString(buffer, 0, result.Count);

                    if (json == "pong")
                    {
                        if (_rttStopwatch.IsRunning)
                        {
                            _rttStopwatch.Stop();
                            _currentRttMs = _rttStopwatch.Elapsed.TotalMilliseconds;
                            UpdateRttWindow(_currentRttMs);
                            AdjustFps();
                        }
                        continue;
                    }

                    try
                    {
                        using var doc = JsonDocument.Parse(json);
                        var root = doc.RootElement;
                        if (root.TryGetProperty("type", out var typeEl))
                        {
                            var type = typeEl.GetString();
                            if (type == "screenshot_result")
                            {
                                var success = root.GetProperty("success").GetBoolean();
                                var url = root.TryGetProperty("url", out var urlEl) ? urlEl.GetString() ?? "" : "";
                                OnScreenshotResult?.Invoke(success, url);
                                continue;
                            }
                            if (type == "drive_sync_result")
                            {
                                var filename = root.TryGetProperty("filename", out var fnEl) ? fnEl.GetString() ?? "" : "";
                                var success = root.GetProperty("success").GetBoolean();
                                var url = root.TryGetProperty("url", out var drUrlEl) ? drUrlEl.GetString() ?? "" : "";
                                OnDriveSyncResult?.Invoke(filename, success, url);
                                continue;
                            }
                        }
                    }
                    catch { }

                    var data = JsonSerializer.Deserialize<LandmarkData>(json, _jsonOptions);

                    if (data?.Skeleton != null && _js != null)
                    {
                        _ = _js.InvokeVoidAsync("avatarCamera.drawSkeleton", data.Skeleton);
                    }

                    if (data?.P != null)
                    {
                        try
                        {
                            var processed = LandmarkParser.Parse(data);
                            if (_calibration != null)
                                processed = ApplyCalibration(processed);
                            OnLandmarksReceived?.Invoke(processed);

                            frameCount++;
                            if (frameCount % 60 == 0)
                                Console.WriteLine($"[TrackingService] Landmarks OK | frame={frameCount} | fps={CurrentFps}");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"[TrackingService] Handler error: {ex.Message}");
                        }
                    }
                }
            }
            catch (OperationCanceledException) { break; }
            catch (WebSocketException)
            {
                break;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TrackingService] ReceiveLoop error: {ex}");
                break;
            }
        }

        if (!ct.IsCancellationRequested)
        {
            Console.WriteLine("[TrackingService] Desconectado - iniciando reconexion...");
            IsDemoMode = true;
            OnTrackingStateChanged?.Invoke(false);
            OnConnectionChanged?.Invoke(false);
            OnStatusChanged?.Invoke("Desconectado - Reconectando...");
            _ = Task.Run(() => ReconnectLoop());
        }
    }

    private async Task HeartbeatLoop(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested && _ws?.State == WebSocketState.Open)
        {
            try
            {
                await Task.Delay(HeartbeatIntervalMs, ct);
                _rttStopwatch.Restart();
                var pingBytes = Encoding.UTF8.GetBytes("ping");
                await _ws.SendAsync(new ArraySegment<byte>(pingBytes), WebSocketMessageType.Text, true, ct);
            }
            catch (OperationCanceledException) { break; }
            catch { break; }
        }
    }

    private async Task ReconnectLoop()
    {
        while (!IsConnected && _reconnectAttempt < MaxReconnectAttempts)
        {
            _reconnectAttempt++;
            var delay = Math.Min(1000 * Math.Pow(2, _reconnectAttempt - 1), 16000);

            if (_reconnectAttempt > 5)
                OnStatusChanged?.Invoke($"Reconectando... (intento {_reconnectAttempt})");

            await Task.Delay((int)delay);
            await TryConnect();

            if (IsConnected) return;
        }

        if (!IsConnected)
        {
            IsDemoMode = true;
            OnTrackingStateChanged?.Invoke(false);
            OnConnectionChanged?.Invoke(false);
            OnStatusChanged?.Invoke("Conexion perdida - Modo demo activo");
        }
    }

    public async Task SendFrame(byte[] jpegBytes)
    {
        if (_ws?.State == WebSocketState.Open)
        {
            try
            {
                await _ws.SendAsync(new ArraySegment<byte>(jpegBytes), WebSocketMessageType.Binary, true, _cts?.Token ?? CancellationToken.None);
            }
            catch { }
        }
    }

    public async Task<bool> SendScreenshot(string base64)
    {
        if (_ws?.State != WebSocketState.Open) return false;

        var now = DateTime.UtcNow.AddHours(-3);
        var filename = $"avatar-{now:yyyyMMdd}-{now:HHmmss}.png";
        var payload = JsonSerializer.Serialize(new
        {
            type = "screenshot",
            image = base64,
            filename
        });

        try
        {
            var bytes = Encoding.UTF8.GetBytes(payload);
            await _ws.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, _cts?.Token ?? CancellationToken.None);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task Disconnect()
    {
        IsDemoMode = true;
        _cts?.Cancel();

        if (_ws?.State == WebSocketState.Open)
        {
            try
            {
                await _ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
            }
            catch { }
        }

        _ws?.Dispose();
        _ws = null;
        OnTrackingStateChanged?.Invoke(false);
    }

    private void UpdateRttWindow(double rtt)
    {
        _rttSamples.Enqueue(rtt);
        while (_rttSamples.Count > RttSampleWindow)
            _rttSamples.Dequeue();
    }

    private void AdjustFps()
    {
        if (_rttSamples.Count < 3) return;

        var avgRtt = _rttSamples.Average();
        int newFps;

        if (avgRtt < 50) newFps = 30;
        else if (avgRtt < 100) newFps = 24;
        else if (avgRtt < 200) newFps = 20;
        else newFps = 15;

        if (newFps != CurrentFps)
        {
            CurrentFps = newFps;
            OnFpsChanged?.Invoke(newFps);
            _cameraService?.SetFps(newFps);
        }
    }

    public async Task Calibrate()
    {
        var shoulderLS = new List<(double x, double y, double z)>();
        var shoulderRS = new List<(double x, double y, double z)>();

        var sub = new Action<ProcessedLandmarks>(data =>
        {
            shoulderLS.Add((data.LSx, data.LSy, data.LSz));
            shoulderRS.Add((data.RSx, data.RSy, data.RSz));
        });

        OnLandmarksReceived += sub;

        var elapsed = 0;
        while (elapsed < 2000)
        {
            await Task.Delay(50);
            elapsed += 50;
        }

        OnLandmarksReceived -= sub;

        if (shoulderLS.Count > 0 && shoulderRS.Count > 0)
        {
            _calibration = new CalibrationOffset
            {
                LSxOffset = shoulderLS.Average(s => s.x),
                LSyOffset = shoulderLS.Average(s => s.y),
                LSzOffset = shoulderLS.Average(s => s.z),
                RSxOffset = shoulderRS.Average(s => s.x),
                RSyOffset = shoulderRS.Average(s => s.y),
                RSzOffset = shoulderRS.Average(s => s.z),
            };
        }
    }

    private ProcessedLandmarks ApplyCalibration(ProcessedLandmarks data)
    {
        if (_calibration == null) return data;

        return new ProcessedLandmarks
        {
            LSx = data.LSx - _calibration.LSxOffset,
            LSy = data.LSy - _calibration.LSyOffset,
            LSz = data.LSz - _calibration.LSzOffset,
            LEx = data.LEx, LEy = data.LEy,
            LWx = data.LWx, LWy = data.LWy,
            RSx = data.RSx - _calibration.RSxOffset,
            RSy = data.RSy - _calibration.RSyOffset,
            RSz = data.RSz - _calibration.RSzOffset,
            REx = data.REx, REy = data.REy,
            RWx = data.RWx, RWy = data.RWy,
            Expression = data.Expression,
            LeftHandVis = data.LeftHandVis,
            RightHandVis = data.RightHandVis
        };
    }

    public async ValueTask DisposeAsync()
    {
        await Disconnect();
    }

    private class CalibrationOffset
    {
        public double LSxOffset { get; set; }
        public double LSyOffset { get; set; }
        public double LSzOffset { get; set; }
        public double RSxOffset { get; set; }
        public double RSyOffset { get; set; }
        public double RSzOffset { get; set; }
    }
}
