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
            _collisionManager = new CollisionManager();
        }

        #endregion ctor

        #region BusinessLogicAbstractAPI

        public override void Dispose()
        {
            if (Disposed)
                throw new ObjectDisposedException(nameof(BusinessLogicImplementation));

            layerBellow.Dispose();
            _collisionManager.Dispose();
            Disposed = true;
        }

        public override void Start(int numberOfBalls, Action<IVector, IBall> upperLayerHandler, double tw, double th, double border)
        {
            if (Disposed)
                throw new ObjectDisposedException(nameof(BusinessLogicImplementation));
            if (upperLayerHandler == null)
                throw new ArgumentNullException(nameof(upperLayerHandler));

            layerBellow.Start(numberOfBalls, (pos, dataBall) =>
            {
                var logicBall = new Ball(dataBall, ballsList, tw, th, border);
                _collisionManager.RegisterBall(logicBall);
                _balls.TryAdd(dataBall, logicBall);
                upperLayerHandler(pos, logicBall);
            });
        }

        /*public override void SetCanvasSize(double width, double height)
        {
            if (Disposed)
                throw new ObjectDisposedException(nameof(BusinessLogicImplementation));

            layerBellow.SetCanvasSize(width, height);

            double borderThickness = 10.0; // Możesz zmienić lub pobrać dynamicznie
            _collisionManager.SetCanvasSize(width, height, borderThickness);
        }*/

        #endregion BusinessLogicAbstractAPI

        #region private

        private bool Disposed = false;

        private readonly UnderneathLayerAPI layerBellow;
        private readonly CollisionManager _collisionManager;
        private readonly ConcurrentDictionary<Data.IBall, Ball> _balls = new();

        #endregion private

        #region TestingInfrastructure

        [Conditional("DEBUG")]
        internal void CheckObjectDisposed(Action<bool> returnInstanceDisposed)
        {
            returnInstanceDisposed(Disposed);
        }

        #endregion TestingInfrastructure
    }

    internal class CollisionManager : IDisposable
    {
        private readonly ConcurrentBag<Ball> _balls = new();
        private readonly object _collisionLock = new();
        private bool _isRunning = true;

        private double _boardWidth;
        private double _boardHeight;
        private double _borderThickness;

        public void SetCanvasSize(double width, double height, double borderThickness)
        {
            _boardWidth = width;
            _boardHeight = height;
            _borderThickness = borderThickness;
        }

        public void RegisterBall(Ball ball)
        {
            _balls.Add(ball);
            Task.Run(() => DetectCollisions(ball));
        }

        private async void DetectCollisions(Ball ball)
        {
            while (_isRunning)
            {
                lock (_collisionLock)
                {
                    foreach (var other in _balls)
                    {
                        if (ball != other && CheckCollision(ball, other))
                        {
                            ResolveCollision(ball, other);
                        }
                    }

                    CheckWallCollision(ball);
                }

                await Task.Delay(10);
            }
        }

        private bool CheckCollision(Ball a, Ball b)
        {
            var dx = a.Position.x - b.Position.x;
            var dy = a.Position.y - b.Position.y;
            var distance = Math.Sqrt(dx * dx + dy * dy);
            var radiusSum = (a.Diameter + b.Diameter) / 2;

            return distance < radiusSum;
        }

        private void ResolveCollision(Ball a, Ball b)
        {
            var dx = b.Position.x - a.Position.x;
            var dy = b.Position.y - a.Position.y;
            var distance = Math.Sqrt(dx * dx + dy * dy);

            if (distance == 0) return; // unikaj dzielenia przez zero

            // Jednostkowy wektor kolizji
            double nx = dx / distance;
            double ny = dy / distance;

            // Wektory prędkości
            double vaX = a.Velocity.x;
            double vaY = a.Velocity.y;
            double vbX = b.Velocity.x;
            double vbY = b.Velocity.y;

            // Składowe prędkości wzdłuż kierunku zderzenia (iloczyn skalarny)
            double vaNormal = vaX * nx + vaY * ny;
            double vbNormal = vbX * nx + vbY * ny;

            if (vaNormal >= vbNormal)
                return; // kulki się oddalają – nie zderzają się

            // Masa ~ średnica (zakładamy proporcjonalność)
            double ma = a.Diameter;
            double mb = b.Diameter;

            // Nowe prędkości normalne po zderzeniu (elastyczne zderzenie)
            double vaNormalAfter = (vaNormal * (ma - mb) + 2 * mb * vbNormal) / (ma + mb);
            double vbNormalAfter = (vbNormal * (mb - ma) + 2 * ma * vaNormal) / (ma + mb);

            // Zmiana prędkości wzdłuż kierunku kolizji
            double dVaNormal = vaNormalAfter - vaNormal;
            double dVbNormal = vbNormalAfter - vbNormal;

            // Dodaj zmianę tylko w kierunku kolizji (reszta zostaje)
            a.SetVelocity(vaX + dVaNormal * nx, vaY + dVaNormal * ny);
            b.SetVelocity(vbX + dVbNormal * nx, vbY + dVbNormal * ny);

            // Minimalne przesunięcie, żeby uniknąć "klejenia się"
            double overlap = 0.5 * (a.Diameter / 2 + b.Diameter / 2 - distance + 0.1);
            a.SetPosition(a.Position.x - overlap * nx, a.Position.y - overlap * ny);
            b.SetPosition(b.Position.x + overlap * nx, b.Position.y + overlap * ny);
        }


        private void CheckWallCollision(Ball ball)
        {
            if (ball.Position.x <= 0 || ball.Position.x >= _boardWidth - ball.Diameter - _borderThickness)
            {
                ball.SetVelocity(-ball.Velocity.x, ball.Velocity.y);
            }

            if (ball.Position.y <= 0 || ball.Position.y >= _boardHeight - ball.Diameter - _borderThickness)
            {
                ball.SetVelocity(ball.Velocity.x, -ball.Velocity.y);
            }
        }

        public void Dispose()
        {
            _isRunning = false;
        }
    }
}
