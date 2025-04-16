//____________________________________________________________________________________________________________________________________
//
//  Copyright (C) 2024, Mariusz Postol LODZ POLAND.
//
//  To be in touch join the community by pressing the `Watch` button and get started commenting using the discussion panel at
//
//  https://github.com/mpostol/TP/discussions/182
//
//_____________________________________________________________________________________________________________________________________

namespace TP.ConcurrentProgramming.Data.Test
{
  [TestClass]
  public class BallUnitTest
  {
    [TestMethod]
    public void ConstructorTestMethod()
    {
      Vector testinVector = new Vector(0.0, 0.0);
      Ball newInstance = new(testinVector, testinVector);
    }

    [TestMethod]
    public void MoveAtZeroVelocityTestMethod()
    {
      Vector initialPosition = new(10.0, 10.0);
      Ball newInstance = new(initialPosition, new Vector(0.0, 0.0));
      IVector currentPosition = initialPosition;
      newInstance.NewPositionNotification += (sender, position) => currentPosition = position;
      newInstance.Move(0); //TODO (board and ball dimentions
      Assert.AreEqual<IVector>(initialPosition, currentPosition);
    }
     [TestMethod]
        public void Move_xVelocityReverseTestMethod()
        {
            Vector initialPosition = new(370, 10.0); //board and ball dimentions
            Ball newInstance = new(initialPosition, new Vector(10.0, 0.0));
            newInstance.Move(0); //TODO (board and ball dimentions
            Vector vector = new(-10.0, 0.0);
            Assert.AreEqual<IVector>(vector, newInstance.Velocity);
        }
     [TestMethod]
        public void Move_yVelocityReverseTestMethod()
        {
            Vector initialPosition = new(10.0, 390); //board and ball dimentions
            Ball newInstance = new(initialPosition, new Vector(0.0, 10.0));
            newInstance.Move(0); //TODO (board and ball dimentions
            Vector vector = new(0.0, -10.0);
            Assert.AreEqual<IVector>(vector, newInstance.Velocity);
        }

    }
}