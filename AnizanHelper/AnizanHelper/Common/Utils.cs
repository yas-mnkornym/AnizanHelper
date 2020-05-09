using System.Text;

namespace AnizanHelper
{
	internal static class Utils
	{
		#region GeneralToString()
		public static string GeneralObjectToString(object dstObj)
		{
			var type = dstObj.GetType();
			var properties = type.GetProperties();
			StringBuilder sb = new StringBuilder();
			sb.Append(type.Name);
			sb.Append("{");
			bool isFirst = true;
			foreach (var prop in properties)
			{
				if (isFirst) { isFirst = false; }
				else { sb.Append(", "); }
				sb.Append(prop.Name);
				sb.Append(": ");
				PrintProperty(prop, dstObj, sb);
			}
			sb.Append("}");
			return sb.ToString();
		}

		private static void PrintProperty(System.Reflection.PropertyInfo prop, object dstObj, StringBuilder sb)
		{
			if (prop.PropertyType.IsArray)
			{
				var array = (System.Collections.IEnumerable)prop.GetValue(dstObj, null);
				PrintArray(array, sb);
			}
			else
			{
				sb.Append(prop.GetValue(dstObj, null));
			}
		}

		private static void PrintArray(System.Collections.IEnumerable array, StringBuilder sb)
		{
			bool isFirst = true;
			sb.Append("[");
			foreach (var el in array)
			{
				if (isFirst) { isFirst = false; }
				else { sb.Append(", "); }

				var elType = el.GetType();
				if (elType.IsArray)
				{
					PrintArray((System.Collections.IEnumerable)el, sb);
				}
				else
				{
					sb.Append(el);
				}
			}
			sb.Append("]");
		}
		#endregion
	}
}
