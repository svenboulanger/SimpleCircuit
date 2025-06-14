using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Blazored.LocalStorage;
using Microsoft.JSInterop;
using SimpleCircuit.Parser.SimpleTexts;
using SimpleCircuit.Drawing.Spans;

namespace SimpleCircuitOnline
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("app");

            builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
            builder.Services.AddSingleton<ITextMeasurer>(sp => new TextMeasurer(sp.GetService<IJSRuntime>()));
            builder.Services.AddSingleton<ITextFormatter>(sp => new SimpleTextFormatter(sp.GetService<ITextMeasurer>()));
            builder.Services.AddBlazoredLocalStorage();

            await builder.Build().RunAsync();
        }
    }
}
