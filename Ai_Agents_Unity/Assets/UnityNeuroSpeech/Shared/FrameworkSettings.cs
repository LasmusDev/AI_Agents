using UnityNeuroSpeech.Utils;

namespace UnityNeuroSpeech.Shared
{
    [System.Serializable]
    public struct FrameworkSettings
    {
        public LogLevel logLevel;
        public string ollamaURI;

        public FrameworkSettings(LogLevel logLevel, string ollamaURI)
        {
            this.logLevel = logLevel;
            this.ollamaURI = ollamaURI;
        }

        public static implicit operator bool(FrameworkSettings? data)
        {
            return data.HasValue && !string.IsNullOrEmpty(data.Value.ollamaURI);
        }
    }
}