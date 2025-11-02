using System;
using System.Collections.Generic;

namespace Fractal.Colormaps
{
    /// <summary>“Огненная” раскраска.</summary>
    public class Fire : IColoredImage
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

                    t = Math.Pow(t, 0.6);

                    int rI = (int)Math.Round(255.0 * Math.Min(1.0, t * 3.0));
                    int gI = (int)Math.Round(255.0 * Math.Min(1.0, Math.Max(0.0, (t - 0.33) * 3.0)));
                    int bI = (int)Math.Round(255.0 * Math.Max(0.0, (t - 0.66) * 3.0 * 0.5));

                    if (rI < 0) rI = 0; if (rI > 255) rI = 255;
                    if (gI < 0) gI = 0; if (gI > 255) gI = 255;
                    if (bI < 0) bI = 0; if (bI > 255) bI = 255;

                    row.Add(new Pixel((byte)rI, (byte)gI, (byte)bI));
                }
                colored.Add(row);
            }
            return new Image(colored);
        }
    }
}
