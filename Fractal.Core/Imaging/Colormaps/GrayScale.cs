using System.Collections.Generic;

namespace Fractal.Colormaps
{
    public class GrayScale : IColoredImage
    {
        public Image Create(FractalData value)
        {
            var counts = value.Counts;
            var colored = new List<List<Pixel>>(counts.Count);

            for (int i = 0; i < counts.Count; i++)
            {
                var row = new List<Pixel>(counts[i].Count);
                for (int j = 0; j < counts[i].Count; j++)
                {
                    double t = (value.MaxIteration > 1)
                        ? (double)counts[i][j] / (value.MaxIteration - 1)
                        : 1.0;

                    int gI = (int)System.Math.Round(255.0 * t);
                    if (gI < 0) gI = 0; if (gI > 255) gI = 255;

                    row.Add(new Pixel((byte)gI, (byte)gI, (byte)gI));
                }
                colored.Add(row);
            }
            return new Image(colored);
        }
    }
}
