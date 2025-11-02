using System.Collections.Generic;
using System.Threading.Tasks;

namespace Fractal
{
    public class MandelbrotFractalDecimal : IFractal
    {
        public Box2D Box { get; private set; } = new Box2D
        {
            Xmax = 1.25M,
            Xmin = -2.25M,
            Ymax = 1.75M,
            Ymin = -1.75M,
        };

        public FractalData Generate(ImageBox imageBox, int? maxIterations)
        {
            const int DefaultMaxIteration = 300;
            int maxIter = maxIterations.HasValue ? maxIterations.Value : DefaultMaxIteration;

            decimal xMin = imageBox.Box.Xmin;
            decimal xMax = imageBox.Box.Xmax;
            decimal yMin = imageBox.Box.Ymin;
            decimal yMax = imageBox.Box.Ymax;

            int w = imageBox.Width;
            int h = imageBox.Height;

            decimal xStep = (w > 1) ? (xMax - xMin) / (w - 1) : 0m;
            decimal yStep = (h > 1) ? (yMax - yMin) / (h - 1) : 0m;

            int[][] rows = new int[h][];
            for (int y = 0; y < h; y++) rows[y] = new int[w];

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
                        decimal zr2 = zr * zr;
                        decimal zi2 = zi * zi;
                        if (zr2 + zi2 > 4m) break;

                        decimal newZr = zr2 - zi2 + cx;
                        decimal newZi = 2m * zr * zi + cy;
                        zr = newZr;
                        zi = newZi;

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
