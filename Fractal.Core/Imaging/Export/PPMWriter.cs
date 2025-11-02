using System;
using System.IO;

namespace Fractal.Writers
{
    public class PPMWriter
    {
        public static void Save(Fractal.Image image, string path)
        {
            if (image == null) throw new ArgumentNullException(nameof(image));
            if (image.Pixels == null || image.Pixels.Count == 0)
                throw new ArgumentException("Image.Pixels пуст.", nameof(image));

            int height = image.Pixels.Count;
            int width = image.Pixels[0].Count;

            using (var fs = new FileStream(path, FileMode.Create, FileAccess.Write))
            using (var bw = new BinaryWriter(fs))
            {
                var header = string.Format("P6\n{0} {1}\n255\n", width, height);
                bw.Write(System.Text.Encoding.ASCII.GetBytes(header));

                for (int y = 0; y < height; y++)
                {
                    var row = image.Pixels[y];
                    for (int x = 0; x < width; x++)
                    {
                        var p = row[x];
                        bw.Write(p.R);
                        bw.Write(p.G);
                        bw.Write(p.B);
                    }
                }
            }
        }
    }
}
