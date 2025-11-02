using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Fractal;                
using FractalViewer.Services;

namespace FractalViewer.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly RenderService _render;
        private CancellationTokenSource _cts;

        public ObservableCollection<string> FractalTypes { get; private set; } =
            new ObservableCollection<string> { "Мандельброт", "Корабль (Burning Ship)" };

        public ObservableCollection<string> Colormaps { get; private set; } =
            new ObservableCollection<string> { "Greys", "Fire", "Viridis" };

        private string _selectedFractal;
        public string SelectedFractal
        {
            get { return _selectedFractal; }
            set
            {
                if (_selectedFractal == value) return;
                _selectedFractal = value;
                OnPropertyChanged("SelectedFractal");

                SetDefaultBoxForCurrentFractal();
                _history.Clear();
                OnPropertyChanged("CanGoBack");
            }
        }

        private string _selectedColormap = "Greys";
        public string SelectedColormap
        {
            get { return _selectedColormap; }
            set { if (_selectedColormap != value) { _selectedColormap = value; OnPropertyChanged("SelectedColormap"); } }
        }

        private bool _useDecimalIterations;
        public bool UseDecimalIterations
        {
            get { return _useDecimalIterations; }
            set
            {
                if (_useDecimalIterations == value) return;
                _useDecimalIterations = value;
                OnPropertyChanged("UseDecimalIterations");
            }
        }

        private int _iterations = 300;
        public int Iterations
        {
            get { return _iterations; }
            set
            {
                int v = Math.Max(1, Math.Min(10000, value));
                if (_iterations != v) { _iterations = v; OnPropertyChanged("Iterations"); }
            }
        }

        private int _width = 960;
        public int Width
        {
            get { return _width; }
            set { if (_width != value) { _width = Math.Max(1, value); OnPropertyChanged("Width"); } }
        }

        private int _height = 960;
        public int Height
        {
            get { return _height; }
            set { if (_height != value) { _height = Math.Max(1, value); OnPropertyChanged("Height"); } }
        }

        private BitmapSource _previewImage;
        public BitmapSource PreviewImage
        {
            get { return _previewImage; }
            set { _previewImage = value; OnPropertyChanged("PreviewImage"); }
        }

        private bool _isBusy;
        public bool IsBusy
        {
            get { return _isBusy; }
            private set { _isBusy = value; OnPropertyChanged("IsBusy"); }
        }

        private Box2D _currentBox;
        private readonly Stack<Box2D> _history = new Stack<Box2D>();
        public bool CanGoBack { get { return _history.Count > 0; } }

        public ICommand ApplyCommand { get; private set; }
        public ICommand ResetCommand { get; private set; }
        public ICommand BackCommand { get; private set; }

        public MainViewModel(RenderService render)
        {
            _render = render;

            SelectedFractal = "Мандельброт";
            SelectedColormap = "Greys";
            UseDecimalIterations = false;

            SetDefaultBoxForCurrentFractal();

            ApplyCommand = new AsyncRelayCommand(ApplyAsync, delegate { return !IsBusy; });
            ResetCommand = new RelayCommand(Reset);
            BackCommand = new AsyncRelayCommand(BackAsync);
        }

        private async Task ApplyAsync()
        {
            Cancel();
            _cts = new CancellationTokenSource();
            try
            {
                IsBusy = true;
                PreviewImage = await _render.RenderAsync(
                    SelectedFractal, SelectedColormap,
                    Width, Height, Iterations,
                    UseDecimalIterations,
                    _cts.Token,
                    _currentBox
                );
            }
            finally { IsBusy = false; }
        }

        private void Cancel()
        {
            if (_cts != null && !_cts.IsCancellationRequested)
                _cts.Cancel();
        }

        public async Task ZoomToPixelsAsync(int x0, int y0, int x1, int y1, int imgW, int imgH)
        {
            if (imgW <= 1 || imgH <= 1) return;
            if (x0 == x1 || y0 == y1) return;
            if (x0 > x1) { int t = x0; x0 = x1; x1 = t; }
            if (y0 > y1) { int t = y0; y0 = y1; y1 = t; }

            _history.Push(new Box2D { Xmin = _currentBox.Xmin, Xmax = _currentBox.Xmax, Ymin = _currentBox.Ymin, Ymax = _currentBox.Ymax });
            OnPropertyChanged("CanGoBack");

            decimal xRange = _currentBox.Xmax - _currentBox.Xmin;
            decimal yRange = _currentBox.Ymax - _currentBox.Ymin;

            decimal u0 = (decimal)x0 / (imgW - 1);
            decimal u1 = (decimal)x1 / (imgW - 1);
            decimal v0 = (decimal)y0 / (imgH - 1);
            decimal v1 = (decimal)y1 / (imgH - 1);

            decimal newXmin = _currentBox.Xmin + u0 * xRange;
            decimal newXmax = _currentBox.Xmin + u1 * xRange;
            decimal newYmax = _currentBox.Ymax - v0 * yRange;
            decimal newYmin = _currentBox.Ymax - v1 * yRange;

            _currentBox = new Box2D { Xmin = newXmin, Xmax = newXmax, Ymin = newYmin, Ymax = newYmax };

            await ApplyAsync();
        }

        private async Task BackAsync()
        {
            if (_history.Count == 0) return;
            _currentBox = _history.Pop();
            OnPropertyChanged("CanGoBack");
            await ApplyAsync();
        }

        private void Reset()
        {
            Iterations = 300;
            Width = 960;
            Height = 960;
            SelectedFractal = SelectedFractal;
            SelectedColormap = SelectedColormap;
            UseDecimalIterations = false;
        }

        private void SetDefaultBoxForCurrentFractal()
        {
            string n = (_selectedFractal ?? "").ToLowerInvariant();
            bool isShip = n.Contains("ship") || n.Contains("кораб");

            if (isShip)
            {
                if (UseDecimalIterations)
                {
                    var f = new BurningShipFractalDecimal();
                    _currentBox = new Box2D { Xmin = f.Box.Xmin, Xmax = f.Box.Xmax, Ymin = f.Box.Ymin, Ymax = f.Box.Ymax };
                }
                else
                {
                    var f = new BurningShipFractal();
                    _currentBox = new Box2D { Xmin = f.Box.Xmin, Xmax = f.Box.Xmax, Ymin = f.Box.Ymin, Ymax = f.Box.Ymax };
                }
            }
            else
            {
                if (UseDecimalIterations)
                {
                    var f = new MandelbrotFractalDecimal();
                    _currentBox = new Box2D { Xmin = f.Box.Xmin, Xmax = f.Box.Xmax, Ymin = f.Box.Ymin, Ymax = f.Box.Ymax };
                }
                else
                {
                    var f = new MandelbrotFractal();
                    _currentBox = new Box2D { Xmin = f.Box.Xmin, Xmax = f.Box.Xmax, Ymin = f.Box.Ymin, Ymax = f.Box.Ymax };
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string name)
        {
            var h = PropertyChanged;
            if (h != null) h(this, new PropertyChangedEventArgs(name));
        }
    }

    #region Commands
    public class RelayCommand : ICommand
    {
        private readonly Action _action;
        private readonly Func<bool> _canExecute;
        public RelayCommand(Action action, Func<bool> canExecute = null)
        {
            _action = action; _canExecute = canExecute;
        }
        public bool CanExecute(object parameter) { return _canExecute == null ? true : _canExecute(); }
        public void Execute(object parameter) { _action(); }
        public event EventHandler CanExecuteChanged { add { } remove { } }
    }

    public class AsyncRelayCommand : ICommand
    {
        private readonly Func<Task> _action;
        private readonly Func<bool> _canExecute;
        public AsyncRelayCommand(Func<Task> action, Func<bool> canExecute = null)
        {
            _action = action; _canExecute = canExecute;
        }
        public bool CanExecute(object parameter) { return _canExecute == null ? true : _canExecute(); }
        public async void Execute(object parameter) { await _action(); }
        public event EventHandler CanExecuteChanged { add { } remove { } }
    }
    #endregion
}
