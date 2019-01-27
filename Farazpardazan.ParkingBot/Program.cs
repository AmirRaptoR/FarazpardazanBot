using System.Threading.Tasks;
using Farazpardazan.ParkingBot.Parking;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;

namespace Farazpardazan.ParkingBot
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console(theme: AnsiConsoleTheme.Literate)
                .CreateLogger();
            await new HostBuilder()
                .ConfigureLogging(builder => builder.SetMinimumLevel(LogLevel.Debug).ClearProviders().AddSerilog(Log.Logger))
                .ConfigureAppConfiguration(builder => builder.AddJsonFile("application.json"))
                .ConfigureServices(x => x.AddHostedService<TelegramHostedService>()
                    .AddSingleton<Database>()
                    .AddSingleton<ParkingLottery>()
                    .AddSingleton<ITelegramHandler, TelegramParkingHandler>())
                .RunConsoleAsync();
        }
    }
}
