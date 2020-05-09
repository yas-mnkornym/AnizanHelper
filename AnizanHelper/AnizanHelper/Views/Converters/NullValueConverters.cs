using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace AnizanHelper.Views.Converters
{
	public class NullValueConverterBase<TValue> : IValueConverter
	{
		public TValue NullValue { get; set; }
		public TValue NonNullValue { get; set; }

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return (value is null)
				? this.NullValue
				: this.NonNullValue;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotSupportedException();
		}
	}

	public sealed class NullBoolConverter : NullValueConverterBase<bool>
	{
	}

	public sealed class NullVisibilityConverter : NullValueConverterBase<Visibility>
	{
	}
}
