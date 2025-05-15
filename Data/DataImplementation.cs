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
using System.Collections.Concurrent;
using System.Diagnostics;

namespace TP.ConcurrentProgramming.Data
{
  internal class DataImplementation : DataAbstractAPI
  {
    #region ctor
    public DataImplementation()
    {
      //MoveTimer = new Timer(Move, null, TimeSpan.Zero, TimeSpan.FromMilliseconds(16)); //krok w terminologii programowania - przechodzenie z instrukcji do instrukcji 
                                                                                       // timer wywołuje Move sekwencyjnie, a Move też jest sekwencyjne -> czyli i timer i funkcja Move jest nieporzebna?
                                                                                       // cykl odświeżania musi zależeć od prędkość kuli - czas odświeżania musi być mniejszy dla szybszych kul-> to musi być przy getterze velocity -> dlatgeo timer jest bez sensu
                                                                                       // data musi pozostać abstrakcyjne 
                                                                                       //TODO: do usunięcia
      BoardWidth = 800;
      BoardHeight = 600;

    }

    #endregion ctor

    #region DataAbstractAPI

    public override void Start(int numberOfBalls, Action<IVector, IBall> upperLayerHandler)
    {
      var balls = CreateBalls(numberOfBalls, BoardWidth, BoardHeight);

      foreach (var ball in balls)
      {
        upperLayerHandler(ball.Position, ball);
      }
    }
    

    public override void SetCanvasSize(double width, double height)
    {
      if (BoardWidth <= 0 || BoardHeight <= 0)
      {
        BoardWidth = width;
        BoardHeight = height;
        return;
      }

      double scaleX = width / BoardWidth;
      double scaleY = height / BoardHeight;

      BoardWidth = width;
      BoardHeight = height;

      foreach (var ball in BallsList)
      {
        ball.ScalePosition(scaleX, scaleY);
      }
    }

    public override List<IBall> CreateBalls(int count, double boardWidth, double boardHeight, double minMass = 0.5, double maxMass = 2.0)
    {
      var random = new Random();
      var balls = new List<IBall>();

      for (int i = 0; i < count; i++)
      {
        // Losowanie masy z zakresu
        var mass = minMass + (maxMass - minMass) * random.NextDouble();

        var position = new Vector(
          random.NextDouble() * (boardWidth - 20), // 20 to minimalna średnica
          random.NextDouble() * (boardHeight - 20)
        );

        var velocity = new Vector(
          (random.NextDouble() - 0.5) * 5 + 0.5,
          (random.NextDouble() - 0.5) * 5 +0.5
        );

        var ball = new Ball(position, velocity, mass);
        balls.Add(ball);
        BallsList.Add(ball);

      }

      return balls;
    }
    #endregion DataAbstractAPI

    #region IDisposable

    protected virtual void Dispose(bool disposing)
    {
      if (!Disposed)
      {
        if (disposing)
        {
          //MoveTimer.Dispose();
          foreach (var ball in BallsList)
          {
            ball.Stop();
          }
          BallsList.Clear();
        }
        Disposed = true;
      }
      else
        throw new ObjectDisposedException(nameof(DataImplementation));
    }

    public override void Dispose()
    {
      // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
      Dispose(disposing: true);
      GC.SuppressFinalize(this);
    }

    #endregion IDisposable

    #region private

    //private bool disposedValue;
    private bool Disposed = false;

    private readonly Timer MoveTimer;
    private Random RandomGenerator = new();
    private readonly ConcurrentBag<Ball> BallsList = new();

    public override double BoardWidth { get; set; } = 800;
    public override double BoardHeight { get; set; } = 600; //TODO: check if these fit the layer 


    #endregion private

    #region TestingInfrastructure

    [Conditional("DEBUG")]
    internal void CheckBallsList(Action<IEnumerable<IBall>> returnBallsList)
    {
      returnBallsList(BallsList);
    }

    [Conditional("DEBUG")]
    internal void CheckNumberOfBalls(Action<int> returnNumberOfBalls)
    {
      returnNumberOfBalls(BallsList.Count);
    }

    [Conditional("DEBUG")]
    internal void CheckObjectDisposed(Action<bool> returnInstanceDisposed)
    {
      returnInstanceDisposed(Disposed);
    }

    #endregion TestingInfrastructure
  }
}