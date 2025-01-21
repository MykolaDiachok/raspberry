using Microsoft.Extensions.Logging;
using Serilog;

namespace raspberry;

public class Program
{
    static void Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console()
            .CreateLogger();

        var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddSerilog();
        });

        var logger = loggerFactory.CreateLogger<Program>();

        logger.LogInformation("Application starting...");
        
        var robotLight = new RobotLight();
        robotLight.Run();
        logger.LogInformation("Starting Breath mode...");
        
        robotLight.Breath(70, 70, 255);
        Thread.Sleep(5000);  
        robotLight.Pause();
        robotLight.FrontLight("off");
        
        logger.LogInformation("Starting Police mode...");
        Thread.Sleep(5000);
        robotLight.Police();
        Thread.Sleep(5000);
        robotLight.FrontLight("off");
        logger.LogInformation("Application ending...");
    }
}