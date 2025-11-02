namespace Fractal
{
    public class ImageBox
    {
        public int Width { get; private set; }
        public int Height { get; private set; }
        public Box2D Box { get; private set; }

        public ImageBox(int width, int height, Box2D box)
        {
            Width = width;
            Height = height;
            Box = box;
        }
    }
}
