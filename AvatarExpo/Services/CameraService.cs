using Microsoft.JSInterop;

namespace AvatarExpo.Services;

public class CameraService : IAsyncDisposable
{
    private readonly IJSRuntime _js;
    private DotNetObjectReference<CameraService>? _dotNetRef;
    private TrackingService? _trackingService;
    private bool _isRunning;

    public event Action<byte[]>? OnFrame;

    public CameraService(IJSRuntime js)
    {
        _js = js;
    }

    public void SetTrackingService(TrackingService trackingService)
    {
        _trackingService = trackingService;
    }

    public async Task<bool> StartCamera()
    {
        _dotNetRef = DotNetObjectReference.Create(this);
        var result = await _js.InvokeAsync<bool>("avatarCamera.start", _dotNetRef);
        _isRunning = result;
        return result;
    }

    public async Task StopCamera()
    {
        _isRunning = false;
        await _js.InvokeVoidAsync("avatarCamera.stop");
        _dotNetRef?.Dispose();
        _dotNetRef = null;
    }

    [JSInvokable]
    public async Task ReceiveFrame(byte[] jpegBytes)
    {
        if (_trackingService != null && _trackingService.IsConnected)
        {
            await _trackingService.SendFrame(jpegBytes);
        }
        OnFrame?.Invoke(jpegBytes);
    }

    public async Task SetFps(int fps)
    {
        await _js.InvokeVoidAsync("avatarCamera.setFps", fps);
    }

    public async Task SetQuality(double quality)
    {
        await _js.InvokeVoidAsync("avatarCamera.setQuality", quality);
    }

    public async Task<string> CaptureScreenshot()
    {
        return await _js.InvokeAsync<string>("avatarCamera.captureAvatarToBase64");
    }

    public bool IsRunning => _isRunning;

    public async ValueTask DisposeAsync()
    {
        await StopCamera();
    }
}
