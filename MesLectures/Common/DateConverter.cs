using System;
using System.Globalization;
using Windows.UI.Xaml.Data;

namespace MesLectures.Common
{
    public class DateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var date = (DateTime)value;
            IFormatProvider culture = new CultureInfo(Settings.GetRessource("Locale"));
            return date.ToString("MMMM yyyy", culture);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            var strValue = value as string;
            DateTime resultDateTime;
            if (DateTime.TryParse(strValue, out resultDateTime))
            {
                return resultDateTime;
            }

            throw new Exception(
                "Unable to convert string to date time");
        }
    }
}