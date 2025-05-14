//__________________________________________________________________________________________
//
//  Copyright 2024 Mariusz Postol LODZ POLAND.
//
//  To be in touch join the community by pressing the `Watch` button and to get started
//  comment using the discussion panel at
//  https://github.com/mpostol/TP/discussions/182
//__________________________________________________________________________________________

using System;
using System.Windows;
using TP.ConcurrentProgramming.Presentation.ViewModel;

namespace TP.ConcurrentProgramming.PresentationView
{
  /// <summary>
  /// View implementation
  /// </summary>
  public partial class MainWindow : Window
  {
    public MainWindow()
    {
      Random random = new Random();
      InitializeComponent();
      MainWindowViewModel viewModel = (MainWindowViewModel)DataContext;
      double screenWidth = SystemParameters.PrimaryScreenWidth;
      double screenHeight = SystemParameters.PrimaryScreenHeight -20;
            this.Width = screenWidth;
            this.Height = screenHeight;
            viewModel.WindowWidth = this.Width;
            viewModel.WindowHeight = this.Height;

            viewModel.UpdateCanvasSize(Width, Height);

            this.SizeChanged += (s, e) =>
            {
                viewModel.WindowWidth = this.Width;
                viewModel.WindowHeight = this.Height;

                viewModel.UpdateCanvasSize(Width, Height);
            };

            viewModel.Start();
    }

    /// <summary>
    /// Raises the <seealso cref="System.Windows.Window.Closed"/> event.
    /// </summary>
    /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
    protected override void OnClosed(EventArgs e)
    {
      if (DataContext is MainWindowViewModel viewModel)
        viewModel.Dispose();
      base.OnClosed(e);
    }
  }
}