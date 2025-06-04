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
            using BusinessLogicAbstractAPI newInstance = new BusinessLogicImplementation(new DataLayerConstructorFixture());
            bool newInstanceDisposed = true;
            ((BusinessLogicImplementation)newInstance).CheckObjectDisposed(x => newInstanceDisposed = x);
            Assert.IsFalse(newInstanceDisposed);
        }

        [TestMethod]
        public void DisposeTestMethod()
        {
            DataLayerDisposeFixture dataLayerFixture = new();
            BusinessLogicAbstractAPI newInstance = new BusinessLogicImplementation(dataLayerFixture);

            Assert.IsFalse(dataLayerFixture.Disposed);

            bool newInstanceDisposed = true;
            ((BusinessLogicImplementation)newInstance).CheckObjectDisposed(x => newInstanceDisposed = x);
            Assert.IsFalse(newInstanceDisposed);

            newInstance.Dispose();

            ((BusinessLogicImplementation)newInstance).CheckObjectDisposed(x => newInstanceDisposed = x);
            Assert.IsTrue(newInstanceDisposed);

            Assert.ThrowsException<ObjectDisposedException>(() => newInstance.Dispose());
            Assert.ThrowsException<ObjectDisposedException>(() => newInstance.Start(0, (position, ball) => { }, 20, 20, 20));
            Assert.IsTrue(dataLayerFixture.Disposed);
        }

        [TestMethod]
        public void StartTestMethod()
        {
            int numberOfBalls2Create = 10;
            DataLayerStartFixture dataLayerFixture = new(numberOfBalls2Create);
            using BusinessLogicAbstractAPI newInstance = new BusinessLogicImplementation(dataLayerFixture);

            int called = 0;

            newInstance.Start(
                numberOfBalls2Create,
                (startingPosition, ball) =>
                {
                    called++;
                    Assert.IsNotNull(startingPosition);
                    Assert.IsNotNull(ball);
                },
                20, 20, 20);

            Assert.AreEqual(10, called);
            Assert.IsTrue(dataLayerFixture.StartCalled);
            Assert.AreEqual(10, dataLayerFixture.NumberOfBallsCreated);
        }

        #region Fixtures

        private class DataLayerConstructorFixture : DataAbstractAPI
        {
            public override void Dispose() { }

            public override void Start(int numberOfBalls, Action<IVector, Data.IBall> upperLayerHandler)
            {
                throw new NotImplementedException();
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
                this.ballsToCreate = ballsToCreate;
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

                public event EventHandler<IVector>? NewPositionNotification;

                public void Stop()
                {
                    NewPositionNotification?.Invoke(this, Position);
                }
            }

        }

        #endregion Fixtures
    }
}
