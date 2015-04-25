// PathUtil.cs

namespace Twin
{
	using System;
	using System.Text;
	using System.Text.RegularExpressions;
	using System.IO;

	public class PathUtil
	{
		public static string ReplaceInvalidPathChars(string text, string replacement)
		{
			StringBuilder sb = new StringBuilder(text);

			foreach (char ch in Path.GetInvalidPathChars())
				sb.Replace(ch.ToString(), replacement);

			foreach (char ch in Path.GetInvalidFileNameChars())
				sb.Replace(ch.ToString(), replacement);

			sb.Replace(Path.DirectorySeparatorChar.ToString(), replacement);
			sb.Replace(Path.AltDirectorySeparatorChar.ToString(), replacement);
			sb.Replace(Path.VolumeSeparatorChar.ToString(), replacement);
			sb.Replace("?", replacement);

			return sb.ToString();
		}
	}
}
