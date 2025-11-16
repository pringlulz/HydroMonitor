using Microsoft.Extensions.Logging;
using HydroMonitor.Services;
using HydroMonitor.Repository;
using Syncfusion.Maui.Toolkit.Hosting;
namespace HydroMonitor
{
#pragma warning disable CA1416
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureSyncfusionToolkit()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                });

            builder.Services.AddMauiBlazorWebView();
            builder.Services.AddBlazorBootstrap();
            builder.Services.AddSingleton<MQTTService>(); //this uses MQTTnet to subscribe to the data coming out of the raspberry pi
            builder.Services.AddSingleton<SensorTypeDAO>();
            builder.Services.AddSingleton<GeolocationService>();
            builder.Services.AddSingleton<SensorDAO>( ); //this is the database connector for the Sensor object




#if DEBUG
            builder.Services.AddBlazorWebViewDeveloperTools();
    		builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
