using System;
using System.Windows;
using System.Windows.Data;

namespace AnizanHelper.Views.Converters
{

	public sealed class BoolVisibilityConverter : IValueConverter
	{
		public BoolVisibilityConverter()
		{
			TrueVisibility = Visibility.Visible;
			FalseVisibility = Visibility.Collapsed;
		}

		public Visibility TrueVisibility { get; set; }
		
		public Visibility FalseVisibility { get; set; }
		
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			return ((bool)value ? TrueVisibility : FalseVisibility);
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			throw new NotSupportedException();
		}
	}
}
