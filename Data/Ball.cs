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
    public static double Diameter { get; internal set; }

    #endregion IBall

    #region private

    private Vector Position;

    private void RaiseNewPositionChangeNotification()
    {
      NewPositionNotification?.Invoke(this, Position);
    }

    internal void Move(double diameter, double boardWidth, double boardHeight)
    {
        double newX = Position.x + Velocity.x;
        double newY = Position.y + Velocity.y;

        if (newX <= 0)
        {
            newX = 0;
            Velocity = new Vector(Math.Abs(Velocity.x), Velocity.y);
        }
        else if (newX + diameter >= boardWidth)
        {
            newX = boardWidth - diameter;
            Velocity = new Vector(-Math.Abs(Velocity.x), Velocity.y);
        }

        if (newY <= 0)
        {
            newY = 0;
            Velocity = new Vector(Velocity.x, Math.Abs(Velocity.y));
        }
        else if (newY + diameter >= boardHeight)
        {
            newY = boardHeight - diameter;
            Velocity = new Vector(Velocity.x, -Math.Abs(Velocity.y));
        }

        Position = new Vector(newX, newY);
        RaiseNewPositionChangeNotification();
    }

        public void SetVelocity(double x, double y)
    { 
        Velocity = new Vector(x, y);
        NewVelocityNotification?.Invoke(this, Velocity);
    }

        #endregion private
        #region Skalowanie
        public static event EventHandler<double>? DiameterChanged;

        internal void ScalePosition(double scaleX, double scaleY)
        {
            Position = new Vector(Position.x * scaleX, Position.y * scaleY);
            RaiseNewPositionChangeNotification();
        }

        internal static void ScaleDiameter(double scale)
        {
            double newDiameter = Diameter * scale;
            Diameter = newDiameter;
            DiameterChanged?.Invoke(null, newDiameter);
        }
        #endregion
    }
}