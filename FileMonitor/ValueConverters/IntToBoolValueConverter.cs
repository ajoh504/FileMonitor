using System;
using System.Globalization;
using System.Windows.Data;

namespace FileMonitor.ValueConverters
{
    /// <summary>
    /// Provides a value converter from <see cref="int"/> to <see cref="bool"/>.
    /// </summary>
    public class IntToBoolValueConverter : IValueConverter
    {
        /// <summary>
        /// A method to convert the provided object to the specified target type. 
        /// <see href=
        /// "https://learn.microsoft.com/en-us/dotnet/api/system.windows.data.ivalueconverter.convert?view=windowsdesktop-7.0#system-windows-data-ivalueconverter-convert(system-object-system-type-system-object-system-globalization-cultureinfo)">
        /// See this link from the Microsoft docs.
        /// </see> 
        /// </summary>
        /// <remarks>
        /// This method is called by a control binding in the MainWindow.xaml file. The <c>DeleteFiles</c> button is
        /// disabled as long as no files have been selected in the <c>FilesDisplayed</c> list view. When one or more
        /// files have been selected, the index is converted to true, allowing the <c>DeleteFiles</c> button to be 
        /// clicked.
        /// </remarks>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is int intValue))
                return false;

            if (intValue < 0)
                return false;

            return true;
        }

        /// <summary>
        /// A method to convert a value from the target type to the source type.
        /// </summary>
        /// <exception cref="NotSupportedException">
        /// Throws an exception if called. This method should not be used.
        /// </exception>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}

