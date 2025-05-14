//__________________________________________________________________________________________
//
//  Copyright 2024 Mariusz Postol LODZ POLAND.
//
//  To be in touch join the community by pressing the `Watch` button and to get started
//  comment using the discussion panel at
//  https://github.com/mpostol/TP/discussions/182
//__________________________________________________________________________________________

using System;
using System.Collections.ObjectModel;
using System.Threading;
using TP.ConcurrentProgramming.Presentation.Model;
using TP.ConcurrentProgramming.Presentation.ViewModel.MVVMLight;
using ModelIBall = TP.ConcurrentProgramming.Presentation.Model.IBall;

namespace TP.ConcurrentProgramming.Presentation.ViewModel
{
  public class MainWindowViewModel : ViewModelBase, IDisposable
  {
    #region ctor

    public MainWindowViewModel() : this(null)
    { }

    private String numberOfBalls = "";
    public RelayCommand StartCommand { get; }
    public RelayCommand StopCommand { get; }

    public String _numberOfBalls
    {
        get => numberOfBalls;
        set
        {
          numberOfBalls = value;
          RaisePropertyChanged();
        }
    }
        private double windowWidth;
        public double WindowWidth
        {
            get => windowWidth;
            set
            {
                windowWidth = value;
                RaisePropertyChanged();
                ModelLayer.BoardWidth = value;
            }
        }
        private double windowHeight;
        public double WindowHeight
        {
            get => windowHeight;
            set
            {
                windowHeight = value;
                RaisePropertyChanged();
                ModelLayer.BoardHeight = value;
            }
        }
    internal MainWindowViewModel(ModelAbstractApi modelLayerAPI)
    {
      ModelLayer = modelLayerAPI ?? ModelAbstractApi.CreateModel();
      var context = SynchronizationContext.Current;
      Observer = ModelLayer.Subscribe<ModelIBall>(x =>
      {
        if (SynchronizationContext.Current == context)
          Balls.Add(x);
        else
          context.Post(_ => Balls.Add(x), null);
      });
      StartCommand = new RelayCommand(Start);
      StopCommand = new RelayCommand(Stop);

      WindowWidth = 1920;
      WindowHeight = 1080;

      ModelLayer.SetCanvasSize(WindowWidth, WindowHeight);
    }

        #endregion ctor

        #region public API
        public void Start()
        {
            if (Disposed)
                throw new ObjectDisposedException(nameof(MainWindowViewModel));
            if (int.TryParse(this.numberOfBalls, out int ballsCount))
            {
                Balls.Clear();
                ModelLayer.Start(ballsCount);
            }
        }

        public void Stop()
        {
            Balls.Clear();
        }
        public ObservableCollection<ModelIBall> Balls { get; } = new ObservableCollection<ModelIBall>();

        public void UpdateCanvasSize(double windowWidth, double windowHeight)
        {
          double marginLeft = 211;
          double marginRight = 60;
          double marginTop = 10;
          double marginBottom = 10;

          double borderThickness = 4;

          double contentWidth = windowWidth - marginLeft - marginRight - (2 * borderThickness);
          double contentHeight = windowHeight - marginTop - marginBottom - (2 * borderThickness);

          ModelLayer.SetCanvasSize(contentWidth, contentHeight);
        }

#endregion public API

    #region IDisposable

    protected virtual void Dispose(bool disposing)
    {
      if (!Disposed)
      {
        if (disposing)
        {
          Balls.Clear();
          Observer.Dispose();
          ModelLayer.Dispose();
        }

        // TODO: free unmanaged resources (unmanaged objects) and override finalizer
        // TODO: set large fields to null
        Disposed = true;
      }
    }

    public void Dispose()
    {
      if (Disposed)
        throw new ObjectDisposedException(nameof(MainWindowViewModel));
      Dispose(disposing: true);
      GC.SuppressFinalize(this);
    }

    #endregion IDisposable

    #region private

    private IDisposable Observer = null;
    private ModelAbstractApi ModelLayer;
    private bool Disposed = false;

    #endregion private
  }
}