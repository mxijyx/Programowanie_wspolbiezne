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

namespace TP.ConcurrentProgramming.Data
{
  internal class DataImplementation : DataAbstractAPI
  {
    #region ctor
    public double MaxX { get; private set; }
    public double MaxY { get; private set; }
    public DataImplementation()
    {
      MoveTimer = new Timer(Move, null, TimeSpan.Zero, TimeSpan.FromMilliseconds(16)); //krok w terminologii programowania - przechodzenie z instrukcji do instrukcji 
                                                                                       // timer wywołuje Move sekwencyjnie, a Move też jest sekwencyjne -> czyli i timer i funkcja Move jest nieporzebna?
                                                                                       // cykl odświeżania musi zależeć od prędkość kuli - czas odświeżania musi być mniejszy dla szybszych kul-> to musi być przy getterze velocity -> dlatgeo timer jest bez sensu
                                                                                       // data musi pozostać abstrakcyjne 
                                                                                       //TODO: do usunięcia
      MaxX = 400;
      MaxY = 420;
    }

        #endregion ctor

        #region DataAbstractAPI

        public override void Start(int numberOfBalls, Action<IVector, IBall> upperLayerHandler)
    {
      if (Disposed)
        throw new ObjectDisposedException(nameof(DataImplementation));
      if (upperLayerHandler == null)
        throw new ArgumentNullException(nameof(upperLayerHandler));
      Random random = new Random();
      for (int i = 0; i < numberOfBalls; i++)
      {
        Vector startingPosition = new(random.Next(100, 400 - 100), random.Next(100, 400 - 100));
                Vector startingVelocity = new((RandomGenerator.NextDouble() - 0.5) * 10, (RandomGenerator.NextDouble() - 0.5) * 10);
                Ball newBall = new(startingPosition, startingVelocity);
        upperLayerHandler(startingPosition, newBall);
        BallsList.Add(newBall);
      }
    }

    public override void SetCanvasSize(double width, double height)
    {
      if (Disposed)
        throw new ObjectDisposedException(nameof(DataImplementation));
       MaxX = width - (Ball.Diameter);
       MaxY = height - (Ball.Diameter);
    }


    #endregion DataAbstractAPI

    #region IDisposable

    protected virtual void Dispose(bool disposing)
    {
      if (!Disposed)
      {
        if (disposing)
        {
          MoveTimer.Dispose();
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
    private List<Ball> BallsList = [];

    public override double BoardWidth { get; set; }
    public override double BoardHeight { get; set; } //TODO: check if these fit the layer 

    private void Move(object? x) //to sekwencyjne, więć jest niepotrzebne??? - to element kuli, kula nie może wiedzieć o istniniu innych kuli, więc do przeniesienia!!!
                                 // balls representations are independent and self-contained - tu nie może być nic o innych kulach
                                 // kolizje muszą być w warstwie Logic, a nie Data -> musimy w Logic mieć SEKCJĘ KRYTYCZNĄ (zamiana współbieżnego na sekwencyjne) - będą wątki dla każdej ze zderzających się kul i trzeba je powiązać 
                                 // inna opcja oprócz sekcji krytycznej to IMMUTABLE   
                                 // kula ma nie poruszać się o więcej niż jeden piksel 

                             
        {
            foreach (Ball item in BallsList)
                item.Move(
                    diameter: Ball.Diameter,
                    boardWidth: (float)BoardWidth,
                    boardHeight: (float) BoardHeight,
                     8
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