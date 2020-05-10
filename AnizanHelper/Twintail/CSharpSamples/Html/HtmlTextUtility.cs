// HtmlTextUtility.cs

namespace CSharpSamples.Html
{
	using System;
	using System.Drawing;
	using System.Text;

	/// <summary>
	/// HtmlUtility の概要の説明です。
	/// </summary>
	public class HtmlTextUtility
	{
		//private static string[] esc_replacement = null;
		private static string[] unesc_replacement = null;

		/// <summary>
		/// 特殊文字をエスケープ
		/// </summary>
		/// <param name="html"></param>
		/// <returns></returns>
		public static string Escape(string html)
		{
			throw new NotSupportedException("未実装");
			//StringBuilder sb = new StringBuilder(html);
			//return sb.ToString();
		}

		/// <summary>
		/// スケープされた文字を元に変換
		/// </summary>
		/// <param name="html"></param>
		/// <returns></returns>
		public static string UnEscape(string html)
		{
			if (unesc_replacement == null)
			{
				unesc_replacement = new string[]
				{
					"&copy;", new string((char)169, 1),
					"&reg;", new string((char)174, 1),
					"&yen;", "\\",
					"&nbsp;", " ",
					"&lt;", "<",
					"&gt;", ">",
				};
			}

			StringBuilder sb = new StringBuilder(html);

			for (int i = 0; i < unesc_replacement.Length; i += 2)
			{
				sb.Replace(unesc_replacement[i], unesc_replacement[i + 1]);
			}

			return sb.ToString();
		}

		/// <summary>
		/// Html形式の色情報をColor構造体に変換
		/// </summary>
		/// <param name="html"></param>
		/// <returns></returns>
		public static Color ColorFromHtml(string html)
		{
			if (html == null)
			{
				throw new ArgumentNullException("html");
			}

			return html.StartsWith("#") ?
				ColorTranslator.FromHtml(html) : Color.FromName(html);
		}
	}
}
