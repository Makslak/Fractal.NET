using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Fractal
{
    public class BurningShipFractal : IFractal
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

            double xMin = (double)imageBox.Box.Xmin;
            double xMax = (double)imageBox.Box.Xmax;
            double yMin = (double)imageBox.Box.Ymin;
            double yMax = (double)imageBox.Box.Ymax;

            double xStep = (w > 1) ? (xMax - xMin) / (w - 1) : 0.0;
            double yStep = (h > 1) ? (yMax - yMin) / (h - 1) : 0.0;

            int[][] rows = new int[h][];
            for (int i = 0; i < h; i++) rows[i] = new int[w];

            Parallel.For(0, h, yIndex =>
            {
                double cy = yMax - yIndex * yStep;
                int[] row = rows[yIndex];

                for (int xIndex = 0; xIndex < w; xIndex++)
                {
                    double cx = xMin + xIndex * xStep;

                    double zr = 0.0, zi = 0.0;
                    int iter = 0;

                    while (iter < maxIter)
                    {
                        double azr = Math.Abs(zr);
                        double azi = Math.Abs(zi);

                        double zr2 = azr * azr - azi * azi + cx;
                        double zi2 = 2.0 * azr * azi + cy;
                        zr = zr2; zi = zi2;

                        if (zr * zr + zi * zi > 4.0) break;
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
