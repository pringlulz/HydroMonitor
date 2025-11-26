using HydroMonitor.Interfaces;
using HydroMonitor.Models;
using HydroMonitor.Repository;
using HydroMonitor.Services;
using Microsoft.Extensions.Logging;
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

            builder.Services.AddSingleton<SensorTypeDAO>( st =>
            {
                SensorTypeDAO stDAO = new SensorTypeDAO();
                stDAO.AddDefaultSensorTypes();
                return stDAO;
            });
            builder.Services.AddSingleton<SensorReadingDAO>();
            builder.Services.AddSingleton<SensorLocationDAO>();
            builder.Services.AddSingleton<GeolocationService>();
            builder.Services.AddSingleton<SensorDAO>( sp =>
            {
                return new SensorDAO(sp.GetRequiredService<SensorTypeDAO>());
            } ); //this is the database connector for the Sensor object

            builder.Services.AddSingleton<MQTTService>(sp =>
            {
                return new MQTTService(sp.GetRequiredService<SensorDAO>(), sp.GetRequiredService<SensorReadingDAO>());
            }); //this uses MQTTnet to subscribe to the data coming out of the raspberry pi



#if DEBUG
            builder.Services.AddBlazorWebViewDeveloperTools();
    		builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
