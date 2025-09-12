using BepInEx.Logging;

namespace LethalHUD;
internal class Loggers
{
    private static void Log(LogLevel logLevel, object data)
    {
        Plugins.Logger.Log(logLevel, data);
    }

    internal static void Info(object data)
    {
        Log(LogLevel.Info, data);
    }

    internal static void Debug(object data)
    {
        Log(LogLevel.Debug, data);
    }

    internal static void Message(object data)
    {
        Log(LogLevel.Message, data);
    }
    internal static void Warning(object data)
    {
        Log(LogLevel.Warning, data);
    }
    internal static void Error(object data)
    {
        Log(LogLevel.Error, data);
    }
    internal static void Fatal(object data)
    {
        Log(LogLevel.Fatal, data);
    }
}