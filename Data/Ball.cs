//____________________________________________________________________________________________________________________________________
//
//  Copyright (C) 2024, Mariusz Postol LODZ POLAND.
//
//  To be in touch join the community by pressing the `Watch` button and get started commenting using the discussion panel at
//
//  https://github.com/mpostol/TP/discussions/182
//
//_____________________________________________________________________________________________________________________________________

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace TP.ConcurrentProgramming.Data
{
    internal class Ball : IBall, IDisposable
    {
        #region Fields
        private Vector _velocity;
        private Vector _position;
        private int Id;

        private readonly Thread _ballThread;
        private volatile bool _isRunning = true;

        private DateTime _lastUpdateTime;

        #endregion

        #region Constructor

        internal Ball(Vector initialPosition, Vector initialVelocity, int id)
        {
            _position = new Vector(initialPosition.x, initialPosition.y);
            _velocity = new Vector(initialVelocity.x, initialVelocity.y);
            _lastUpdateTime = DateTime.UtcNow;
            this.Id = id;

            _ballThread = new Thread(ThreadLoop)
            {
                Name = $"BallThread_{GetHashCode():X}",
                IsBackground = true,
                Priority = ThreadPriority.AboveNormal
            };
            _ballThread.Start();
        }

        #endregion

        #region IBall Implementation

        public event EventHandler<IVector>? NewPositionNotification;

        public IVector Velocity => new Vector(_velocity.x, _velocity.y);

        public IVector Position => new Vector(_position.x, _position.y);

        int IBall.Id => Id;

        public void Stop()
        {
            _isRunning = false;
        }

        public void SetVelocity(IVector newVelocity)
        {
            if (newVelocity == null)
                throw new ArgumentNullException(nameof(newVelocity));

            _velocity = new Vector(newVelocity.x, newVelocity.y);
        }

        public int GetId()
        {
            return Id;
        }
        #endregion

        #region Real-Time Movement Loop

        private void ThreadLoop()
        {
            while (_isRunning)
            {
                try
                {
                    DateTime currentTime = DateTime.UtcNow;
                    double deltaTimeSeconds = (currentTime - _lastUpdateTime).TotalSeconds;
                    _lastUpdateTime = currentTime;

                    Move(deltaTimeSeconds);

                    int sleepMs = CalculateRefreshTime();
                    Thread.Sleep(sleepMs);
                }
                catch (ThreadInterruptedException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.Log($"ERROR in ThreadLoop: {ex.Message}", LogLevel.Error);
                }
            }
        }

        private void Move(double deltaTimeSeconds)
        {
            _position = new Vector(
                _position.x + (_velocity.x * deltaTimeSeconds),
                _position.y + (_velocity.y * deltaTimeSeconds)
            );

            _logger.Log($"Pos=({_position.x:F2},{_position.y:F2}); " +
                        $"Vel=({_velocity.x:F2},{_velocity.y:F2}); " +
                        $"dt={deltaTimeSeconds * 1000:F1}ms", LogLevel.Info);

            NewPositionNotification?.Invoke(this, new Vector(_position.x, _position.y));
        }

        #endregion

        #region Refresh Time Calculator

        private int CalculateRefreshTime()
        {
            double actualVelocity = Math.Sqrt(_velocity.x * _velocity.x + _velocity.y * _velocity.y);

            const int maxRefreshTime = 50;
            const int minRefreshTime = 10;

            double normalizedVelocity = Math.Clamp(actualVelocity / 100.0, 0.0, 1.0);

            return Math.Clamp(
                (int)(maxRefreshTime - normalizedVelocity * (maxRefreshTime - minRefreshTime)),
                minRefreshTime,
                maxRefreshTime
            );
        }

        #endregion

        #region Diagnostic Data Management
        private Logger _logger = Logger.Instance;

        #endregion

        #region IDisposable

        public void Dispose()
        {
            _isRunning = false;

            if (_ballThread.IsAlive && !_ballThread.Join(TimeSpan.FromSeconds(2)))
            {
                try
                {
                    _ballThread.Interrupt();
                    _ballThread.Join(TimeSpan.FromSeconds(1));
                }
                catch (ThreadStateException)
                {
                    // Wątek już jest zakończony, więc nic nie rób
                }
            }
        }

        #endregion
    }
}