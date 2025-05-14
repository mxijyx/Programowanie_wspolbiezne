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

    public override void Start(int numberOfBalls, Action<IPosition, IBall> upperLayerHandler)
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
        upperLayerHandler(new Position(pos.x, pos.y), logicBall);
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
            layerBellow.SetCanvasSize(width, height);
        }
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
          CheckWallCollision(ball);
        }
        await Task.Delay(10);
      }
    }

    private bool CheckCollision(Ball a, Ball b)
    {
      // Implementacja detekcji kolizji
      return false; //Not implemented
    }

    private void ResolveCollision(Ball a, Ball b)
    {
      // Obliczenia fizyczne dla zderzenia

    }

    private void CheckWallCollision(Ball ball)
    {
      // Logika odbijania od ścian
    }

    public void Dispose() => _isRunning = false;
  }
}