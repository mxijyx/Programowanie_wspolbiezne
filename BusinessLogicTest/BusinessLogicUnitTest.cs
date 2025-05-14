//____________________________________________________________________________________________________________________________________
//
//  Copyright (C) 2024, Mariusz Postol LODZ POLAND.
//
//  To be in touch join the community by pressing the `Watch` button and get started commenting using the discussion panel at
//
//  https://github.com/mpostol/TP/discussions/182
//
//_____________________________________________________________________________________________________________________________________

using TP.ConcurrentProgramming.Data;

namespace TP.ConcurrentProgramming.BusinessLogic.Test
{
  [TestClass]
  public class BusinessLogicImplementationUnitTest
  {
    [TestMethod]
    public void ConstructorTestMethod()
    {
      using (BusinessLogicImplementation newInstance = new(new DataLayerConstructorFixcure()))
      {
        bool newInstanceDisposed = true;
        newInstance.CheckObjectDisposed(x => newInstanceDisposed = x);
        Assert.IsFalse(newInstanceDisposed);
      }
    }

    [TestMethod]
    public void DisposeTestMethod()
    {
      DataLayerDisposeFixcure dataLayerFixcure = new DataLayerDisposeFixcure();
      BusinessLogicImplementation newInstance = new(dataLayerFixcure);
      Assert.IsFalse(dataLayerFixcure.Disposed);
      bool newInstanceDisposed = true;
      newInstance.CheckObjectDisposed(x => newInstanceDisposed = x);
      Assert.IsFalse(newInstanceDisposed);
      newInstance.Dispose();
      newInstance.CheckObjectDisposed(x => newInstanceDisposed = x);
      Assert.IsTrue(newInstanceDisposed);
      Assert.ThrowsException<ObjectDisposedException>(() => newInstance.Dispose());
      Assert.ThrowsException<ObjectDisposedException>(() => newInstance.Start(0, (position, ball) => { }));
      Assert.IsTrue(dataLayerFixcure.Disposed);
    }

    [TestMethod]
    public void StartTestMethod()
    {
      DataLayerStartFixcure dataLayerFixcure = new();
      using (BusinessLogicImplementation newInstance = new(dataLayerFixcure))
      {
        int called = 0;
        int numberOfBalls2Create = 10;
        newInstance.Start(
          numberOfBalls2Create,
          (startingPosition, ball) => { called++; Assert.IsNotNull(startingPosition); Assert.IsNotNull(ball); });
        Assert.AreEqual<int>(1, called);
        Assert.IsTrue(dataLayerFixcure.StartCalled);
        Assert.AreEqual<int>(numberOfBalls2Create, dataLayerFixcure.NumberOfBallseCreated);
      }
    }

    #region testing instrumentation

    private class DataLayerConstructorFixcure : Data.DataAbstractAPI
    {
      public override void SetCanvasSize(double width, double height)
      {
        throw new NotImplementedException();
      }

      public override List<Data.IBall> CreateBalls(int count, double boardWidth, double boardHeight, double minMass, double maxMass)
      {
        throw new NotImplementedException();
      }

      public override void Dispose()
      { }

      public override void Start(int numberOfBalls, Action<IVector, Data.IBall> upperLayerHandler)
      {
        throw new NotImplementedException();
      }

      public override double BoardWidth { get; set; }
      public override double BoardHeight { get; set; }
    }

    private class DataLayerDisposeFixcure : Data.DataAbstractAPI
    {
      internal bool Disposed = false;

      public override void SetCanvasSize(double width, double height)
      {
        throw new NotImplementedException();
      }

      public override List<Data.IBall> CreateBalls(int count, double boardWidth, double boardHeight, double minMass, double maxMass)
      {
        throw new NotImplementedException();
      }

      public override void Dispose()
      {
        Disposed = true;
      }

      public override void Start(int numberOfBalls, Action<IVector, Data.IBall> upperLayerHandler)
      {
        throw new NotImplementedException();
      }

      public override double BoardWidth { get; set; }
      public override double BoardHeight { get; set; }
    }

    private class DataLayerStartFixcure : Data.DataAbstractAPI
    {
      internal bool StartCalled = false;
      internal int NumberOfBallseCreated = -1;

      public override void SetCanvasSize(double width, double height)
      {
        throw new NotImplementedException();
      }

      public override List<Data.IBall> CreateBalls(int count, double boardWidth, double boardHeight, double minMass, double maxMass)
      {
        throw new NotImplementedException();
      }

      public override void Dispose()
      { }

      public override void Start(int numberOfBalls, Action<IVector, Data.IBall> upperLayerHandler)
      {
        StartCalled = true;
        NumberOfBallseCreated = numberOfBalls;
        upperLayerHandler(new DataVectorFixture(), new DataBallFixture());
      }

      public override double BoardWidth { get; set; }
      public override double BoardHeight { get; set; }

      private record DataVectorFixture : Data.IVector
      {
        public double x { get; init; }
        public double y { get; init; }
      }

      private class DataBallFixture : Data.IBall
      {
        public void SetVelocity(double x, double y)
        {
          throw new NotImplementedException();
        }

        public IVector Velocity { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public IVector Position { get; }
        public double Mass { get; set; }
        public double Diameter { get; }

        public event EventHandler<IVector>? NewVelocityNotification;
        public event EventHandler<IVector>? NewPositionNotification = null;
        public event EventHandler<double>? DiameterChanged;
      }
    }

    #endregion testing instrumentation
  }
}