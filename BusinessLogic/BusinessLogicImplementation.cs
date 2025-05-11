//____________________________________________________________________________________________________________________________________
//
//  Copyright (C) 2024, Mariusz Postol LODZ POLAND.
//
//  To be in touch join the community by pressing the `Watch` button and get started commenting using the discussion panel at
//
//  https://github.com/mpostol/TP/discussions/182
//
//_____________________________________________________________________________________________________________________________________

using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnderneathLayerAPI = TP.ConcurrentProgramming.Data.DataAbstractAPI;

namespace TP.ConcurrentProgramming.BusinessLogic
{
  internal class BusinessLogicImplementation : BusinessLogicAbstractAPI
  {
    #region ctor

    public BusinessLogicImplementation() : this(null)
    { }

    internal BusinessLogicImplementation(UnderneathLayerAPI? underneathLayer)
    {
      layerBellow = underneathLayer == null ? UnderneathLayerAPI.GetDataLayer() : underneathLayer;
    }

    #endregion ctor

    #region BusinessLogicAbstractAPI

    public override double TableWidth
    {
      get => layerBellow.BoardWidth;
      set => layerBellow.BoardWidth = value;
    }

    public override double TableHeight
    {
      get => layerBellow.BoardHeight;
      set => layerBellow.BoardHeight = value;
    }

    public override void Dispose()
    {
      if (Disposed)
        throw new ObjectDisposedException(nameof(BusinessLogicImplementation));
      layerBellow.Dispose();
      Disposed = true;
    }

    public override void Start(int numberOfBalls, Action<IPosition, IBall> upperLayerHandler)
    {
      if (Disposed)
        throw new ObjectDisposedException(nameof(BusinessLogicImplementation));
      if (upperLayerHandler == null)
        throw new ArgumentNullException(nameof(upperLayerHandler));
      layerBellow.Start(numberOfBalls, (startingPosition, databall) =>
      {
        var pos = new Position(startingPosition.x, startingPosition.y); //mapowanie StartingPosition do IPosition
        var velocity = layerBellow.CreateVector(databall.Velocity.x, databall.Velocity.y); //pobranie prędkości z warstwy niższej
        var newBall = layerBellow.CreateBall(startingPosition, velocity); //utworzenie instancji IBall
        var businessBall = new Ball(newBall); //utworzenie obiektu AsyncBall na poziomie warstwy logiki
        
        upperLayerHandler(pos, businessBall); //przekazanie do wyższego poziomu
        //TODO: śledzenie w drzewie
      });
    }

    #endregion BusinessLogicAbstractAPI

    #region private

    private bool Disposed = false;

    private readonly UnderneathLayerAPI layerBellow;
    #endregion private

    #region TestingInfrastructure

    [Conditional("DEBUG")]
    internal void CheckObjectDisposed(Action<bool> returnInstanceDisposed)
    {
      returnInstanceDisposed(Disposed);
    }

    #endregion TestingInfrastructure
  }
  
  

}