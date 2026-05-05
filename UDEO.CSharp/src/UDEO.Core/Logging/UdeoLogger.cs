using System.Collections.Concurrent;
using System.Text.Json;

namespace UDEO.Core.Logging;

/// <summary>
/// Structured logger with console colors and file output.
/// Replaces the PowerShell UDEOLogger class.
/// </summary>
public sealed class UdeoLogger
{
    private static readonly Lazy<UdeoLogger> _instance = new(() => new UdeoLogger());
    public static UdeoLogger Instance => _instance.Value;

    private static readonly Dictionary<string, int> LevelWeights = new()
    {
        ["Trace"] = 0, ["Debug"] = 1, ["Info"] = 2, ["Warn"] = 3, ["Error"] = 4
    };

    private string _logLevel = "Info";
    private string? _logFile;
    private bool _quiet;
    private bool _useColors = true;
    private readonly object _fileLock = new();

    private UdeoLogger() { }

    public void Configure(string? level = null, string? logFile = null, bool? quiet = null, bool? useColors = null)
    {
        if (level != null) _logLevel = level;
        if (logFile != null) _logFile = logFile;
        if (quiet.HasValue) _quiet = quiet.Value;
        if (useColors.HasValue) _useColors = useColors.Value;

        // Ensure log directory exists
        if (_logFile != null)
        {
            var dir = Path.GetDirectoryName(_logFile);
            if (dir != null && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);
        }
    }

    public void Trace(string message, object? data = null) => Write("Trace", message, data);
    public void Debug(string message, object? data = null) => Write("Debug", message, data);
    public void Info(string message, object? data = null) => Write("Info", message, data);
    public void Warn(string message, object? data = null) => Write("Warn", message, data);
    public void Error(string message, object? data = null) => Write("Error", message, data);

    private void Write(string level, string message, object? data)
    {
        if (!LevelWeights.TryGetValue(level, out var msgWeight) ||
            !LevelWeights.TryGetValue(_logLevel, out var currentWeight))
            return;

        if (msgWeight < currentWeight) return;

        var ts = DateTime.UtcNow.ToString("HH:mm:ss.fff");
        var line = $"[{ts}] [{level}] {message}";

        if (!_quiet)
        {
            if (_useColors && level is "Error" or "Warn" or "Info" or "Debug")
            {
                var color = level switch
                {
                    "Error" => ConsoleColor.Red,
                    "Warn" => ConsoleColor.Yellow,
                    "Info" => ConsoleColor.Cyan,
                    "Debug" => ConsoleColor.DarkGray,
                    _ => ConsoleColor.White
                };

                WriteColored(line, color);
            }
            else
            {
                Console.WriteLine(line);
            }

            if (data != null)
            {
                try
                {
                    var json = JsonSerializer.Serialize(data, new JsonSerializerOptions
                    {
                        WriteIndented = false,
                        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
                    });
                    WriteColored($"  {json}", ConsoleColor.DarkGray);
                }
                catch { }
            }
        }

        // File output
        if (_logFile != null)
        {
            lock (_fileLock)
            {
                try
                {
                    File.AppendAllText(_logFile, line + Environment.NewLine);
                }
                catch { }
            }
        }
    }

    private static void WriteColored(string text, ConsoleColor color)
    {
        var original = Console.ForegroundColor;
        Console.ForegroundColor = color;
        Console.WriteLine(text);
        Console.ForegroundColor = original;
    }
}
