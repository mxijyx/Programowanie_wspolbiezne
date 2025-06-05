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
using System.Threading;
using TP.ConcurrentProgramming.Data;


namespace TP.ConcurrentProgramming.BusinessLogic
{
    internal class Ball : IBall
    {
        private readonly Data.IBall _dataBall;
        private readonly List<Ball> _allBalls;
        private static readonly object _globalCollisionLock = new object();
        private ILogger logger;

        public Ball(Data.IBall dataBall, double tableWidth, double tableHeight, double tableBorder, List<Ball> allBalls, ILogger logger)
        {
            _dataBall = dataBall ?? throw new ArgumentNullException(nameof(dataBall));
            _allBalls = allBalls ?? throw new ArgumentNullException(nameof(allBalls));

            TableWidth = tableWidth;
            TableHeight = tableHeight;
            TableBorder = tableBorder;

            _dataBall.NewPositionNotification += OnPositionChanged;
            this.logger = ILogger.CreateDefaultLogger(); 

        }

        #region IBall Implementation

        public event EventHandler<IPosition>? NewPositionNotification;

        public double TableWidth { get; }
        public double TableHeight { get; }
        public double TableBorder { get; }

        #endregion

        #region Internal Methods

        internal void Stop()
        {
            _dataBall.Stop();
        }

        #endregion

        #region Event Handlers

        private void OnPositionChanged(object? sender, Data.IVector newPosition)
        {
            HandleWallCollisions(newPosition);

            HandleBallCollisions();

            NewPositionNotification?.Invoke(this, new Position(newPosition.x, newPosition.y));
        }

        #endregion

        #region Collision Detection and Response

        private void HandleWallCollisions(Data.IVector position)
        {
            double x = position.x;
            double y = position.y;
            double diameter = 10; // Assuming a fixed diameter for the ball
            double radius = 10/2; // Assuming diameter is 10, so radius is 5

            bool collisionOccurred = false;
            var currentVelocity = _dataBall.Velocity;
            double newVelX = currentVelocity.x;
            double newVelY = currentVelocity.y;

            // Left/Right collisions
            if (position.x >= TableWidth - diameter - 2 * TableBorder || position.x <= 0)
            {
                newVelX = -currentVelocity.x;
                collisionOccurred = true;
                logger.Log(position, currentVelocity, Thread.CurrentThread.ManagedThreadId, LogLevel.Info);
            }

            // Top/Bottom collisions
            if (position.y >= TableHeight - diameter - 2 * TableBorder || position.y <= 0)
            {
                newVelY = -currentVelocity.y;
                collisionOccurred = true;
                logger.Log(position, currentVelocity, Thread.CurrentThread.ManagedThreadId, LogLevel.Info);
            }

            if (collisionOccurred)
            {
                var newVelocity = new Data.Vector(newVelX, newVelY);
                _dataBall.SetVelocity(newVelocity);
            }
        }

        private void HandleBallCollisions()
        {
            lock (_globalCollisionLock)
            {
                foreach (Ball otherBall in _allBalls)
                {
                    if (otherBall == this)
                        continue;

                    if (DetectCollision(otherBall))
                    {
                        ResolveCollision(otherBall);
                    }
                }
            }
        }

        private bool DetectCollision(Ball otherBall)
        {
            var pos1 = _dataBall.Position;
            var pos2 = otherBall._dataBall.Position;

            double dx = pos1.x - pos2.x;
            double dy = pos1.y - pos2.y;
            double distance = Math.Sqrt(dx * dx + dy * dy);
            double minDistance = (10 + 10) / 2.0; // Assuming a fixed diameter of 10 for both balls

            return distance > 0 && distance <= minDistance;
        }

        private void ResolveCollision(Ball otherBall)
        {
            var pos1 = _dataBall.Position;
            var pos2 = otherBall._dataBall.Position;
            var vel1 = _dataBall.Velocity;
            var vel2 = otherBall._dataBall.Velocity;
            var diameter = 10; // Assuming a fixed diameter for the ball

            double dx = pos1.x - pos2.x;
            double dy = pos1.y - pos2.y;
            double distance = Math.Sqrt(dx * dx + dy * dy);

            if (distance == 0) return;

            double nx = dx / distance;
            double ny = dy / distance;

            double relativeVelX = vel1.x - vel2.x;
            double relativeVelY = vel1.y - vel2.y;
            double velocityAlongNormal = relativeVelX * nx + relativeVelY * ny;

            if (velocityAlongNormal > 0) return;

            double mass1 = 5; //tbc
            double mass2 = 5; //tbc
            double impulse = 2 * velocityAlongNormal / (mass1 + mass2);

            double impulseX = impulse * nx;
            double impulseY = impulse * ny;

            var newVel1 = new Data.Vector(
                vel1.x - impulseX * mass2,
                vel1.y - impulseY * mass2
            );

            var newVel2 = new Data.Vector(
                vel2.x + impulseX * mass1,
                vel2.y + impulseY * mass1
            );

            _dataBall.SetVelocity(newVel1);
            otherBall._dataBall.SetVelocity(newVel2);

            double minDistance = (diameter+diameter) / 2.0;
            double overlap = minDistance - distance;

            if (overlap > 0)
            {
                double separationX = nx * overlap * 0.5;
                double separationY = ny * overlap * 0.5;
            }

            logger.Log(
                $"Ball collision resolved: " +
                $"Ball1[pos=({pos1.x:F2},{pos1.y:F2}), vel=({newVel1.x:F2},{newVel1.y:F2})] " +
                $"Ball2[pos=({pos2.x:F2},{pos2.y:F2}), vel=({newVel2.x:F2},{newVel2.y:F2})] " +
                $"distance={distance:F2}, minDistance={minDistance:F2}",
                LogLevel.Info
            );
        }

        #endregion
    }
}