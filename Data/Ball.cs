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

    internal Ball(Vector initialPosition, Vector initialVelocity)
    {
      Position = initialPosition;
      Velocity = initialVelocity;
    }

    #endregion ctor

    #region IBall

    public event EventHandler<IVector>? NewPositionNotification;
    public event EventHandler<IVector>? NewVelocityNotification;

    public IVector Velocity { get; private set; }
    public static float Diameter { get; internal set; }

    #endregion IBall

    #region private

    private Vector Position;

    private void RaiseNewPositionChangeNotification()
    {
      NewPositionNotification?.Invoke(this, Position);
    }

    internal void Move(float diameter, float boardWidth, float boardHeight, float borderThickness) //private?
    { 
        Position = new Vector(Position.x + Velocity.x, Position.y + Velocity.y);

        if (Position.x <= 0 || Position.x >= boardWidth - diameter - borderThickness)  
        {
            Velocity = new Vector(-Velocity.x, Velocity.y);
        }

        if (Position.y <= 0 || Position.y >= boardHeight - diameter - borderThickness) 
        {
            Velocity = new Vector(Velocity.x, -Velocity.y);
        }
        RaiseNewPositionChangeNotification();
    }

    public void SetVelocity(double x, double y)
    { 
        Velocity = new Vector(x, y);
        NewVelocityNotification?.Invoke(this, Velocity);
    }

        #endregion private
    }
}