using Serilog;

namespace CandidateTesting.GabrielKobayashiBarboza.ConvertLog.Configuration
{
    public static class LogConfiguration
    {
        public static void ConfigLog()
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.File(@"C:\Temp\ConvertLog.log",
                    rollingInterval: RollingInterval.Day,
                    outputTemplate: "[{Timestamp: dd-MM-yyyy HH:mm:ss} [{Level:u3}]] {Message:lj}{NewLine}{Exception}")
                .WriteTo.Console(outputTemplate: "{Message:lj}{NewLine}{Exception}")
                .CreateLogger();
        }
    }
}