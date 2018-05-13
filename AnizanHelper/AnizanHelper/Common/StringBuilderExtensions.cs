using System.Text;

namespace AnizanHelper
{
	public static class StringBuilderExtensions
	{
		public static void AppendLine(
			this StringBuilder stringBuilder,
			string format,
			params object[] args)
		{
			stringBuilder.AppendFormat(format, args);
			stringBuilder.AppendLine();
		}
	}
}
