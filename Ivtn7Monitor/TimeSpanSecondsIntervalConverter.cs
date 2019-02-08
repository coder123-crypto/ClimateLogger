// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

using System;
using System.Globalization;
using System.Windows.Data;

namespace Ivtn7Monitor
{
    internal sealed class TimeSpanSecondsIntervalConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is TimeSpan timeSpan)
            {
                return (int) timeSpan.TotalSeconds;
            }

            return Binding.DoNothing;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int seconds)
            {
                return TimeSpan.FromSeconds(seconds);
            }

            return Binding.DoNothing;
        }
    }
}