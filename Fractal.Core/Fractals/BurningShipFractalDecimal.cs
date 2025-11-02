using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Fractal
{
    /// <summary>Фрактал "Burning Ship" на decimal (медленнее, но десятичная арифметика).</summary>
    public class BurningShipFractalDecimal : IFractal
    {
        public Box2D Box { get; private set; } = new Box2D
        {
            Xmin = -2.2M,
            Xmax = 1.2M,
            Ymin = -2.5M,
            Ymax = 1.5M
        };

        public FractalData Generate(ImageBox imageBox, int? maxIterations)
        {
            const int DefaultMaxIteration = 300;
            int maxIter = maxIterations.HasValue ? maxIterations.Value : DefaultMaxIteration;

            int w = imageBox.Width;
            int h = imageBox.Height;

            decimal xMin = imageBox.Box.Xmin;
            decimal xMax = imageBox.Box.Xmax;
            decimal yMin = imageBox.Box.Ymin;
            decimal yMax = imageBox.Box.Ymax;

            decimal xStep = (w > 1) ? (xMax - xMin) / (w - 1) : 0m;
            decimal yStep = (h > 1) ? (yMax - yMin) / (h - 1) : 0m;

            int[][] rows = new int[h][];
            for (int i = 0; i < h; i++) rows[i] = new int[w];

            Parallel.For(0, h, yIndex =>
            {
                decimal cy = yMax - yIndex * yStep;
                int[] row = rows[yIndex];

                for (int xIndex = 0; xIndex < w; xIndex++)
                {
                    decimal cx = xMin + xIndex * xStep;

                    decimal zr = 0m, zi = 0m;
                    int iter = 0;

                    while (iter < maxIter)
                    {
                        decimal azr = Math.Abs(zr);
                        decimal azi = Math.Abs(zi);

                        decimal zr2 = azr * azr - azi * azi + cx;
                        decimal zi2 = 2m * azr * azi + cy;
                        zr = zr2; zi = zi2;

                        if (zr * zr + zi * zi > 4m) break;
                        iter++;
                    }

                    row[xIndex] = iter;
                }
            });

            var counts = new List<List<int>>(h);
            for (int y = 0; y < h; y++)
            {
                var list = new List<int>(w);
                var src = rows[y];
                for (int x = 0; x < w; x++) list.Add(src[x]);
                counts.Add(list);
            }

            return new FractalData { MaxIteration = maxIter, Counts = counts };
        }
    }
}
