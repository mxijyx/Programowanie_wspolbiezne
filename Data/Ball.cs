//____________________________________________________________________________________________________________________________________
//
//  Copyright (C) 2024, Mariusz Postol LODZ POLAND.
//
//  To be in touch join the community by pressing the `Watch` button and get started commenting using the discussion panel at
//
//  https://github.com/mpostol/TP/discussions/182
//
//_____________________________________________________________________________________________________________________________________

namespace TP.ConcurrentProgramming.Data
{
  internal class Ball : IBall
  {
    #region ctor

    internal Ball(IVector initialPosition, IVector initialVelocity)
    {
      Position = initialPosition;
      Velocity = initialVelocity;
    }

    #endregion ctor

    #region IBall

    public event EventHandler<IVector>? NewPositionNotification;
    public event EventHandler<IVector>? NewVelocityNotification;

    public IVector Velocity { get; private set; }
    public void SetVelocity(double x, double y)
    {
      Velocity = new Vector(x, y);
      NewVelocityNotification?.Invoke(this, Velocity);
    }
    public IVector Position { get; private set; }  
    public static double Diameter { get; internal set; }
    public static double Mass { get; internal set; }
    
    #endregion IBall

    #region private
    private void RaiseNewPositionChangeNotification()
    {
      NewPositionNotification?.Invoke(this, Position);
    }

    internal void Move(double diameter, double boardWidth, double boardHeight) 
    {
        Position = new Vector(Position.x + Velocity.x, Position.y + Velocity.y);
        RaiseNewPositionChangeNotification();
    }

    #endregion private
  }
}