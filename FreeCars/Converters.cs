using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace FreeCars {
	public class BoolToVisibilityConverter : IValueConverter {
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
			var result = (bool) value
				       ? Visibility.Visible
				       : Visibility.Collapsed;
			try 
			{
				if ("Invert" == (string)parameter) result = (Visibility)Convert(!(bool)value, targetType, null, culture);
			} catch {}
			return result;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
			return true;
		}
	}
}
