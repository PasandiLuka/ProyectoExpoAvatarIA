using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using AvatarExpo;
using AvatarExpo.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddSingleton<AvatarState>();
builder.Services.AddScoped<TrackingService>(sp =>
{
    var js = sp.GetRequiredService<Microsoft.JSInterop.IJSRuntime>();
    var wsUrl = builder.Configuration["WebSocket:Url"]
        ?? "ws://" + new Uri(builder.HostEnvironment.BaseAddress).Host + ":8765";
    var tracking = new TrackingService(wsUrl, js);
    var camera = sp.GetRequiredService<CameraService>();
    tracking.SetCameraService(camera);
    camera.SetTrackingService(tracking);
    return tracking;
});
builder.Services.AddScoped<CameraService>();

await builder.Build().RunAsync();
