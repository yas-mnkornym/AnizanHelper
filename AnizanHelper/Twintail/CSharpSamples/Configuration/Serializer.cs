// Serializer.cs

namespace CSharpSamples
{
	using System;
	using System.Reflection;
	using System.Runtime.Serialization;

	/// <summary>
	/// Serializer
	/// </summary>
	public class Serializer
	{
		/// <summary>
		/// objのフィールドをシリアライズ
		/// </summary>
		/// <param name="obj"></param>
		/// <param name="info"></param>
		/// <param name="flags"></param>
		public static void Serialize(object obj, SerializationInfo info, BindingFlags flags)
		{
			FieldInfo[] fields = obj.GetType().GetFields(flags);
			foreach (FieldInfo field in fields)
			{
				object val = field.GetValue(obj);
				info.AddValue(field.Name, val);
			}
		}

		/// <summary>
		/// objのフィールドをシリアライズ
		/// デフォルトではパブリック＆インスタンスなメンバのみ検索。
		/// </summary>
		/// <param name="obj"></param>
		/// <param name="info"></param>
		public static void Serialize(object obj, SerializationInfo info)
		{
			Serialize(obj, info, BindingFlags.Public | BindingFlags.Instance);
		}

		/// <summary>
		/// infoを使用して逆シリアライズしobjに値を設定
		/// </summary>
		/// <param name="obj"></param>
		/// <param name="info"></param>
		/// <param name="flags"></param>
		public static void Deserialize(object obj, SerializationInfo info, BindingFlags flags)
		{
			FieldInfo[] fields = obj.GetType().GetFields(flags);
			foreach (FieldInfo field in fields)
			{
				try {
					object data = info.GetValue(field.Name, field.FieldType);
					field.SetValue(obj, data);
				}
				catch (SerializationException) {}
			}
		}

		/// <summary>
		/// infoを使用して逆シリアライズしobjに値を設定。
		/// デフォルトではパブリック＆インスタンスなメンバのみ検索。
		/// </summary>
		/// <param name="obj"></param>
		/// <param name="info"></param>
		public static void Deserialize(object obj, SerializationInfo info)
		{
			Deserialize(obj, info, BindingFlags.Public | BindingFlags.Instance);
		}
	}
}
