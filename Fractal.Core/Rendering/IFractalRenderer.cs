using System.Threading;

namespace Fractal.Rendering
{
    public interface IFractalRenderer
    {
        Image Render(IFractal fractal, ImageBox box, int maxIterations, IColoredImage colormap, CancellationToken ct);
    }
}
