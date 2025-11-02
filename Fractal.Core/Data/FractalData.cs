using System.Collections.Generic;

namespace Fractal
{
    /// <summary>Результаты расчёта фрактала: число итераций по каждой точке.</summary>
    public class FractalData
    {
        public int MaxIteration { get; set; }
        public List<List<int>> Counts { get; set; }
    }
}
