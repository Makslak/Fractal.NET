using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace FractalViewer.Converters
{
    public class EqualityToBrushMultiConverter : IMultiValueConverter
    {
        private static readonly Brush Selected = new SolidColorBrush(Color.FromRgb(32, 38, 58));
        private static readonly Brush Normal = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            string selected = values.Length > 0 ? values[0] as string : null;
            string current = values.Length > 1 ? values[1] as string : null;
            return string.Equals(selected, current) ? Selected : Normal;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
