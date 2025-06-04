//____________________________________________________________________________________________________________________________________
//
//  Copyright (C) 2024, Mariusz Postol LODZ POLAND.
//
//  To be in touch join the community by pressing the `Watch` button and get started commenting using the discussion panel at
//
//  https://github.com/mpostol/TP/discussions/182
//
//_____________________________________________________________________________________________________________________________________

using System.Collections.Concurrent;
using System.Diagnostics;
using UnderneathLayerAPI = TP.ConcurrentProgramming.Data.DataAbstractAPI;

namespace TP.ConcurrentProgramming.BusinessLogic
{
    internal class BusinessLogicImplementation : BusinessLogicAbstractAPI
    {
        #region Constructor

        public BusinessLogicImplementation() : this(null)
        {
        }

        internal BusinessLogicImplementation(UnderneathLayerAPI? underneathLayer)
        {
            _layerBellow = underneathLayer ?? UnderneathLayerAPI.GetDataLayer();
        }

        #endregion

        #region BusinessLogicAbstractAPI Implementation

        public override void Dispose()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(BusinessLogicImplementation));

            foreach (var ball in _ballList)
            {
                ball.Stop();
            }

            _layerBellow.Dispose();

            Logger.Instance.Dispose();

            _disposed = true;
        }

        public override void Start(int numberOfBalls, Action<IPosition, IBall> upperLayerHandler, double width, double height, double border)
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(BusinessLogicImplementation));
            if (upperLayerHandler == null)
                throw new ArgumentNullException(nameof(upperLayerHandler));

            Logger.Instance.Log($"Starting simulation with {numberOfBalls} balls", LogLevel.Info);

            _layerBellow.Start(numberOfBalls, (startingPosition, dataBall) =>
            {
                var ball = new Ball(dataBall, width, height, border, _ballList);
                _ballList.Add(ball);

                upperLayerHandler(new Position(startingPosition.x, startingPosition.y), ball);
            });
        }

        #endregion

        #region Private Fields

        private bool _disposed = false;
        private readonly List<Ball> _ballList = new List<Ball>();
        private readonly UnderneathLayerAPI _layerBellow;

        #endregion

        #region Testing Infrastructure

        [Conditional("DEBUG")]
        internal void CheckObjectDisposed(Action<bool> returnInstanceDisposed)
        {
            returnInstanceDisposed(_disposed);
        }

        #endregion
    }

    #region Logger Implementation

    internal class Logger : ILogger, IDisposable
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
            try
            {
                using var writer = new StreamWriter(_logFilePath, append: true) { AutoFlush = true };

                while (_isRunning || !_logQueue.IsCompleted)
                {
                    try
                    {
                        if (_logQueue.TryTake(out var entry, TimeSpan.FromSeconds(1)))
                        {
                            writer.WriteLine(FormatLogEntry(entry));
                        }
                    }
                    catch (InvalidOperationException)
                    {
                        // Kolekcja została zamknięta, przerywamy przetwarzanie
                        break;
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"LOG PROCESSING ERROR: {ex.Message}");
                    }
                }

                // Spłucz pozostałe wpisy w kolejce
                while (_logQueue.TryTake(out var remainingEntry))
                {
                    writer.WriteLine(FormatLogEntry(remainingEntry));
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"CRITICAL LOG ERROR: {ex.Message}");
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

        public void Log(string message, LogLevel level = LogLevel.Info)
        {
            EnqueueLog(message, level);
        }

        public void Log(Data.IBall ball, string message, LogLevel level = LogLevel.Info)
        {
            if (ball == null)
            {
                EnqueueLog($"[ERROR] Ball is null: {message}", LogLevel.Critical);
                return;
            }

            try
            {
                var position = ball.Position;
                var velocity = ball.Velocity;

                EnqueueLog(
                    $"Ball[pos=({position.x:F2},{position.y:F2}), " +
                    $"vel=({velocity.x:F2},{velocity.y:F2}), " +
                    $"d={ball.Diameter:F2}]: {message}",
                    level
                );
            }
            catch (Exception ex)
            {
                EnqueueLog($"[ERROR] Exception logging ball info: {ex.Message}", LogLevel.Error);
            }
        }

        public void Log(Data.IBall ball, string message)
        {
            Log(ball, message, LogLevel.Info);
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

        private class LogEntry
        {
            public DateTime Timestamp { get; set; }
            public LogLevel Level { get; set; }
            public string Message { get; set; } = string.Empty;
        }
    }

    internal enum LogLevel
    {
        Trace,
        Debug,
        Info,
        Warning,
        Error,
        Critical
    }

    #endregion
}