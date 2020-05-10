using System;
using System.ComponentModel;
using System.Globalization;

namespace Twintail3
{
	/// <summary>
	/// byte配列のデータをbase64に変換するクラスです。
	/// </summary>
	public class Base64Converter : TypeConverter
	{
		public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
		{
			if (sourceType == typeof(string))
				return true;

			return base.CanConvertFrom(context, sourceType);
		}

		public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
		{
			if (value is string)
			{
				return Convert.FromBase64String(value.ToString());
			}

			return base.ConvertFrom(context, culture, value);
		}

		public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
		{
			if (destinationType == typeof(string))
			{
				if (value == null)
					value = new byte[0];

				return Convert.ToBase64String((byte[])value);
			}

			return base.ConvertTo(context, culture, value, destinationType);
		}
	}
}
