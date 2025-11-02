using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Fractal;            
using Fractal.Colormaps;    

namespace FractalViewer.Services
{
    public class RenderService
    {
        private static IColoredImage CreateColormap(string name)
        {
            if (!string.IsNullOrEmpty(name))
            {
                var n = name.Trim();
                if (n.Equals("Greys", StringComparison.OrdinalIgnoreCase)) return new GrayScale();
                if (n.Equals("Fire", StringComparison.OrdinalIgnoreCase)) return new Fire();
                if (n.Equals("Viridis", StringComparison.OrdinalIgnoreCase)) return new Viridis();
            }
            return new GrayScale();
        }

        private static IFractal CreateFractal(string name, bool useDecimal)
        {
            var n = (name ?? string.Empty).Trim().ToLowerInvariant();
            bool isShip = n.Contains("ship") || n.Contains("кораб");

            if (useDecimal)
                return isShip ? (IFractal)new BurningShipFractalDecimal()
                              : (IFractal)new MandelbrotFractalDecimal();
            else
                return isShip ? (IFractal)new BurningShipFractal()
                              : (IFractal)new MandelbrotFractal();
        }

        private static Box2D GetDefaultBox(IFractal f)
        {
            if (f is MandelbrotFractal m) return m.Box;
            if (f is MandelbrotFractalDecimal md) return md.Box;
            if (f is BurningShipFractal bs) return bs.Box;
            if (f is BurningShipFractalDecimal bsd) return bsd.Box;
            return new Box2D { Xmin = -2m, Xmax = 1m, Ymin = -1.5m, Ymax = 1.5m };
        }

        public Task<BitmapSource> RenderAsync(
            string fractalName, string colormapName,
            int width, int height, int maxIterations,
            CancellationToken ct,
            Box2D boxOverride = null)
        {
            return RenderAsync(fractalName, colormapName, width, height, maxIterations, false, ct, boxOverride);
        }

        public Task<BitmapSource> RenderAsync(
            string fractalName, string colormapName,
            int width, int height, int maxIterations,
            bool useDecimal,
            CancellationToken ct,
            Box2D boxOverride = null)
        {
            return Task.Run<BitmapSource>(delegate
            {
                ct.ThrowIfCancellationRequested();

                var fractal = CreateFractal(fractalName, useDecimal);
                Box2D box = (boxOverride != null) ? boxOverride : GetDefaultBox(fractal);

                var data = fractal.Generate(new ImageBox(width, height, box), maxIterations);
                ct.ThrowIfCancellationRequested();

                var cmap = CreateColormap(colormapName);
                var image = cmap.Create(data);

                var wb = new WriteableBitmap(width, height, 96, 96, PixelFormats.Bgra32, null);
                int stride = width * 4;
                byte[] buffer = new byte[stride * height];

                for (int y = 0; y < height; y++)
                {
                    var row = image.Pixels[y];
                    int o = y * stride;
                    for (int x = 0; x < width; x++)
                    {
                        var p = row[x];
                        buffer[o + x * 4 + 0] = p.B; 
                        buffer[o + x * 4 + 1] = p.G; 
                        buffer[o + x * 4 + 2] = p.R; 
                        buffer[o + x * 4 + 3] = 255; 
                    }
                }

                wb.WritePixels(new System.Windows.Int32Rect(0, 0, width, height), buffer, stride, 0);
                wb.Freeze(); 
                return (BitmapSource)wb;
            }, ct);
        }
    }
}
