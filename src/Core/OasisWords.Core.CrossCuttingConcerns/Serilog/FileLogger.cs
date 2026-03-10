using Serilog;
using Serilog.Core;

namespace OasisWords.Core.CrossCuttingConcerns.Serilog;

public abstract class LoggerServiceBase
{
    protected Logger? Logger { get; set; }

    public void Verbose(string message) => Logger?.Verbose(message);
    public void Debug(string message) => Logger?.Debug(message);
    public void Information(string message) => Logger?.Information(message);
    public void Warning(string message) => Logger?.Warning(message);
    public void Error(string message) => Logger?.Error(message);
    public void Fatal(string message) => Logger?.Fatal(message);
}

public class FileLogger : LoggerServiceBase
{
    public FileLogger(string logFilePath = "logs/log.txt")
    {
        Logger = new LoggerConfiguration()
            .WriteTo.File(
                logFilePath,
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 7,
                outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level}] {Message:lj}{NewLine}{Exception}")
            .CreateLogger();
    }
}
