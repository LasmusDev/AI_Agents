using UnityEngine;

namespace UnityNeuroSpeech.Utils
{
    public enum LogLevel
    {
        None,
        Error,
        All
    }

    public static class LogUtils
    {
        public static LogLevel logLevel;

        public static void LogMessage(string msg)
        {
            if (logLevel == LogLevel.All) Debug.Log($"[UnityNeuroSpeech] {msg}");
        }

        public static void LogError(string msg)
        {
            if (logLevel == LogLevel.Error || logLevel == LogLevel.All) Debug.LogError($"[UnityNeuroSpeech] {msg}");
        }
    }
}
