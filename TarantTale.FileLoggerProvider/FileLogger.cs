using System;
using System.IO;
using System.Diagnostics.CodeAnalysis;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.FileProviders;
using System.Xml.Linq;


namespace TarantTale.FileLoggerProvider
{
    public class FileLogger : ILogger
    {
        protected readonly FileLoggerProvider _loggerFileProvider;

        public FileLogger([NotNull] FileLoggerProvider loggerFileProvider)
        {
            _loggerFileProvider = loggerFileProvider;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return null;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return logLevel != LogLevel.None;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (!IsEnabled(logLevel))
            {
                return;
            }

            var path = _loggerFileProvider.Options.FolderPath;
            var name = _loggerFileProvider.Options.FilePath.Replace("{date}", DateTimeOffset.UtcNow.ToString("yyyyMMdd"));

            var logRecord = string.Format(
                "{0} [{1}] {2} {3}", 
                "[" + DateTimeOffset.UtcNow.ToString("yyyy-MM-dd HH:mm:ss+00:00") + "]", 
                logLevel.ToString(), 
                formatter(state, exception), 
                exception != null ? exception.StackTrace : ""
            );

            lock (locker)
            {
                var fullfilepath = Path.Combine(path, name);

                using (FileStream file = new FileStream(fullfilepath, FileMode.Append, FileAccess.Write, FileShare.Read))
                using (StreamWriter streamWriter = new StreamWriter(file))
                {
                    streamWriter.WriteLine(logRecord);
                    streamWriter.Close();
                }
            }
        }

#pragma warning disable IDE0044 // Добавить модификатор только для чтения
        private static object locker = new object();
#pragma warning restore IDE0044 // Добавить модификатор только для чтения
    }
}
