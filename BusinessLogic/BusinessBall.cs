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
        public Ball(Data.IBall dataBall, List<Data.IBall> ballsList, double tWidth, double tHeight, double tBorder)
        {
            dataBall.NewPositionNotification += RaisePositionChangeEvent;
            dataBall.NewVelocityNotification += (sender, position) => HandleCollision(position, dataBall);
            this.ballsList = ballsList;
            TableWidth = tWidth;
            TableHeight = tHeight;
            TableBorder = tBorder;
        }

        #region IBall

        public event EventHandler<IPosition>? NewPositionNotification;
        //public event EventHandler<double>? DiameterChanged;

        #endregion IBall

        #region private
        private readonly List<Data.IBall> ballsList;
        private double TableWidth { get; }
        private double TableHeight { get; }
        private double TableBorder { get; }

        //TODO: czy to powinno być powtarzane???
        double IBall.TableWidth => TableWidth;

        double IBall.TableHeight => TableHeight;

        double IBall.TableBorder => TableBorder;

        //Stop()

        //private readonly Data.IBall _dataBall;
        private readonly object _collisionLock = new();

        public IVector Velocity => _dataBall.Velocity;

        public IPosition Position => _dataBall.Position;

        public double Diameter => _dataBall.Diameter;

        private void RaisePositionChangeEvent(object? sender, Data.IVector e)
        {
            NewPositionNotification?.Invoke(this, new Position (e.x, e.y));
        }
        private void HandleCollision(IVector Position, Data.IBall _dataBall)
        {
            bool lockTaken = false;
            try
            {
                Monitor.Enter(this, ref lockTaken);
                if (!lockTaken)
                    throw new ArgumentException("Lock not taken");
                bool bounceX = Position.x <= 0 || Position.x >= TableWidth - _dataBall.Diameter - 2 * TableBorder; //TODO: podaj prawidłowe wymiary
                bool bounceY = Position.y <= 0 || (Position.y + _dataBall.Velocity.y) > TableHeight - _dataBall.Diameter - 2 * TableBorder; //TODO: podaj prawidłowe wymiary
                if (bounceX)
                {
                    _dataBall.SetVelocity(-_dataBall.Velocity.x, _dataBall.Velocity.y);
                }
                if (bounceY)
                {
                    _dataBall.SetVelocity(_dataBall.Velocity.x, -_dataBall.Velocity.y);
                }
                foreach (Data.IBall other in ballsList)
                {
                    if (other == this) continue;

                    {
                        double dx = Position.x - other.Position.x;
                        double dy = Position.y - other.Position.y;

                        double distance = Math.Sqrt(dx * dx + dy * dy);
                        double minDistance = (other.Diameter + _dataBall.Diameter) / 2;

                        if (distance > 0 && distance <= minDistance)
                        {
                            double nx = dx / distance;
                            double ny = dy / distance;

                            double vN = _dataBall.Velocity.x * nx + _dataBall.Velocity.y * ny;
                            double vN2 = other.Velocity.x * nx + other.Velocity.y * ny;

                            double m1 = _dataBall.Mass;
                            double m2 = other.Mass;

                            double v1 = (vN * (m1 - m2) + 2 * m2 * vN2) / (m1 + m2);
                            double v2 = (vN2 * (m2 - m1) + 2 * m1 * vN) / (m1 + m2);

                            double tx = -ny;
                            double ty = nx;

                            double vTx1 = _dataBall.Velocity.x * tx + _dataBall.Velocity.y * ty;
                            double vTx2 = other.Velocity.x * tx + other.Velocity.y * ty;

                            _dataBall.SetVelocity(v1 * nx + vTx1 * tx, v1 * ny + vTx1 * ty);
                            other.SetVelocity(v2 * nx + vTx2 * tx, v2 * ny + vTx2 * ty);

                            double overlap = minDistance - distance;
                            if (overlap > 0)
                            {
                                _dataBall.SetPosition(_dataBall.Position.x + nx * overlap / 2, _dataBall.Position.y + ny * overlap / 2);
                                other.SetPosition(other.Position.x - nx * overlap / 2, other.Position.y - ny * overlap / 2);
                            }

                        }
                    }
                }
            }
            finally
            {
                if (lockTaken)
                {
                    Monitor.Exit(this);
                }
            }
        }
        #endregion private

    }
}