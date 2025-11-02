using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace FractalViewer.Converters
{
    /// <summary>Строит LinearGradientBrush по имени колормэпа (для превью в UI).</summary>
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

            // локальная функция добавления стопов
            Action<string, double> add = (hex, off) =>
            {
                b.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString(hex), off));
            };

            switch (name)
            {
                case "Viridis":
                    add("#440154", 0.0); add("#3b528b", 0.25); add("#21908d", 0.5); add("#5ec962", 0.75); add("#fde725", 1.0);
                    break;
                case "Plasma":
                    add("#0d0887", 0.0); add("#7e03a8", 0.25); add("#cc4778", 0.5); add("#f89441", 0.75); add("#f0f921", 1.0);
                    break;
                case "Inferno":
                    add("#000004", 0.0); add("#2c105c", 0.25); add("#871a5b", 0.5); add("#e35933", 0.75); add("#fcffa4", 1.0);
                    break;
                case "Magma":
                    add("#000004", 0.0); add("#3b0f70", 0.25); add("#8c2981", 0.5); add("#de4968", 0.75); add("#fbfcbf", 1.0);
                    break;
                case "Turbo":
                    add("#30123b", 0.0); add("#3465d9", 0.25); add("#35c18f", 0.5); add("#f6d743", 0.75); add("#fa2a2a", 1.0);
                    break;
                case "Cividis":
                    add("#00204c", 0.0); add("#2d3a73", 0.25); add("#576490", 0.5); add("#9aa06a", 0.75); add("#ffd166", 1.0);
                    break;
                case "Rainbow":
                    add("#9400D3", 0.0); add("#4B0082", 0.16); add("#0000FF", 0.33); add("#00FF00", 0.5); add("#FFFF00", 0.66); add("#FF7F00", 0.83); add("#FF0000", 1.0);
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
