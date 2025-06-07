//____________________________________________________________________________________________________________________________________
//
//  Copyright (C) 2024, Mariusz Postol LODZ POLAND.
//
//  To be in touch join the community by pressing the `Watch` button and get started commenting using the discussion panel at
//
//  https://github.com/mpostol/TP/discussions/182
//
//_____________________________________________________________________________________________________________________________________

using System.Collections.Concurrent;
using System.Diagnostics;
using TP.ConcurrentProgramming.Data;
using UnderneathLayerAPI = TP.ConcurrentProgramming.Data.DataAbstractAPI;

namespace TP.ConcurrentProgramming.BusinessLogic
{
    internal class BusinessLogicImplementation : BusinessLogicAbstractAPI
    {
        #region Constructor

        public BusinessLogicImplementation() : this(null)
        {
        }

        internal BusinessLogicImplementation(UnderneathLayerAPI? underneathLayer)
        {
            _layerBellow = underneathLayer ?? UnderneathLayerAPI.GetDataLayer();
        }

        #endregion

        #region BusinessLogicAbstractAPI Implementation

        public override void Dispose()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(BusinessLogicImplementation));

            foreach (var ball in _ballList)
            {
                ball.Stop();
            }

            _layerBellow.Dispose();

            _disposed = true;
        }

        public override void Start(int numberOfBalls, Action<IPosition, IBall> upperLayerHandler, double width, double height, double border)
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(BusinessLogicImplementation));
            if (upperLayerHandler == null)
                throw new ArgumentNullException(nameof(upperLayerHandler));


            _layerBellow.Start(numberOfBalls, (startingPosition, dataBall) =>
            {
                var ball = new Ball(dataBall, width, height, border, _ballList, ILogger.CreateDefaultLogger());
                _ballList.Add(ball);

                upperLayerHandler(new Position(startingPosition.x, startingPosition.y), ball);
            });
        }

        #endregion

        #region Private Fields

        private bool _disposed = false;
        private readonly List<Ball> _ballList = new List<Ball>();
        private readonly UnderneathLayerAPI _layerBellow;

        #endregion

        #region Testing Infrastructure

        [Conditional("DEBUG")]
        internal void CheckObjectDisposed(Action<bool> returnInstanceDisposed)
        {
            returnInstanceDisposed(_disposed);
        }

        #endregion
    }
}