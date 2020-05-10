using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

namespace Twintail3
{
	/// <summary>
	/// ApplicationSettingsSerializerを継承しているクラスを文字列に変換/文字列から復元します。
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class ASSConverter<T> : TypeConverter
		where T : new()
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
				List<T> list = new List<T>();
				string[] elements = Regex.Split(value.ToString(), "<HEAD/>");
				
				foreach (string partial in elements)
				{
					T newObj = new T();

					StringReader sr = new StringReader(partial);
					XmlTextReader xmlIn = new XmlTextReader(sr);
					ApplicationSettingsSerializer.ReadFromXml(xmlIn, newObj);
					xmlIn.Close();

					list.Add(newObj);
				}

				return list;
			}

			return base.ConvertFrom(context, culture, value);
		}

		public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
		{
			if (destinationType == typeof(string))
			{
				IEnumerable<T> enumerable = value as IEnumerable<T>;

				if (enumerable == null)
					throw new ArgumentException();

				StringBuilder sb = new StringBuilder();

				foreach (T obj in enumerable)
				{
					StringWriter sw = new StringWriter();
					XmlTextWriter xmlOut = new XmlTextWriter(sw);
					ApplicationSettingsSerializer.WriteToXml(xmlOut, obj);
					sb.Append("<HEAD/>");
					sb.Append(sw.ToString());
					xmlOut.Close();
				}

				return sb.ToString();
			}

			return base.ConvertTo(context, culture, value, destinationType);
		}
	}
}
