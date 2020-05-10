using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Collections;
using System.Collections.Specialized;

namespace Twin
{
	public class IntArrayConverter : TypeConverter
	{
		public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
		{
			if (sourceType == typeof(string))
				return true;

			return base.CanConvertFrom(context, sourceType);
		}

		// 文字列から変換
		public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
		{
			if (value is string)
			{
				string[] array = value.ToString().Split(',');
				List<int> list = new List<int>();

				int result;

				foreach (string val in array)
				{
					if (Int32.TryParse(val, out result))
						list.Add(result);
				}

				return list.ToArray();
			}

			return base.ConvertFrom(context, culture, value);
		}

		// 文字列に変換
		public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
		{
			if (destinationType == typeof(string))
			{
				StringBuilder sb = new StringBuilder();

				foreach (int val in (int[])value)
				{
					sb.Append(val).Append(',');
				}

				// 余分な最後のコロンを取り除く
				if (sb.Length > 0)
					sb.Remove(sb.Length - 1, 1);

				return sb.ToString();
			}

			return base.ConvertTo(context, culture, value, destinationType);
		}
	}

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
				string r = Convert.ToBase64String((byte[])value);
				return r;
			}

			return base.ConvertTo(context, culture, value, destinationType);
		}
	}

	public class BoardInfoCollectionConverter : TypeConverter
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
				string[] array = value.ToString().Split(',');
				BoardInfoCollection collection = new BoardInfoCollection();

				foreach (string item in array)
				{
					string[] info = item.Split('|');

					if (info.Length == 3)
					{
						BoardInfo bi = new BoardInfo(info[1], info[2], info[0]);
						collection.Add(bi);
					}
				}

				return collection;
			}

			return base.ConvertFrom(context, culture, value);
		}

		public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
		{
			if (destinationType == typeof(string))
			{
				BoardInfoCollection src = (BoardInfoCollection)value;

				StringBuilder sb = new StringBuilder();

				foreach (BoardInfo bi in src)
				{
					sb.AppendFormat("{0}|{1}|{2},",
						bi.Name, bi.Server, bi.Path);
				}

				if (sb.Length > 0)
					sb.Remove(sb.Length - 1, 1);

				return sb.ToString();
			}

			return base.ConvertTo(context, culture, value, destinationType);
		}
	}

	public class StringCollectionConverter : TypeConverter
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
				ArrayListConverter<string> conv = new ArrayListConverter<string>();

				return conv.ConvertToString(value);
			}

			return base.ConvertTo(context, culture, value, destinationType);
		}
	}

	public class ArrayListConverter<T> : TypeConverter
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
				string[] array = Regex.Split(value.ToString(), @"(?<!(?<!\\)\\),"); // NTwin23.103 

				ArrayList al = new ArrayList();
				TypeConverter converter = TypeDescriptor.GetConverter(typeof(T));

				foreach (string s in array)
				{
					if (typeof(T) == typeof(string))
					{
						// NTwin23.103 
						string sr = s.Replace(@"\,", ",");
						sr = sr.Replace(@"\\", @"\");
						al.Add(sr);
						// NTwin23.103
					}
					else
						al.Add(converter.ConvertFromString(s));
				}

				return al;
			}

			return base.ConvertFrom(context, culture, value);
		}

		public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
		{
			if (destinationType == typeof(string))
			{
				IEnumerable enumerable = value as IEnumerable;

				if (enumerable == null)
					throw new ArgumentException();

				TypeConverter converter = TypeDescriptor.GetConverter(destinationType);
				StringBuilder sb = new StringBuilder();

				foreach (object obj in enumerable)
				{
					string s = converter.ConvertToString(obj);

					s = Escape(s);

					sb.Append(s).Append(',');
				}

				if (sb.Length > 0)
					sb.Remove(sb.Length - 1, 1);

				return sb.ToString();
			}

			return base.ConvertTo(context, culture, value, destinationType);
		}

		private string Escape(string s)
		{
			// NTwin23.103 
			s = s.Replace(@"\", @"\\"); // まず「\」をエスケープしておいて 
			s = s.Replace(",", @"\,");  // 「,」（コンマ）をエスケープ 

			return s;
		}
	}

	public class TwinExpandableConverter : TypeConverter
	{
		public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
		{
			return base.CanConvertFrom(context, sourceType);
		}

		public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
		{
			return base.ConvertFrom(context, culture, value);
		}

		public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
		{
			return base.ConvertTo(context, culture, value, destinationType);
		}
	}
}
