//____________________________________________________________________________________________________________________________________
//
//  Copyright (C) 2024, Mariusz Postol LODZ POLAND.
//
//  To be in touch join the community by pressing the `Watch` button and get started commenting using the discussion panel at
//
//  https://github.com/mpostol/TP/discussions/182
//
//_____________________________________________________________________________________________________________________________________

using System.Diagnostics;
using System.Numerics;
using System.IO;
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
            private static readonly Lazy<Logger> _instance = new Lazy<Logger>(() =>
            {
                var logger = new Logger();
                logger.Log("Zainicjalizowano logger pomyślnie.", LogLevel.Debug);
                return logger;
            });
            public static Logger Instance => _instance.Value;

            private readonly StreamWriter _writer;
            private readonly object _lock = new();

            public Logger()
            {
                string? repoDir = FindRepoRoot(AppDomain.CurrentDomain.BaseDirectory);
                string logPath = Path.Combine(repoDir ?? AppDomain.CurrentDomain.BaseDirectory, "log.txt");
                _writer = new StreamWriter(logPath, append: true) { AutoFlush = true };
                Log($"Logger initialized, log file: {logPath}", LogLevel.Debug);
            }

            private static string? FindRepoRoot(string startDir)
            {
                var dir = new DirectoryInfo(startDir);
                while (dir != null)
                {
                    if (dir.GetDirectories(".git").Any() || dir.GetFiles("*.sln").Any())
                        return dir.FullName;
                    dir = dir.Parent;
                }
                return null;
            }

            public void Log(string message, LogLevel level = LogLevel.Info)
            {
                var logEntry = $"[{level}] {DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}";
                lock (_lock)
                {
                    _writer.WriteLine(logEntry);
                }
            }

            public void Log(Data.IBall ball, string message, LogLevel level = LogLevel.Info)
            {
                if (ball == null)
                    throw new ArgumentNullException(nameof(ball));
                Log($"Kula na pozycji {ball.Position.x}, {ball.Position.y} o prędkości {ball.Velocity.x}, {ball.Velocity.y}: {message}", level);
            }

            public void Log(Data.IBall ball, string message)
            {
                Log(ball, message, LogLevel.Info);
            }

            public void Dispose()
            {
                _writer?.Dispose();
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