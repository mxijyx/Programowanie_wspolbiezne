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

    public override void Start(int numberOfBalls, Action<IVector, IBall> upperLayerHandler)
    {
      if (Disposed)
        throw new ObjectDisposedException(nameof(BusinessLogicImplementation));
      if (upperLayerHandler == null)
        throw new ArgumentNullException(nameof(upperLayerHandler));

      layerBellow.Start(numberOfBalls, (pos, dataBall) =>
      {
        var logicBall = new Ball(dataBall);
        _collisionManager.RegisterBall(logicBall);
        _balls.TryAdd(dataBall, logicBall);
        upperLayerHandler(pos, logicBall);
      });
    }

    #endregion BusinessLogicAbstractAPI

    #region private

    private bool Disposed = false;

        private readonly UnderneathLayerAPI layerBellow;
    private readonly CollisionManager _collisionManager;
    private readonly ConcurrentDictionary<Data.IBall, Ball> _balls = new();
    
        #endregion private

        #region SetCanvasSize
        public override void SetCanvasSize(double width, double height)
        {
            if (Disposed)
                throw new ObjectDisposedException(nameof(BusinessLogicImplementation));
            layerBellow.SetCanvasSize(width, height);        }
        #endregion SetCanvasSize

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
          //CheckWallCollision(ball, boardWidth, boardHeight, borderThickness);
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

            if (distance < radiusSum)
            {
                return true;
            }
            // Implementacja detekcji kolizji
            return false; //Not implemented
    }

    private void ResolveCollision(Ball a, Ball b)
    {
            // Obliczenia fizyczne dla zderzenia
            var dx = a.Position.x - b.Position.x;
            var dy = a.Position.y - b.Position.y;
            var distance = Math.Sqrt(dx * dx + dy * dy);
            if(distance == 0) return; // Avoid division by zero

            var va = a.Velocity.x* dx + a.Velocity.y * dy;
            va = va / distance;
            var vb = b.Velocity.x * dx + b.Velocity.y * dy;
            vb = vb / distance;

            if (va <= vb) return; // No collision
            //else -> collision 
            var newVa = (va * (a.Diameter - b.Diameter) + 2 * b.Diameter * vb) / (a.Diameter + b.Diameter);
            var newVb = (vb * (b.Diameter - a.Diameter) + 2 * a.Diameter * va) / (a.Diameter + b.Diameter);
            double newAx = newVa * dx / distance;
            double newAy = newVa * dy / distance;
            double newBx = newVb * dx / distance;
            double newBy = newVb * dy / distance;
            a.SetVelocity(newAx, newAy);
            b.SetVelocity(newBx, newBy);
            a.SetPosition(a.Position.x + newAx, a.Position.y + newAy);
            b.SetPosition(b.Position.x + newBx, b.Position.y + newBy);

        }

        private void CheckWallCollision(Ball ball, double boardWidth, double boardHeight, double borderThickness)
        {
          lock (_collisionLock)
          {
            if (ball.Position.x <= 0 || ball.Position.x >= boardWidth - ball.Diameter - borderThickness)
            {
              ball.SetVelocity(-ball.Velocity.x, ball.Velocity.y);
            }

            if (ball.Position.y <= 0 || ball.Position.y >= boardHeight - ball.Diameter - borderThickness)
            {
              ball.SetVelocity(ball.Velocity.x, -ball.Velocity.y);
            }

            //notify()?
          }

        }

    public void Dispose() => _isRunning = false;
  }
}