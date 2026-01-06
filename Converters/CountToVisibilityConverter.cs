using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace PcPerformanceManager.Converters;

public class CountToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        bool inverse = parameter?.ToString()?.ToLower() == "inverse";
        
        if (value is int count)
        {
            bool hasItems = count > 0;
            if (inverse)
                return hasItems ? Visibility.Collapsed : Visibility.Visible;
            return hasItems ? Visibility.Visible : Visibility.Collapsed;
        }
        return inverse ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}




