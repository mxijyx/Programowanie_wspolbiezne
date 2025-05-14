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
using TP.ConcurrentProgramming.Data;

namespace TP.ConcurrentProgramming.BusinessLogic
{
  internal class Ball : IBall
  {
    public Ball(Data.IBall dataBall)
    {
      _dataBall = dataBall;
      dataBall.NewPositionNotification += (s, pos) =>
        NewPositionNotification?.Invoke(this, pos);
      dataBall.NewVelocityNotification += HandleCollision;
    }

    #region IBall

    public event EventHandler<IVector>? NewPositionNotification;
    public event EventHandler<double>? DiameterChanged;

    #endregion IBall

    #region private

    private readonly Data.IBall _dataBall;
    private readonly object _collisionLock = new();

    public IVector Velocity => _dataBall.Velocity;

    public IVector Position => _dataBall.Position;

    public double Diameter => _dataBall.Diameter;

    private void RaisePositionChangeEvent(object? sender, Data.IVector e)
    {
      NewPositionNotification?.Invoke(this, e);
    }
    private void HandleCollision(object? sender, Data.IVector velocity)
    {
      lock (_collisionLock)
      {
        // Aktualizacja prędkości po kolizji
      }
    }

        public void SetVelocity(double x, double y)
        {
            _dataBall.SetVelocity(x, y);
        }

        public void SetPosition(double x, double y)
        {
            _dataBall.SetPosition(x, y);
        }
        #endregion private
    }
}