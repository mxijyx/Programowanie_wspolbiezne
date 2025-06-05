using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TP.ConcurrentProgramming.Data
{
    internal class Logger : ILogger
    {
        private readonly BlockingCollection<LogEntry> _logQueue = new BlockingCollection<LogEntry>();
        private static readonly Lazy<Logger> _instance = new Lazy<Logger>(() => new Logger());
        public static Logger Instance => _instance.Value;

        private readonly Thread _processingThread;
        private readonly string _logFilePath;
        private volatile bool _isRunning = true;

        public string LogPath => _logFilePath;

        private Logger()
        {
            string? repoRoot = FindRepoRoot(AppDomain.CurrentDomain.BaseDirectory);
            string logsDir = Path.Combine(repoRoot ?? AppDomain.CurrentDomain.BaseDirectory, "logs");
            Directory.CreateDirectory(logsDir);

            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            _logFilePath = Path.Combine(logsDir, $"log_{timestamp}.txt");

            _processingThread = new Thread(ProcessLogQueue)
            {
                Name = "LogProcessorThread",
                IsBackground = true,
                Priority = ThreadPriority.BelowNormal
            };
            _processingThread.Start();

            // First log entry
            EnqueueLog("Logger initialized", LogLevel.Debug);
        }

        private static string? FindRepoRoot(string startDir)
        {
            var dir = new DirectoryInfo(startDir);
            while (dir != null)
            {
                if (Directory.Exists(Path.Combine(dir.FullName, ".git")) ||
                    dir.GetFiles("*.sln").Any())
                    return dir.FullName;

                dir = dir.Parent;
            }
            return null;
        }

        private void ProcessLogQueue()
        {
                using System.IO.StreamWriter writer = new StreamWriter(_logFilePath, append: true) { AutoFlush = true };

                while (_isRunning || !_logQueue.IsCompleted)
                {
                        if (_logQueue.TryTake(out LogEntry entry, TimeSpan.FromSeconds(1)))
                        {
                            writer.WriteLine(FormatLogEntry(entry)); //POLIMORPHISM!!! TODO: Use polymorphism
                }

                }

                while (_logQueue.TryTake(out LogEntry remainingEntry))
                {
                    writer.WriteLine(FormatLogEntry(remainingEntry));
                }
        }

        private void EnqueueLog(string message, LogLevel level)
        {
            var entry = new LogEntry
            {
                Timestamp = DateTime.Now,
                Level = level,
                Message = message
            };

            if (!_logQueue.TryAdd(entry, millisecondsTimeout: 10))
            {
                Debug.WriteLine("LOG QUEUE FULL - ENTRY DROPPED");
            }
        }

        private string FormatLogEntry(LogEntry entry)
        {
            return $"[{entry.Level}] {entry.Timestamp:yyyy-MM-dd HH:mm:ss.fff} - {entry.Message}";
        }

        public void Log(IVector position, IVector velocity, int threadID,  LogLevel level = LogLevel.Info)
        {
            if (position == null || velocity == null || _logQueue.IsCompleted)
            {
                EnqueueLog($"[ERROR] Ball is null", LogLevel.Critical);
                return;
            }

                EnqueueLog(
                    $"Ball[pos=({position.x:F2},{position.y:F2}), " +
                    $"vel=({velocity.x:F2},{velocity.y:F2}), ",
                    level
                );
            
        }

        public void Dispose()
        {
            _isRunning = false;
            EnqueueLog("Simulation ended successfully", LogLevel.Info);
            EnqueueLog("End of event log", LogLevel.Debug);

            _logQueue.CompleteAdding();

            if (!_processingThread.Join(TimeSpan.FromSeconds(3)))
            {
                Debug.WriteLine("Log thread did not terminate in time");
            }

            _logQueue.Dispose();
        }

        public void Log(string message, LogLevel level = LogLevel.Info)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                EnqueueLog("[ERROR] Log message is null or empty", LogLevel.Critical);
                return;
            }
            EnqueueLog(message, level);
        }

        private class LogEntry
        {
            public DateTime Timestamp { get; set; }
            public LogLevel Level { get; set; }
            public string Message { get; set; } = string.Empty;
        }
    }

    public enum LogLevel
    {
        Trace,
        Debug,
        Info,
        Warning,
        Error,
        Critical
    }
}
