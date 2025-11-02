using System;
using System.Collections.Generic;

namespace Fractal.Colormaps
{
    /// <summary>Colormap Viridis (5 опорных точек, линейная интерполяция).</summary>
    public class Viridis : IColoredImage
    {
        // Опорные цвета из твоего превью (Hex -> RGB)
        private static readonly byte[,] Stops = new byte[,]
        {
            { 0x44, 0x01, 0x54 }, // #440154
            { 0x3B, 0x52, 0x8B }, // #3b528b
            { 0x21, 0x90, 0x8D }, // #21908d
            { 0x5E, 0xC9, 0x62 }, // #5ec962
            { 0xFD, 0xE7, 0x25 }, // #fde725
        };

        public Image Create(FractalData value)
        {
            var counts = value.Counts;
            int h = counts.Count;
            int w = counts[0].Count;

            var pixels = new List<List<Pixel>>(h);
            int maxIter = Math.Max(1, value.MaxIteration - 1);
            int segs = Stops.GetLength(0) - 1;

            for (int y = 0; y < h; y++)
            {
                var row = new List<Pixel>(w);
                for (int x = 0; x < w; x++)
                {
                    double t = (double)counts[y][x] / maxIter; // 0..1
                    if (t < 0) t = 0; if (t > 1) t = 1;

                    double pos = t * segs;
                    int i = (int)Math.Floor(pos);
                    if (i >= segs) i = segs - 1;
                    double f = pos - i; // 0..1 в пределах сегмента

                    byte r = (byte)Math.Round(Stops[i, 0] * (1 - f) + Stops[i + 1, 0] * f);
                    byte g = (byte)Math.Round(Stops[i, 1] * (1 - f) + Stops[i + 1, 1] * f);
                    byte b = (byte)Math.Round(Stops[i, 2] * (1 - f) + Stops[i + 1, 2] * f);

                    row.Add(new Pixel(r, g, b));
                }
                pixels.Add(row);
            }
            return new Image(pixels);
        }
    }
}
