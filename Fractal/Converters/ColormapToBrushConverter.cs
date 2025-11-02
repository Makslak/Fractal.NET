using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace FractalViewer.Converters
{
    public class ColormapToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string name = value as string ?? "Greys";
            return CreateBrush(name);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public static Brush CreateBrush(string name)
        {
            var b = new LinearGradientBrush();
            b.StartPoint = new System.Windows.Point(0, 0.5);
            b.EndPoint = new System.Windows.Point(1, 0.5);

            Action<string, double> add = (hex, off) =>
            {
                b.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString(hex), off));
            };

            switch (name)
            {
                case "Viridis":
                    add("#440154", 0.0); add("#3b528b", 0.25); add("#21908d", 0.5); add("#5ec962", 0.75); add("#fde725", 1.0);
                    break;
                case "Greys":
                    add("#000000", 0.0); add("#777777", 0.5); add("#FFFFFF", 1.0);
                    break;
                case "Fire":
                default:
                    add("#000000", 0.0); add("#8a1c00", 0.33); add("#ff7f00", 0.66); add("#ffff9f", 1.0);
                    break;
            }
            return b;
        }
    }
}
