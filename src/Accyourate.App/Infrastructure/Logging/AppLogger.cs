namespace Accyourate.App.Infrastructure.Logging;

public sealed class AppLogger
{
    private readonly string _logDirectory;

    public AppLogger()
    {
        _logDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
            "Accyourate Enterprise X",
            "logs");

        Directory.CreateDirectory(_logDirectory);
    }

    public void Info(string message) => Write("INFO", message);

    public void Warning(string message) => Write("WARN", message);

    public void Error(string message, Exception? exception = null)
    {
        var text = exception is null ? message : $"{message}{Environment.NewLine}{exception}";
        Write("ERROR", text);
    }

    private void Write(string level, string message)
    {
        var path = Path.Combine(_logDirectory, $"accyourate_{DateTime.Now:yyyyMMdd}.log");
        var line = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [{level}] {message}{Environment.NewLine}";
        File.AppendAllText(path, line);
    }
}
