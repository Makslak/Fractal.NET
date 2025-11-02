using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using FractalViewer.Services;
using FractalViewer.ViewModels;

namespace FractalViewer
{
    public partial class MainWindow : Window
    {
        private Point? _dragStart;
        private System.Windows.Rect _imgRect;
        private double _scale = 1.0;

        public MainWindow()
        {
            InitializeComponent();

            var vm = new MainViewModel(new RenderService());
            this.DataContext = vm;
            this.Loaded += (s, e) => vm.ApplyCommand.Execute(null);

            PreviewHost.MouseLeftButtonDown += PreviewHost_MouseLeftButtonDown;
            PreviewHost.MouseMove += PreviewHost_MouseMove;
            PreviewHost.MouseLeftButtonUp += PreviewHost_MouseLeftButtonUp;
            PreviewHost.SizeChanged += (s, e) => UpdateImageLayoutCache();
        }

        private void UpdateImageLayoutCache()
        {
            var bmp = Preview.Source as BitmapSource;
            if (bmp == null) { _imgRect = System.Windows.Rect.Empty; _scale = 1.0; return; }

            double cw = Preview.ActualWidth, ch = Preview.ActualHeight;
            double iw = bmp.PixelWidth, ih = bmp.PixelHeight;
            if (cw <= 0 || ch <= 0 || iw <= 0 || ih <= 0) { _imgRect = System.Windows.Rect.Empty; _scale = 1.0; return; }

            _scale = Math.Min(cw / iw, ch / ih);
            double dw = iw * _scale, dh = ih * _scale;
            double ox = (cw - dw) / 2.0, oy = (ch - dh) / 2.0;
            _imgRect = new System.Windows.Rect(ox, oy, dw, dh);
        }

        private Point ClampToImage(Point p)
        {
            return new Point(
                Math.Max(_imgRect.X, Math.Min(_imgRect.Right, p.X)),
                Math.Max(_imgRect.Y, Math.Min(_imgRect.Bottom, p.Y))
            );
        }

        private bool TryControlToPixel(Point pControl, out int px, out int py)
        {
            px = py = 0;
            var bmp = Preview.Source as BitmapSource;
            if (bmp == null || _imgRect.IsEmpty || _scale <= 0.0) return false;

            var p = ClampToImage(pControl);
            double xInImg = (p.X - _imgRect.X) / _scale;
            double yInImg = (p.Y - _imgRect.Y) / _scale;

            px = (int)Math.Round(xInImg);
            py = (int)Math.Round(yInImg);

            px = Math.Max(0, Math.Min(bmp.PixelWidth - 1, px));
            py = Math.Max(0, Math.Min(bmp.PixelHeight - 1, py));
            return true;
        }

        private void PreviewHost_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (Preview.Source == null) return;
            UpdateImageLayoutCache();

            _dragStart = e.GetPosition(Preview);
            SelectionRect.Width = 0;
            SelectionRect.Height = 0;
            SelectionRect.Visibility = Visibility.Visible;

            PreviewHost.CaptureMouse();
        }

        private void PreviewHost_MouseMove(object sender, MouseEventArgs e)
        {
            if (_dragStart == null) return;

            Point p0Prev = ClampToImage(_dragStart.Value);
            Point p1Prev = ClampToImage(e.GetPosition(Preview));

            Point p0 = Preview.TranslatePoint(p0Prev, SelectionCanvas);
            Point p1 = Preview.TranslatePoint(p1Prev, SelectionCanvas);

            double x = Math.Min(p0.X, p1.X);
            double y = Math.Min(p0.Y, p1.Y);
            double w = Math.Abs(p1.X - p0.X);
            double h = Math.Abs(p1.Y - p0.Y);

            System.Windows.Controls.Canvas.SetLeft(SelectionRect, x);
            System.Windows.Controls.Canvas.SetTop(SelectionRect, y);
            SelectionRect.Width = w;
            SelectionRect.Height = h;
        }

        private async void PreviewHost_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_dragStart == null) return;
            PreviewHost.ReleaseMouseCapture();

            Point p0 = _dragStart.Value;
            Point p1 = e.GetPosition(Preview);
            _dragStart = null;

            SelectionRect.Visibility = Visibility.Collapsed;

            if (!TryControlToPixel(p0, out int x0, out int y0)) return;
            if (!TryControlToPixel(p1, out int x1, out int y1)) return;
            if (Math.Abs(x1 - x0) < 3 || Math.Abs(y1 - y0) < 3) return;

            var vm = this.DataContext as ViewModels.MainViewModel;
            var bmp = Preview.Source as BitmapSource;
            if (vm != null && bmp != null)
            {
                // 2) Подгоняем выделение к нужному соотношению сторон (см. ниже)
                AdjustToAspect(ref x0, ref y0, ref x1, ref y1, bmp.PixelWidth, bmp.PixelHeight);

                await vm.ZoomToPixelsAsync(x0, y0, x1, y1, bmp.PixelWidth, bmp.PixelHeight);
            }
        }

        private void PaletteItem_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var fe = sender as FrameworkElement;
            var vm = this.DataContext as MainViewModel;
            if (fe != null && vm != null)
                vm.SelectedColormap = fe.DataContext as string;
        }

        private void AdjustToAspect(ref int x0, ref int y0, ref int x1, ref int y1, int imgW, int imgH)
        {
            int minX = Math.Min(x0, x1), maxX = Math.Max(x0, x1);
            int minY = Math.Min(y0, y1), maxY = Math.Max(y0, y1);
            int selW = maxX - minX + 1;
            int selH = maxY - minY + 1;
            if (selW <= 0 || selH <= 0) return;

            double target = (double)imgW / imgH;
            double selAR = (double)selW / selH;
            int cx = (minX + maxX) / 2;
            int cy = (minY + maxY) / 2;

            int newW = selW, newH = selH;

            if (selAR < target)
            {
                newW = (int)Math.Round(target * selH);
                if (newW > imgW) { newW = imgW; newH = (int)Math.Round(newW / target); }
            }
            else if (selAR > target)
            {
                newH = (int)Math.Round(selW / target);
                if (newH > imgH) { newH = imgH; newW = (int)Math.Round(newH * target); }
            }

            int nx0 = cx - newW / 2;
            int ny0 = cy - newH / 2;
            int nx1 = nx0 + newW - 1;
            int ny1 = ny0 + newH - 1;

            if (nx0 < 0) { nx0 = 0; nx1 = newW - 1; }
            if (ny0 < 0) { ny0 = 0; ny1 = newH - 1; }
            if (nx1 > imgW - 1) { nx1 = imgW - 1; nx0 = imgW - newW; }
            if (ny1 > imgH - 1) { ny1 = imgH - 1; ny0 = imgH - newH; }

            x0 = nx0; x1 = nx1; y0 = ny0; y1 = ny1;
        }

    }
}
