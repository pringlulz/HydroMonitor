using System;
using System.Threading.Tasks;
using Microsoft.Maui;
using Microsoft.Maui.Hosting;
using Services;

namespace HydroMonitor
{
    internal class Program : MauiApplication
    {
        protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();

        static async Task Main(string[] args)
        {
            var app = new Program();
            app.Run(args);

               
        }
    }
}
