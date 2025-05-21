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
using System.Windows.Input;
using TP.ConcurrentProgramming.Presentation.Model;
using TP.ConcurrentProgramming.Presentation.ViewModel.MVVMLight;
using ModelIBall = TP.ConcurrentProgramming.Presentation.Model.IBall;

namespace TP.ConcurrentProgramming.Presentation.ViewModel
{
    public class MainWindowViewModel : ViewModelBase, IDisposable
    {
        #region ctor

        public MainWindowViewModel() : this(null)
        {
            SetBallsCommand = new RelayCommand(SetBalls, CanSetBalls);
        }

        internal MainWindowViewModel(ModelAbstractApi modelLayerAPI)
        {
            ModelLayer = modelLayerAPI == null ? ModelAbstractApi.CreateModel() : modelLayerAPI;
            Observer = ModelLayer.Subscribe<ModelIBall>(x => Balls.Add(x));
            SetBallsCommand = new RelayCommand(SetBalls, CanSetBalls);
        }

        #endregion ctor

        #region Properties
        private bool BallsSetted = false;

        private string _ballCountInput;
        public string BallCountInput
        {
            get => _ballCountInput;
            set
            {
                if (Set(ref _ballCountInput, value))
                {
                    (SetBallsCommand as RelayCommand)?.RaiseCanExecuteChanged();
                }
            }
        }
        private int BallCount => int.TryParse(BallCountInput, out int result) ? result : 0;

        private double _borderWidth;
        public double BorderWidth
        {
            get => _borderWidth;
            set => Set(ref _borderWidth, value);
        }

        private double _borderHeight;
        public double BorderHeight
        {
            get => _borderHeight;
            set => Set(ref _borderHeight, value);
        }

        private double _borderPadding;
        public double BorderPadding
        {
            get => _borderPadding;
            set => Set(ref _borderPadding, value);
        }
        #endregion

        #region public API

        public void Start(int numberOfBalls)
        {
            if (Disposed)
                throw new ObjectDisposedException(nameof(MainWindowViewModel));
            ModelLayer.Start(numberOfBalls, BorderWidth, BorderHeight, BorderPadding);
            Observer.Dispose();


        }

        public ObservableCollection<ModelIBall> Balls { get; } = new ObservableCollection<ModelIBall>();

        #endregion public API

        #region Commands
        public ICommand SetBallsCommand { get; }

        private void SetBalls()
        {
            if (Disposed)
                throw new ObjectDisposedException(nameof(MainWindowViewModel));

            Balls.Clear();
            this.Start(BallCount);
            BallsSetted = true;
        }
        private bool CanSetBalls()
        {
            if (!BallsSetted)
            {
                if (!int.TryParse(BallCountInput, out int parsedValue))
                    return false;
                return parsedValue > 0 && parsedValue <= 20;
            }
            return false;
        }


        #endregion

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