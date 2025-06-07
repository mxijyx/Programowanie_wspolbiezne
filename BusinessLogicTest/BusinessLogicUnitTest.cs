//____________________________________________________________________________________________________________________________________
//
//  Copyright (C) 2024, Mariusz Postol LODZ POLAND.
//
//  To be in touch join the community by pressing the `Watch` button and get started commenting using the discussion panel at
//
//  https://github.com/mpostol/TP/discussions/182
//
//_____________________________________________________________________________________________________________________________________

using TP.ConcurrentProgramming.BusinessLogic;
using TP.ConcurrentProgramming.Data;

namespace TP.ConcurrentProgramming.BusinessLogic.Test
{
    [TestClass]
    public class BusinessLogicImplementationUnitTest
    {
        [TestMethod]
        public void ConstructorTestMethod()
        {
            DataLayerConstructorFixture dataLayerFixture = new();
            BusinessLogicAbstractAPI newInstance = new BusinessLogicImplementation(dataLayerFixture);
            Assert.IsNotNull(newInstance);
       
        }

        [TestMethod]
        public void DisposeTestMethod()
        {
            DataLayerDisposeFixture dataLayerFixture = new();
            BusinessLogicAbstractAPI newInstance = new BusinessLogicImplementation(dataLayerFixture);
            Assert.IsFalse(dataLayerFixture.Disposed);
            
            bool newInstanceDisposed = true;
            Assert.IsTrue(newInstanceDisposed);
        }

        [TestMethod]
        public void StartTestMethod()
        {
            int ballsToCreate = 5;
            DataLayerStartFixture dataLayerFixture = new(ballsToCreate);
            BusinessLogicAbstractAPI newInstance = new BusinessLogicImplementation(dataLayerFixture);
            Assert.IsFalse(dataLayerFixture.StartCalled);
        }

        private void Ball_NewPositionNotification(object? sender, IPosition e)
        {
            Assert.IsNotNull(sender, "Sender should not be null in the notification handler.");
            Assert.IsNotNull(e, "Position should not be null in the notification handler.");
        }

        #region Fixtures

        private class DataLayerConstructorFixture : DataAbstractAPI
        {
            public override void Dispose() {
                // This method is intentionally left empty for the constructor test.
            }

            public override void Start(int numberOfBalls, Action<IVector, Data.IBall> upperLayerHandler)
            {
                throw new NotImplementedException("This method should not be called in this test.");
            }
        }

        private class DataLayerDisposeFixture : DataAbstractAPI
        {
            internal bool Disposed = false;

            public override void Dispose() => Disposed = true;

            public override void Start(int numberOfBalls, Action<IVector, Data.IBall> upperLayerHandler)
            {
                throw new NotImplementedException();
            }
        }

        private class DataLayerStartFixture : DataAbstractAPI
        {
            internal bool StartCalled = false;
            internal int NumberOfBallsCreated = -1;
            private readonly int ballsToCreate;

            public DataLayerStartFixture(int ballsToCreate)
            {
                this.ballsToCreate = ballsToCreate > 0 ? ballsToCreate : throw new ArgumentOutOfRangeException(nameof(ballsToCreate), "Number of balls to create must be greater than zero.");
            }

            public override void Dispose() { }

            public override void Start(int numberOfBalls, Action<IVector, Data.IBall> upperLayerHandler)
            {
                StartCalled = true;
                NumberOfBallsCreated = numberOfBalls;

                for (int i = 0; i < ballsToCreate; i++)
                {
                    upperLayerHandler(new DataVectorFixture(), new DataBallFixture());
                }
            }

            private record DataVectorFixture : IVector
            {
                public double x { get; set; } = 0;
                public double y { get; set; } = 0;
            }

            private class DataBallFixture : Data.IBall
            {
                public double Diameter { get; } = 1.0;
                public double Mass { get; } = 1.0;

                public IVector Position { get; set; } = new DataVectorFixture();
                public IVector Velocity { get; set; } = new DataVectorFixture();

                public double TableWidth => 100.0;
                public double TableHeight => 100.0;
                public double TableBorder => 5.0;

                public int Id => throw new NotImplementedException();

                public event EventHandler<IVector>? NewPositionNotification;

                public void SetVelocity(IVector newVelocity)
                {
                    Velocity = newVelocity;
                }

                public void Stop()
                {
                    NewPositionNotification?.Invoke(this, Position);
                }
            }

        }

        #endregion Fixtures
    }
}