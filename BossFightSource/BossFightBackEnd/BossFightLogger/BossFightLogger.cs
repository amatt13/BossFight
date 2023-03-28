using Microsoft.Extensions.Logging;
using System;
using System.IO;

namespace BossFight.BossFightBackEnd.BossFightLogger
{
    public class BossFightLoggerProvider : ILoggerProvider
    {
        private readonly string _filePath;

        public BossFightLoggerProvider(string filePath)
        {
            _filePath = filePath;
        }

        public ILogger CreateLogger(string categoryName)
        {
            return new FileLogger(_filePath);
        }

        public void Dispose() { }

        private class FileLogger : ILogger
        {
            private readonly string _filePath;

            public FileLogger(string filePath)
            {
                _filePath = filePath;
            }

            IDisposable ILogger.BeginScope<TState>(TState state)
            {
                return null;
            }


            public bool IsEnabled(LogLevel logLevel)
            {
                return true;
            }

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            {
                if (formatter == null)
                {
                    throw new ArgumentNullException(nameof(formatter));
                }

                if (!IsEnabled(logLevel))
                {
                    return;
                }

                string message = formatter(state, exception);
                if (string.IsNullOrEmpty(message))
                {
                    return;
                }

                using StreamWriter writer = new(_filePath, true);
                writer.WriteLine($"[{DateTime.Now}] [{logLevel}] {message}");
            }
        }
    }
}