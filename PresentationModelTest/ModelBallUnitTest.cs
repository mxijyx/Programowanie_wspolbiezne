//____________________________________________________________________________________________________________________________________
//
//  Copyright (C) 2024, Mariusz Postol LODZ POLAND.
//
//  To be in touch join the community by pressing the `Watch` button and get started commenting using the discussion panel at
//
//  https://github.com/mpostol/TP/discussions/182
//
//_____________________________________________________________________________________________________________________________________

using System.Numerics;
using TP.ConcurrentProgramming.BusinessLogic;
using TP.ConcurrentProgramming.Data;

namespace TP.ConcurrentProgramming.Presentation.Model.Test
{
    [TestClass]
    public class ModelBallUnitTest
    {
        [TestMethod]
        public void ConstructorTestMethod()
        {
            ModelBall ball = new ModelBall(0.0, 0.0, new BusinessLogicIBallFixture());
            Assert.AreEqual<double>(0.0, ball.Top);
            Assert.AreEqual<double>(0.0, ball.Top);
        }

        [TestMethod]
        public void PositionChangeNotificationTestMethod()
        {
            int notificationCounter = 0;
            ModelBall ball = new ModelBall(0, 0.0, new BusinessLogicIBallFixture());
            ball.PropertyChanged += (sender, args) => notificationCounter++;
            Assert.AreEqual(0, notificationCounter);
            ball.SetLeft(1.0);
            Assert.AreEqual<int>(1, notificationCounter);
            Assert.AreEqual<double>(1.0, ball.Left);
            Assert.AreEqual<double>(0.0, ball.Top);
            ball.SetTop(1.0);
            Assert.AreEqual(2, notificationCounter);
            Assert.AreEqual<double>(1.0, ball.Left);
            Assert.AreEqual<double>(1.0, ball.Top);
            // TODO: Test bottom and right
        }

        #region testing instrumentation

        private class BusinessLogicIBallFixture : BusinessLogic.IBall
        {
            public IVector Velocity { get; }

            public IVector Position { get; }

            public double Diameter { get; }

            public event EventHandler<IVector>? NewPositionNotification;
            public event EventHandler<double> DiameterChanged;

            public void Dispose()
            {
                throw new NotImplementedException();
            }


            #endregion testing instrumentation
        }
    }
}