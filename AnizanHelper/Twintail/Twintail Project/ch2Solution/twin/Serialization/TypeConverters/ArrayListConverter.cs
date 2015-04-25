using System;
using System.Collections;
using System.ComponentModel;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Twintail3
{
	/// <summary>
	/// ArrayList クラスに格納されているプリミティブ型を変換するクラスです。
	/// </summary>
	/// <typeparam name="T"></typeparam>
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
				string[] array = Regex.Split(value.ToString(), @"(?<!(?<!\\)\\),");　// NTwin23.103

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
				if (value == null)
					return String.Empty;

				IEnumerable enumerable = value as IEnumerable;

				if (enumerable == null)
					throw new ArgumentException();

				TypeConverter converter = TypeDescriptor.GetConverter(typeof(T));
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
	　　　　s = s.Replace( @"\" , @"\\" );　　// まず「\」をエスケープしておいて
	　　　　s = s.Replace( "," , @"\," );　　　// 「,」（コンマ）をエスケープ

	　　　　//// コロンをエスケープしていく
	　　　　//int startIndex = 0 , token = -1;

	　　　　//while ( (token = s.IndexOf( ',' , startIndex )) >= 0 )
	　　　　//{
	　　　　//　　s = s.Insert( token , "\\" );
	　　　　//　　startIndex = token + 2;
	　　　　//}
	　　　　//// NTwin23.103

	　　　　return s;
		}
	}
}
