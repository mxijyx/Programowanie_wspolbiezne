﻿//____________________________________________________________________________________________________________________________________
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
    public abstract class DataAbstractAPI : IDisposable
    {
        #region Layer Factory

        public static DataAbstractAPI GetDataLayer()
        {
            return modelInstance.Value;
        }

        #endregion Layer Factory

        #region public API

        public abstract void Start(int numberOfBalls, Action<IVector, IBall> upperLayerHandler);

        #endregion public API

        #region IDisposable

        public abstract void Dispose();

        #endregion IDisposable

        #region private

        private static Lazy<DataAbstractAPI> modelInstance = new Lazy<DataAbstractAPI>(() => new DataImplementation());

        #endregion private
    }

    public interface IVector
    {
        /// <summary>
        /// The X component of the vector.
        /// </summary>
        double x { get; set; }

        /// <summary>
        /// The y component of the vector.
        /// </summary>
        double y { get; set; }
    }
    //public interface IPosition
    //{
    //    double x { get; }
    //    double y { get; }
    //}

    public interface IBall
    {
        event EventHandler<IVector> NewPositionNotification;
        IVector Velocity { get; }
        IVector Position { get; }
        int Id { get; }
        //double Diameter { get; }
        //double Mass { get; }

        void Stop();
        void SetVelocity(IVector newVelocity);
    }
    public interface ILogger : IDisposable
    {
        void Log(IVector position, IVector velocity, int threadID, LogLevel level);
        void Log(string message, LogLevel level = LogLevel.Info); //to change (string messaage not optimal for diagnostics)
        public static ILogger CreateDefaultLogger() => Logger.Instance;
    }
}