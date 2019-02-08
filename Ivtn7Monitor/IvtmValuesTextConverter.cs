// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

using System;
using System.Globalization;
using System.Windows.Data;
using Ivtn7Monitor.Properties;

namespace Ivtn7Monitor
{
    public sealed class IvtmValuesTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is IvtmValues values)
            {
                return string.Format(Resources.ResultFormat, values.Temperature, values.Humidity, values.Pressure, values.Voltage);
            }

            throw new InvalidOperationException($"value must be {nameof(IvtmValues)}");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}