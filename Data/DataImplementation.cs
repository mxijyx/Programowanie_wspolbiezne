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
using System.Diagnostics;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace TP.ConcurrentProgramming.Data
{
  internal class DataImplementation : DataAbstractAPI
  {
    #region ctor

    public DataImplementation()
    {
      movingTask = Task.Run(async () =>
      {
        while (!cts.IsCancellationRequested)
        {
          double w = BoardWidth;
          double h = BoardHeight;
          double d = Ball.Diameter;
          foreach (var ball in BallsList)
          {
            ball.Move(d,w,h);
          }
          await Task.Delay(16, cts.Token).ConfigureAwait(false);
        }
      }, cts.Token); //krok w terminologii programowania - przechodzenie z instrukcji do instrukcji 
                     // timer wywołuje Move sekwencyjnie, a Move też jest sekwencyjne -> czyli i timer i funkcja Move jest nieporzebna?
                     // cykl odświeżania musi zależeć od prędkość kuli - czas odświeżania musi być mniejszy dla szybszych kul-> to musi być przy getterze velocity -> dlatgeo timer jest bez sensu
                     // data musi pozostać abstrakcyjne 

    }

    #endregion ctor

    #region DataAbstractAPI

    public override void Start(int numberOfBalls, Action<IVector, IBall> upperLayerHandler)
    {
      if (numberOfBalls <= 0)
      {
        throw new ArgumentOutOfRangeException(nameof(numberOfBalls));
      }
      var rand = new Random();
      for (int i = 0; i < numberOfBalls; i++)
      {
        var pos = new Vector(rand.NextDouble() * BoardWidth, rand.NextDouble() * BoardHeight);
        var vel = new Vector((rand.NextDouble() - 0.5) * 200 / 60, (rand.NextDouble() - 0.5) * 200 / 60);
        var ball = new Ball(pos, vel);
        BallsList.Add(ball);
        upperLayerHandler(pos, ball);
      }
    }

    #endregion DataAbstractAPI

    #region IDisposable

    protected virtual void Dispose(bool disposing)
    {
      if (!Disposed)
      {
        if (disposing)
        {
          cts.Cancel();
          try
          {
            movingTask.Wait();
          }
          catch (AggregateException ae)
          {
            ae.Handle(e => e is OperationCanceledException);
          }
          finally
          {
            BallsList.Clear();
            cts.Dispose();
          }
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

    public override IBall CreateBall(IVector position, IVector velocity) => new Ball(position, velocity); //TODO: to nie powinno być w Data - to jest w BusinessLogic - to jest tylko reprezentacja kuli, a nie kula jako taka
    public override IVector CreateVector(double x, double y) => new Vector(x, y); //TODO: to nie powinno być w Data - to jest w BusinessLogic - to jest tylko reprezentacja kuli, a nie kula jako taka
    
    #region private

    private bool Disposed = false;
    private Random RandomGenerator = new();
    private readonly ConcurrentBag<Ball> BallsList = new();
    private readonly CancellationTokenSource cts = new();
    private readonly Task movingTask;


    public override double BoardWidth { get; set; } = 800;
    public override double BoardHeight { get; set; } = 600; //TODO: check if these fit the layer 

    private void Move(object? x) //to sekwencyjne, więć jest niepotrzebne??? - to element kuli, kula nie może wiedzieć o istniniu innych kuli, więc do przeniesienia!!!
                                 // balls representations are independent and self-contained - tu nie może być nic o innych kulach
                                 // kolizje muszą być w warstwie Logic, a nie Data -> musimy w Logic mieć SEKCJĘ KRYTYCZNĄ (zamiana współbieżnego na sekwencyjne) - będą wątki dla każdej ze zderzających się kul i trzeba je powiązać 
                                 // inna opcja oprócz sekcji krytycznej to IMMUTABLE   
                                 // kula ma nie poruszać się o więcej niż jeden piksel 

                             
        {
            foreach (Ball item in BallsList)
                item.Move(
                    diameter: Ball.Diameter,
                    boardWidth: BoardWidth,
                    boardHeight: BoardHeight
            ); 
    }

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