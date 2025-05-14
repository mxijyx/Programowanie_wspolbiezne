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

    internal Ball(Vector initialPosition, Vector initialVelocity, double initialMass)
    {
      _position = initialPosition;
      _velocity = initialVelocity;
      Mass = initialMass;
      StartMovement();
    }

    #endregion ctor

    #region IBall

    public event EventHandler<IVector>? NewPositionNotification;
    public event EventHandler<IVector>? NewVelocityNotification;
    public event EventHandler<double>? DiameterChanged;

    public IVector Velocity
    {
        get { lock (_syncRoot) return _velocity; }
        private set { lock (_syncRoot) _velocity = (Vector)value; }
    }

    public IVector Position
    {
        get
        {
            lock (_syncRoot) return _position; 
        }
    }

    public double Mass
    {
        get { lock (_syncRoot) return _mass; }
        set
        {
            lock (_syncRoot)
            {
                if (Math.Abs(_mass - value) < double.Epsilon) return;
                _mass = value;
                Diameter = CalculateDiameter(value);
            }
            DiameterChanged?.Invoke(this, Diameter);
        }
    }

    public double Diameter { get; internal set; }

        #endregion IBall

        #region private
    private Vector _position;
    private Vector _velocity;
    private double _mass;
    private readonly object _syncRoot = new();
    private bool _isMoving = true;

    private void RaiseNewPositionChangeNotification()
    {
      NewPositionNotification?.Invoke(this, _position);
    }

    private async void StartMovement()
    {
        await Task.Run(() =>
        {
            while (_isMoving)
            {
                UpdatePosition();
                Thread.Sleep(16); // ~60 FPS
            }
        });
    }

    private void UpdatePosition()
    {
        var newPosition = new Vector(
            _position.x + _velocity.x,
            _position.y + _velocity.y
        );

        _position = newPosition;
        NewPositionNotification?.Invoke(this, _position);
    }

        public void SetVelocity(double x, double y)
        { 
            Velocity = new Vector(x, y);
            NewVelocityNotification?.Invoke(this, Velocity);
        }


        #endregion private
        public void Stop() => _isMoving = false;
        private static double CalculateDiameter(double mass) => 20.0 * Math.Sqrt(mass);
        #region Skalowanie

        internal void ScalePosition(double scaleX, double scaleY)
        {
            _position = new Vector(_position.x * scaleX, _position.y * scaleY);
            RaiseNewPositionChangeNotification();
        }

        internal void ScaleDiameter(double scale)
        {
            double newDiameter = Diameter * scale;
            Diameter = newDiameter;
            DiameterChanged?.Invoke(null, newDiameter);
        }
        #endregion
    }
}