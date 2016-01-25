using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace OSMToSCT2.Helpers
{
    public class BooleanVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            bool boolValue;

            if (value is bool)
            {
                boolValue = (bool)value;

                if (boolValue)
                    return Visibility.Visible;
                else
                    return Visibility.Collapsed;
            }
            else
            {
                throw new ArgumentException("Value must be Boolean");
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            Visibility visValue;

            if (value is Visibility)
            {
                visValue = (Visibility)value;

                switch (visValue)
                {
                    case Visibility.Visible:
                        return true;
                    case Visibility.Hidden:
                    case Visibility.Collapsed:
                        return false;   
                    default:
                        return false;
                }
            }
            else
            {
                throw new ArgumentException("Value must be Visibility");
            }

        }
    }
}
