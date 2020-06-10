using System;
using System.Net.Http;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MiPWinRTSDK.MiP;
using Blazm.Bluetooth;

namespace SampleClientSide
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("app");

            builder.Services.AddTransient(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
            builder.Services.AddScoped<MiPRobot>();
            //<1. Add BlazmBluetooth>
            builder.Services.AddBlazmBluetooth();
            //</1. Add BlazmBluetooth>
            await builder.Build().RunAsync();
        }
    }
}
