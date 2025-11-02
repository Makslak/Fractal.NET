using System.Collections.Generic;

namespace Fractal
{
    public class Image
    {
        public List<List<Pixel>> Pixels { get; private set; }

        public Image(List<List<Pixel>> pixels)
        {
            Pixels = pixels;
        }
    }
}
