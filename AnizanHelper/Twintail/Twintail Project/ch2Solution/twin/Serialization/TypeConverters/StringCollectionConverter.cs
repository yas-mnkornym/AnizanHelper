using System;
using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;

namespace Twintail3
{
	/// <summary>
	/// 文字列のコレクションを変換するクラスです。
	/// </summary>
	public class StringCollectionConverter : TypeConverter
	{
		public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
		{
			if (destinationType == typeof(string))
				return true;

			return base.CanConvertTo(context, destinationType);
		}

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
				ArrayListConverter<string> conv = new ArrayListConverter<string>();

				ArrayList al = (ArrayList)conv.ConvertFrom(value);
				StringCollection collection = new StringCollection();

				foreach (string s in al)
					collection.Add(s);

				return collection;
			}

			return base.ConvertFrom(context, culture, value);
		}

		public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
		{
			if (destinationType == typeof(string))
			{
				if (value == null)
					value = new StringCollection();

				ArrayListConverter<string> conv = new ArrayListConverter<string>();

				return conv.ConvertToString(value);
			}

			return base.ConvertTo(context, culture, value, destinationType);
		}
	}
}
