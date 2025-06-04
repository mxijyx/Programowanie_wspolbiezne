//____________________________________________________________________________________________________________________________________
//
//  Copyright (C) 2024, Mariusz Postol LODZ POLAND.
//
//  To be in touch join the community by pressing the `Watch` button and get started commenting using the discussion panel at
//
//  https://github.com/mpostol/TP/discussions/182
//
//_____________________________________________________________________________________________________________________________________

using Microsoft.VisualStudio.TestTools.UnitTesting;
using TP.ConcurrentProgramming.Data;
using System;
using System.Collections.Generic;

namespace TP.ConcurrentProgramming.BusinessLogic.Test
{
    [TestClass]
    public class BallUnitTest
    {
        [TestMethod]
        public void Move_ShouldTriggerNotificationAndUpdatePosition()
        {
            // Arrange
            var dataBallFixture = new DataBallFixture();
            var otherBalls = new List<Ball>(); // Przekazujemy pustą listę innych piłek
            var ball = new Ball(dataBallFixture, 10, 10, 2, otherBalls);

            int callbackCount = 0;
            ball.NewPositionNotification += (sender, position) =>
            {
                Assert.IsNotNull(sender);
                Assert.IsNotNull(position);
                callbackCount++;
            };

            // Act
            dataBallFixture.SimulateMovement();

            // Assert
            Assert.AreEqual(1, callbackCount, "Notification should be called once");
            Assert.AreEqual(1, dataBallFixture.Position.x, "X position should be updated");
            Assert.AreEqual(1, dataBallFixture.Position.y, "Y position should be updated");
        }

        #region testing instrumentation
        private class DataBallFixture : Data.IBall
        {
            public Data.IVector Velocity { get; set; } = new VectorFixture(1, 1);
            public Data.IVector Position { get; private set; } = new VectorFixture(0, 0);
            public double Diameter => 2;
            public double Mass => 1;

            public event EventHandler<Data.IVector>? NewPositionNotification;

            internal void SimulateMovement()
            {
                // Aktualizacja pozycji zgodnie z prędkością
                Position = new VectorFixture(
                    Position.x + Velocity.x,
                    Position.y + Velocity.y
                );

                NewPositionNotification?.Invoke(this, Position);
            }

            public void Stop() { }

            public void SetVelocity(IVector newVelocity)
            {
                throw new NotImplementedException();
            }
        }

        private class VectorFixture : Data.IVector
        {
            public double x { get; set; }
            public double y { get; set; }

            public VectorFixture(double x, double y)
            {
                this.x = x;
                this.y = y;
            }
        }
        #endregion
    }
}