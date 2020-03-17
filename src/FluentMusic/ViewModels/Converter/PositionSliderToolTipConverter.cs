using System;
using Windows.UI.Xaml.Data;

namespace FluentMusic.ViewModels.Converter
{
    internal class PositionSliderToolTipConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is double d)
            {
                var ts = TimeSpan.FromSeconds(d);
                return ts.Hours != 0 ? $"{ts:hh\\.m\\:ss}" : $"{ts:m\\:ss}";
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}