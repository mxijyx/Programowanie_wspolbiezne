//____________________________________________________________________________________________________________________________________
//
//  Copyright (C) 2024, Mariusz Postol LODZ POLAND.
//
//  To be in touch join the community by pressing the `Watch` button and get started commenting using the discussion panel at
//
//  https://github.com/mpostol/TP/discussions/182
//
//_____________________________________________________________________________________________________________________________________

namespace TP.ConcurrentProgramming.BusinessLogic
{
  internal class Ball : IBall
  {
    public Ball(Data.IBall dataBall)
    {
      _dataBall = dataBall;
      dataBall.NewPositionNotification += (s, pos) =>
        NewPositionNotification?.Invoke(this, new Position(pos.x, pos.y));
      dataBall.NewVelocityNotification += HandleCollision;
    }

    #region IBall

    public event EventHandler<IPosition>? NewPositionNotification;
    public event EventHandler<double>? DiameterChanged;

    #endregion IBall

    #region private

    private readonly Data.IBall _dataBall;
    private readonly object _collisionLock = new();

    private void RaisePositionChangeEvent(object? sender, Data.IVector e)
    {
      NewPositionNotification?.Invoke(this, new Position(e.x, e.y));
    }
    private void HandleCollision(object? sender, Data.IVector velocity)
    {
      lock (_collisionLock)
      {
        // Aktualizacja prędkości po kolizji
      }
    }
    #endregion private
  }
}