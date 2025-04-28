using BepInEx.Logging;

namespace ItemBundles
{
    internal static class DebugLogger
    {
        public static ManualLogSource ManualLogSource { get; private set; }

        public static void Init(ManualLogSource manualLogSource)
        {
            DebugLogger.ManualLogSource = manualLogSource;
        }
        
        public static void Log(LogLevel level, object data, bool debugOnly = false)
        {
            if ( debugOnly && !ItemBundles.Instance.config_debugLogging.Value )
            {
                return;
            }

            ManualLogSource.Log(level, data);
        }

        public static void LogFatal(object data, bool debugOnly = false)
        {
            DebugLogger.Log(LogLevel.Fatal, data, debugOnly);
        }

        public static void LogError(object data, bool debugOnly = false)
        {
            DebugLogger.Log(LogLevel.Error, data, debugOnly);
        }

        public static void LogWarning(object data, bool debugOnly = false)
        {
            DebugLogger.Log(LogLevel.Warning, data, debugOnly);
        }

        public static void LogMessage(object data, bool debugOnly = false)
        {
            DebugLogger.Log(LogLevel.Message, data, debugOnly);
        }

        public static void LogInfo(object data, bool debugOnly = false)
        {
            DebugLogger.Log(LogLevel.Info, data, debugOnly);
        }

        public static void LogDebug(object data, bool debugOnly = false)
        {
            DebugLogger.Log(LogLevel.Debug, data, debugOnly);
        }
    }
}
