//____________________________________________________________________________________________________________________________________
//
//  Copyright (C) 2024, Mariusz Postol LODZ POLAND.
//
//  To be in touch join the community by pressing the `Watch` button and get started commenting using the discussion panel at
//
//  https://github.com/mpostol/TP/discussions/182
//
//_____________________________________________________________________________________________________________________________________


using TP.ConcurrentProgramming.Data;

namespace TP.ConcurrentProgramming.BusinessLogic
{
    internal class Ball : IBall
    {
        Data.IBall dataBall;
        List<Ball> ballList = new List<Ball>();
        private readonly object ballLock = new();

        public Ball(Data.IBall ball, double w, double h, double border, List<Ball> otherBalls)
        {
            dataBall = ball;
            TableWidth = w;
            TableHeight = h;
            TableBorder = border;
            ball.NewPositionNotification += RaisePositionChangeEvent;
            ballList = otherBalls;

        }

        #region IBall

        public event EventHandler<IPosition>? NewPositionNotification;

        #endregion IBall

        #region private
        public double TableWidth { get; }
        public double TableHeight { get; }
        public double TableBorder { get; }

        internal void Stop()
        {
            dataBall.Stop();
        }

        private void RaisePositionChangeEvent(object? sender, Data.IVector e)
        {
            HandleWalls(e);
            HandleBallCollisions();
            NewPositionNotification?.Invoke(this, new Position(e.x, e.y));
        }
        private void HandleWalls(Data.IVector position)
        {
            lock (ballLock)
            {
                if (position.x >= TableWidth - dataBall.Diameter - 2 * TableBorder || position.x <= 0)
                {
                    dataBall.Velocity.x = -dataBall.Velocity.x;
                }
                if (position.y >= TableHeight - dataBall.Diameter - 2 * TableBorder || position.y <= 0)
                {
                    dataBall.Velocity.y = -dataBall.Velocity.y;
                }
            }
        }
        private void HandleBallCollisions()
        {

            foreach (Ball other in ballList)
            {
                //lock (ballLock) // czy to jest w ogóle potrzebne? ~dr Jak zaimplementować sekcję krytyczna, aby była skuteczna? Monitor!!!!! 
                //{
                    if (other == this) continue;
                    bool isLockTaken = false;
                try 
                { 
                    Monitor.Enter(ballLock, ref isLockTaken);

                    double dx = dataBall.Position.x - other.dataBall.Position.x;
                    double dy = dataBall.Position.y - other.dataBall.Position.y;

                    double distance = Math.Sqrt(dx * dx + dy * dy);
                    double minDistance = (dataBall.Diameter + other.dataBall.Diameter) / 2;

                    if (distance > 0 && distance <= minDistance)
                    {

                        double nx = dx / distance;
                        double ny = dy / distance;


                        double vN = dataBall.Velocity.x * nx + dataBall.Velocity.y * ny;
                        double vN2 = other.dataBall.Velocity.x * nx + other.dataBall.Velocity.y * ny;

                        double m1 = dataBall.Mass;
                        double m2 = other.dataBall.Mass;

                        double vNp = (vN * (m1 - m2) + 2 * m2 * vN2) / (m1 + m2);
                        double vN2p = (vN2 * (m2 - m1) + 2 * m1 * vN) / (m1 + m2);

                        double tx = -ny;
                        double ty = nx;
                        double v1t = dataBall.Velocity.x * tx + dataBall.Velocity.y * ty;
                        double v2t = other.dataBall.Velocity.x * tx + other.dataBall.Velocity.y * ty;

                        dataBall.Velocity.x = vNp * nx + v1t * tx;
                        dataBall.Velocity.y = vNp * ny + v1t * ty;
                        other.dataBall.Velocity.x = vN2p * nx + v2t * tx;
                        other.dataBall.Velocity.y = vN2p * ny + v2t * ty;

                        double overlap = minDistance - distance;
                        if (overlap > 0)
                        {
                            dataBall.Position.x += nx * overlap/2;
                            dataBall.Position.y += ny * overlap/2;
                            other.dataBall.Position.x -= nx * overlap/2;
                            other.dataBall.Position.y -= ny * overlap/2;
                        }
                    }
                }
                finally
                {
                    if (isLockTaken)
                    {
                        Monitor.Exit(ballLock);
                    }
                }

                #endregion private
            }
        }
    }
}