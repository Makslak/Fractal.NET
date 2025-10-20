using Fractal;
using Fractal.Abstractions;
using Fractal.Entities;
using Fractal.Entities.Base;
using Fractal.Entities.ColoredImages;
using Fractal.Entities.Writers;

internal class Program
{
    static void Main(string[] args)
    {
        var screen = (Nx: 1920, Ny: 1080); // FullHD
        // Default MaxIteration - видимо, должно быть свойством каждого фрактала, а не общим?
        int maxIter = 300;
        string filename = "FractalMandelbrot.ppm";

        var fractal = new FractalMandelbrot();
        // Создадим бокс картинки для 2D бокса фрактала:
        var imBox = new ImageBox(screen, fractal.box);
        // Посчитаем фрактал для всех точек бокса картинки:
        var data = fractal.Generate(imBox, maxIter);
        // Пересчитаем counts в цвета картинки:
        IColoredImage palette = new Fire();
        var img = palette.Create(data);
        PPMWriter.Save(img, filename);
        // Вопрос: может перенести Save внутрь Image?
        // img.Save(string filename, string format?=null)
    }
}
