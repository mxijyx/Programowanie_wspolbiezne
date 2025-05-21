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
    //public abstract double BoardWidth { get; set; }
    //public abstract double BoardHeight { get; set; }
        
    //public abstract void SetCanvasSize(double width, double height);
    //public abstract List<IBall> CreateBalls(int count, double boardWidth, double boardHeight, double minMass, double maxMass);

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
    double x { get; init; }

    /// <summary>
    /// The y component of the vector.
    /// </summary>
    double y { get; init; }
  }
    
    public interface IPosition
    {
        double x { get; set; }
        double y { get; set; }
    }

    public interface IBall
  {
        // add instead of velocity setter according to the seminar SetVelocity(double velocity); -> chyba zrobione 
        // = tu nie można nic usunac ani dostawić 
        event EventHandler<IVector> NewVelocityNotification;
        event EventHandler<IVector> NewPositionNotification;
        //event EventHandler<double> DiameterChanged;
        void SetVelocity(double x, double y); // spójność danych! 
        void SetPosition(double x, double y);

        //wątki tworzymy według threadcreation.cs
        //taski nie są wymagane 
        //średnicy masy kuli też nie trzeba brać 
        //jeśli stąd coś wyeksportujemy to musimy to uzasadnić - dlatego tu już lepiej NIC NIE ZMIENIAĆ -> ROZWIĄZAUJEMY PROBLEM PRZEZ DANE IMMUTABLE NP. REKORD 

        //czy to w ogóle potrzebne?
        IVector Velocity { get; } //TODO: dodaj private set
        IVector Position { get; }
        double Mass { get; set; } 
        double Diameter { get; }
        public void Stop();
  }
}