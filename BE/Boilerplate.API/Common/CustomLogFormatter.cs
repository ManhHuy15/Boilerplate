using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging.Console;

namespace Boilerplate.API.Common;

public sealed class CustomLogFormatter : ConsoleFormatter
{
    public const string FormatterName = "customLogFormatter";

    public CustomLogFormatter() : base(FormatterName)
    {
    }

    public override void Write<TState>(
        in LogEntry<TState> logEntry,
        IExternalScopeProvider? scopeProvider,
        TextWriter textWriter)
    {
        string? message = logEntry.Formatter?.Invoke(logEntry.State, logEntry.Exception);
        if (message == null)
            return;

        var category = logEntry.Category ?? "";
        var shortName = category;
        var lastDot = category.LastIndexOf('.');
        if (lastDot >= 0 && lastDot < category.Length - 1)
        {
            shortName = category[(lastDot + 1)..];
        }

        var header = $"[{shortName}]";
        
        var logLevel = logEntry.LogLevel;
        var (levelText, levelColor) = logLevel switch
        {
            LogLevel.Trace => ("TRCE", ConsoleColor.DarkGray),
            LogLevel.Debug => ("DBUG", ConsoleColor.Gray),
            LogLevel.Information => ("INFO", ConsoleColor.Green),
            LogLevel.Warning => ("WARN", ConsoleColor.Yellow),
            LogLevel.Error => ("EROR", ConsoleColor.Red),
            LogLevel.Critical => ("CRIT", ConsoleColor.DarkRed),
            _ => ("????", ConsoleColor.White)
        };

        var ansiColor = GetAnsiColor(levelColor);

        textWriter.Write($"\x1b[{ansiColor}m{levelText}\x1b[0m ");
        textWriter.Write($"\x1b[38;5;14m{header}\x1b[0m: ");
        textWriter.WriteLine(message);

        if (logEntry.Exception != null)
        {
            textWriter.WriteLine($"\x1b[{GetAnsiColor(ConsoleColor.Red)}m{logEntry.Exception}\x1b[0m");
        }
    }

    private static string GetAnsiColor(ConsoleColor color)
    {
        return color switch
        {
            ConsoleColor.DarkGray => "90",
            ConsoleColor.Gray => "37",
            ConsoleColor.Green => "32",
            ConsoleColor.Yellow => "33",
            ConsoleColor.Red => "31",
            ConsoleColor.DarkRed => "91",
            _ => "0"
        };
    }
}
