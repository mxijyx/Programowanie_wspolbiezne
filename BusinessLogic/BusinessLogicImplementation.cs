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
using System.IO;
using System.Numerics;
using TP.ConcurrentProgramming.Data;
using UnderneathLayerAPI = TP.ConcurrentProgramming.Data.DataAbstractAPI;

namespace TP.ConcurrentProgramming.BusinessLogic
{
    internal class BusinessLogicImplementation : BusinessLogicAbstractAPI
    {
        #region ctor

        public BusinessLogicImplementation() : this(null)
        { }

        internal BusinessLogicImplementation(UnderneathLayerAPI? underneathLayer)
        {
            layerBellow = underneathLayer == null ? UnderneathLayerAPI.GetDataLayer() : underneathLayer;
        }

        #endregion ctor

        #region BusinessLogicAbstractAPI

        public override void Dispose()
        {
            if (Disposed)
                throw new ObjectDisposedException(nameof(BusinessLogicImplementation));
            foreach (var ball in ballList)
            {
                ball.Stop();
            }
            layerBellow.Dispose();
            Disposed = true;
        }

        public override void Start(int numberOfBalls, Action<IPosition, IBall> upperLayerHandler, double width, double height, double border)
        {
            Logger.Instance.Log($"Program rozpoczęto, liczba kul: {numberOfBalls}", LogLevel.Debug);

            if (Disposed)
                throw new ObjectDisposedException(nameof(BusinessLogicImplementation));
            if (upperLayerHandler == null)
                throw new ArgumentNullException(nameof(upperLayerHandler));
            layerBellow.Start(numberOfBalls, (startingPosition, databall) =>
            {
                var ball = new Ball(databall, width, height, border, ballList);
                ballList.Add(ball);
                upperLayerHandler(new Position(startingPosition.x, startingPosition.y), ball);
            });
        }


        #region private

        private bool Disposed = false;
        private List<Ball> ballList = new List<Ball>();

        private readonly UnderneathLayerAPI layerBellow;

        #endregion private

        #region TestingInfrastructure

        [Conditional("DEBUG")]
        internal void CheckObjectDisposed(Action<bool> returnInstanceDisposed)
        {
            returnInstanceDisposed(Disposed);
        }

        #endregion TestingInfrastructure
        #endregion BusinessLogicAbstractAPI
    }

    #region Logger
    internal class Logger : ILogger, IDisposable
    {
        private readonly BlockingCollection<string> _logQueue = new BlockingCollection<string>();
        private static readonly Lazy<Logger> _instance = new Lazy<Logger>(() => new Logger());
        public static Logger Instance => _instance.Value;

        private readonly Thread _processingThread;
        private readonly string _logFilePath;
        private volatile bool _isRunning = true;

        public string LogPath => _logFilePath;

        public Logger()
        {
            // Znajdź katalog repozytorium i utwórz folder logs
            string? repoRoot = FindRepoRoot(AppDomain.CurrentDomain.BaseDirectory);
            string logsDir = Path.Combine(repoRoot ?? AppDomain.CurrentDomain.BaseDirectory, "logs");
            Directory.CreateDirectory(logsDir);

            // Wygeneruj unikalną nazwę pliku
            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            _logFilePath = Path.Combine(logsDir, $"log_{timestamp}.txt");

            // Uruchom wątek przetwarzający
            _processingThread = new Thread(ProcessLogQueue)
            {
                Name = "LogProcessorThread",
                IsBackground = true,
                Priority = ThreadPriority.BelowNormal // Niski priorytet, aby nie wpływać na kulki
            };
            _processingThread.Start();

            // Pierwszy wpis w logu
            EnqueueLog($"[Info] Logger initialized at {DateTime.Now:HH:mm:ss.fff}", LogLevel.Info);
        }

        private static string? FindRepoRoot(string startDir)
        {
            var dir = new DirectoryInfo(startDir);
            while (dir != null)
            {
                if (Directory.Exists(Path.Combine(dir.FullName, ".git")))
                    return dir.FullName;

                if (dir.GetFiles("*.sln").Any())
                    return dir.FullName;

                dir = dir.Parent;
            }
            return null;
        }

        private void ProcessLogQueue()
        {
            using var writer = new StreamWriter(_logFilePath, append: true) { AutoFlush = true };

            while (_isRunning || !_logQueue.IsCompleted)
            {
                try
                {
                    // Oczekuj max 1s na wiadomość
                    if (_logQueue.TryTake(out var entry, TimeSpan.FromSeconds(1)))
                    {
                        writer.WriteLine(entry);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"LOG PROCESSING ERROR: {ex.Message}");
                }
            }

            // Opróżnij resztę kolejki przed zamknięciem
            while (_logQueue.TryTake(out var remainingEntry))
            {
                writer.WriteLine(remainingEntry);
            }
        }

        private void EnqueueLog(string message, LogLevel level)
        {
            var entry = $"[{level}] {DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} - {message}";

            if (!_logQueue.TryAdd(entry, millisecondsTimeout: 50))
            {
                // Jeśli kolejka jest pełna, odrzuć wpis (nie blokuj wątku wywołującego)
                Debug.WriteLine("LOG QUEUE FULL - ENTRY DROPPED");
            }
        }

        public void Log(string message, LogLevel level = LogLevel.Info)
        {
            EnqueueLog(message, level);
        }

        public void Log(Data.IBall ball, string message, LogLevel level = LogLevel.Info)
        {
            if (ball == null)
            {
                EnqueueLog($"[Logger] Ball is null: {message}", LogLevel.Critical);
                throw new NullReferenceException("Ball cannot be null");
            }

            try
            {
                // Log without using ball.ID (since it does not exist)
                EnqueueLog(
                    $"Ball at ({ball.Position.x:F2}, {ball.Position.y:F2}) " +
                    $"Vel: ({ball.Velocity.x:F2}, {ball.Velocity.y:F2}) " +
                    $"Diameter: {ball.Diameter:F2}: {message}",
                    level
                );
            }
            catch (Exception ex)
            {
                EnqueueLog($"[Logger] Exception logging ball info: {ex.Message}", LogLevel.Error);
            }
        }

        public void Log(Data.IBall ball, string message)
        {
            Log(ball, message, LogLevel.Info);
        }

        public void Dispose()
        {
            _isRunning = false;
            EnqueueLog("Koniec dziennika zdarzeń", LogLevel.Debug);
            _logQueue.CompleteAdding();

            // Poczekaj maksymalnie 3s na zakończenie wątku
            if (!_processingThread.Join(TimeSpan.FromSeconds(3)))
            {
                Debug.WriteLine("Log thread did not terminate in time");
            }
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
    #endregion Logger
}