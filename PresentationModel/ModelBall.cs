//____________________________________________________________________________________________________________________________________
//
//  Copyright (C) 2023, Mariusz Postol LODZ POLAND.
//
//  To be in touch join the community by pressing the `Watch` button and get started commenting using the discussion panel at
//
//  https://github.com/mpostol/TP/discussions/182
//
//  by introducing yourself and telling us what you do with this community.
//_____________________________________________________________________________________________________________________________________

using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using TP.ConcurrentProgramming.BusinessLogic;
using TP.ConcurrentProgramming.Data;
using LogicIBall = TP.ConcurrentProgramming.BusinessLogic.IBall;

namespace TP.ConcurrentProgramming.Presentation.Model
{
  internal class ModelBall : IBall
  {
    public ModelBall(double top, double left, LogicIBall underneathBall)
    {
      TopBackingField = top;
      LeftBackingField = left;
      _diameter = 80.0;

      underneathBall.NewPositionNotification += NewPositionNotification;
      underneathBall.DiameterChanged += (sender, newDiameter) => Diameter = newDiameter;
    }

    #region IBall

    public double Top
    {
      get => TopBackingField;
      private set
      {
        if (TopBackingField == value)
          return;
        TopBackingField = value;
        RaisePropertyChanged();
      }
    }

    public double Left
    {
      get => LeftBackingField;
      private set
      {
        if (LeftBackingField == value)
          return;
        LeftBackingField = value;
        RaisePropertyChanged();
      }
    }

    public double Diameter
    {
      get => _diameter;
      set
      {
        if (_diameter == value)
          return;
        _diameter = value;
        RaisePropertyChanged();
      }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    #endregion IBall

    #region private

    private double TopBackingField;
    private double LeftBackingField;
    private double _diameter;

    private void NewPositionNotification(object? sender, IVector e)
    {
      Top = e.y;
      Left = e.x;
    }

    private void RaisePropertyChanged([CallerMemberName] string propertyName = "")
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    #endregion private

    #region testing instrumentation

    [Conditional("DEBUG")]
    internal void SetLeft(double x)
    { Left = x; }

    [Conditional("DEBUG")]
    internal void SetTop(double x)
    { Top = x; }

        #endregion testing instrumentation
    }
}