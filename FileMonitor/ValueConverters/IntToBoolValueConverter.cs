﻿using System;
using System.Globalization;
using System.Windows.Data;

namespace FileMonitor.ValueConverters
{
    public class IntToBoolValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is int intValue))
                return false;

            if (intValue == -1)
                return false;

            return true;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
