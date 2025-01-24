using CielaDocs.AdminPanel;
using CielaDocs.AdminPanel.Helper;
using CielaDocs.Domain.Entities;

using Serilog;
using Serilog.Events;
using Serilog.Exceptions;
using Serilog.Formatting.Compact;

using System.Reflection;
namespace CielaDocs.WebAdminPanel
{
    public class Program
{
    public static int Main(string[] args)
    {
        var name = Assembly.GetExecutingAssembly().GetName();
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .Enrich.FromLogContext()
            .Enrich.WithExceptionDetails()
            .Enrich.WithMachineName()
            .Enrich.WithProperty("Assembly", $"{name.Name}")
            .Enrich.WithProperty("Assembly", $"{name.Version}")
            .WriteTo.File(
                    new CompactJsonFormatter(),
                    Environment.CurrentDirectory + @"/Logs/log.json",
                    rollingInterval: RollingInterval.Day,
                    restrictedToMinimumLevel: LogEventLevel.Error)
            .WriteTo.Console()
        .CreateLogger();

        try
        {
            Log.Information("Starting host");
                var host = CreateHostBuilder(args).Build();

                // Initialize GlobalConfig with the app's configuration
                GlobalConfig.Initialize(host.Services.GetRequiredService<IConfiguration>());

                host.Run();
                return 0;
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Host terminated unexpectedly");
            return 1;
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
         .UseSerilog()
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>();
            }).ConfigureAppConfiguration((context, config) =>
            {
                config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            })
            .ConfigureServices((context, services) =>
            {
                // Register services here if needed
            });
    }
}
