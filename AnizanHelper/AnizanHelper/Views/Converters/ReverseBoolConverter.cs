﻿using System;
using System.Globalization;
using System.Windows.Data;

namespace AnizanHelper.Views.Converters
{
	public sealed class ReverseBoolConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return !(bool)value;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotSupportedException();
		}
	}
}
