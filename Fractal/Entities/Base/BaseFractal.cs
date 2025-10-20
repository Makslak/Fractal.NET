using Fractal.Abstractions;
using Fractal.ValueObjects;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace Fractal.Entities.Base;

/// <summary>
/// Базовый класс фрактала, по умолчанию фрактал Мандельброта, 
/// в случае необходимости все нужные методы переопределяются в наследниках
/// </summary>
class FractalMandelbrot : IFractal
{
    // internal readonly Box2D box;
    public readonly Box2D box = new Box2D()
    {
        Xmax = 1.25M,
        Xmin = -2.25M,
        Ymax = 1.75M,
        Ymin = -1.75M,
    };

    protected virtual Func<Complex, Complex, Complex> Z() =>
        (z0, c) => z0 * z0 + c;

    public virtual FractalData Generate(ImageBox imageBox, int? maxIterations) 
    {
        const int MaxIteration = 300; // Перенести потом в лучшее место!
        var maxIter = maxIterations ?? MaxIteration;
        Box2D computingBox = imageBox.Box;
        var z0 = new Complex(0,0);
        var z = z0;

        int xSize = imageBox.Screen.Nx;
        var xStep = (computingBox.Xmax - computingBox.Xmin) / (xSize - 1);

        int ySize = imageBox.Screen.Ny;
        var yStep = (computingBox.Ymax - computingBox.Ymin) / (ySize - 1);

        var counts = new List<List<int>>(ySize);
        
        for (int yIndex = 0; yIndex < ySize; yIndex++)
        {
            var y = computingBox.Ymax - yIndex * yStep;
            var row = new List<int>(xSize);

            for (int xIndex = 0; xIndex < xSize; xIndex++)
            {
                var x = computingBox.Xmin + xIndex * xStep;
                var c = new Complex((double)x, (double)y);

                z = z0;
                int iter = 0;

                while (iter < maxIter && z.Magnitude <= 2.0)
                {
                    z = Z().Invoke(z, c);
                    iter++;
                }

                row.Add(iter);
            }

            counts.Add(row);
        }

        return new FractalData()
        {
            MaxIteration = maxIter,
            Counts = counts,
        };
    }
}