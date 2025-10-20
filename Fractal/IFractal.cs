namespace Fractal;

public interface IFractal
{
    FractalData Generate(ImageBox? box, int? maxIterations);
}
