using System;
using System.Collections.Generic;
using System.Numerics;

namespace Fractal
{
    /// <summary>Фрактал Мандельброта.</summary>
    public class MandelbrotFractal : IFractal
    {
        public Box2D Box { get; private set; } = new Box2D
        {
            Xmax = 1.25M,
            Xmin = -2.25M,
            Ymax = 1.75M,
            Ymin = -1.75M,
        };

        protected virtual Func<Complex, Complex, Complex> Z()
        {
            return (z0, c) => z0 * z0 + c;
        }

        public virtual FractalData Generate(ImageBox imageBox, int? maxIterations)
        {
            const int DefaultMaxIteration = 300;
            int maxIter = maxIterations.HasValue ? maxIterations.Value : DefaultMaxIteration;

            // Границы в double (чтобы не кастовать в цикле)
            double xMin = (double)imageBox.Box.Xmin;
            double xMax = (double)imageBox.Box.Xmax;
            double yMin = (double)imageBox.Box.Ymin;
            double yMax = (double)imageBox.Box.Ymax;

            int w = imageBox.Width;
            int h = imageBox.Height;

            double xStep = (xMax - xMin) / (w - 1);
            double yStep = (yMax - yMin) / (h - 1);

            // Массивы под параллельную запись
            int[][] rows = new int[h][];
            for (int y = 0; y < h; y++) rows[y] = new int[w];

            System.Threading.Tasks.Parallel.For(0, h, yIndex =>
            {
                double cy = yMax - yIndex * yStep;  // сверху вниз
                int[] row = rows[yIndex];

                for (int xIndex = 0; xIndex < w; xIndex++)
                {
                    double cx = xMin + xIndex * xStep;

                    double zr = 0.0, zi = 0.0;
                    int iter = 0;

                    // z = z^2 + c; escape radius 2 => r^2 <= 4
                    while (iter < maxIter)
                    {
                        double zr2 = zr * zr;
                        double zi2 = zi * zi;
                        if (zr2 + zi2 > 4.0) break;

                        double newZr = zr2 - zi2 + cx;
                        double newZi = 2.0 * zr * zi + cy;
                        zr = newZr;
                        zi = newZi;

                        iter++;
                    }

                    row[xIndex] = iter;
                }
            });

            // Перегоняем в List<List<int>> для совместимости с остальным кодом
            var counts = new System.Collections.Generic.List<System.Collections.Generic.List<int>>(h);
            for (int y = 0; y < h; y++)
            {
                var list = new System.Collections.Generic.List<int>(w);
                var src = rows[y];
                for (int x = 0; x < w; x++) list.Add(src[x]);
                counts.Add(list);
            }

            return new FractalData { MaxIteration = maxIter, Counts = counts };
        }

    }
}
