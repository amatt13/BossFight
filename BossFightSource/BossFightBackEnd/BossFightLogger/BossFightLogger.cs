using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace BossFight.BossFightBackEnd.BossFightLogger
{
    public class BossFightLoggerProvider : ILoggerProvider
    {
        private readonly string _filePath;
        private readonly Channel<string> _logChannel;
        private readonly CancellationTokenSource _cts = new();
        private readonly Task _backgroundWriter;
        private readonly LogLevel _minLogLevel;

        public BossFightLoggerProvider(string filePath, LogLevel minLogLevel = LogLevel.Debug)
        {
            _filePath = filePath;
            _minLogLevel = minLogLevel;

            _logChannel = Channel.CreateBounded<string>(new BoundedChannelOptions(1000)
            {
                FullMode = BoundedChannelFullMode.DropOldest
            });

            _backgroundWriter = Task.Run(WriteLogBackgroundAsync);
        }

        public ILogger CreateLogger(string categoryName)
        {
            return new FileLogger(_logChannel, _minLogLevel);
        }

        public void Dispose()
        {
            _cts.Cancel();
            _backgroundWriter.Wait();
        }

        private async Task WriteLogBackgroundAsync()
        {
            await foreach (var log in _logChannel.Reader.ReadAllAsync(_cts.Token))
            {
                await File.AppendAllTextAsync(_filePath, log + Environment.NewLine);
            }
        }

        private class FileLogger : ILogger
        {
            private readonly Channel<string> _channel;
            private readonly LogLevel _minLevel;

            public FileLogger(Channel<string> channel, LogLevel minLevel)
            {
                _channel = channel;
                _minLevel = minLevel;
            }

            public IDisposable BeginScope<TState>(TState state) => null!;

            public bool IsEnabled(LogLevel logLevel) => logLevel >= _minLevel;

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
            {
                if (!IsEnabled(logLevel) || formatter == null)
                    return;

                var message = formatter(state, exception);
                if (String.IsNullOrWhiteSpace(message))
                    return;

                if (exception != null)
                    message += Environment.NewLine + exception;

                var formatted = $"[{DateTime.Now:yyyy/MM/dd HH:mm:ss}] [{logLevel}] {message}";

                _channel.Writer.TryWrite(formatted); // Drop if full
            }
        }
    }
}
