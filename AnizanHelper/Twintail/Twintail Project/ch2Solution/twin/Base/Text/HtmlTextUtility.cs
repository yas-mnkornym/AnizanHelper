// HtmlTextUtility.cs

namespace Twin.Text
{
	using System;
	using System.Text;
	using System.Text.RegularExpressions;
	using System.Drawing;
	using System.Windows.Forms;

	// *****
	//  2012/1/15
	//  URLを改行せずに連続して書かれていた場合にもうまくリンクするように修正
	//  水玉さんｻﾝｸｽです！
	// ******

	/// <summary>
	/// Htmlや文字列操作のユーティリティ群
	/// </summary>
	public class HtmlTextUtility
	{
		/// <summary>
		/// タグすべてを検索する正規表現
		/// </summary>
		public static Regex SearchTagRegex = 
			new Regex(@"</?[^>]*/?>", RegexOptions.Compiled);

		/// <summary>
		/// 数字かどうかを判断するための正規表現
		/// </summary>
		public static readonly Regex IsDigitRegex =
			new Regex(@"^\d+$", RegexOptions.Compiled);

//		/// <summary>
//		/// <br>タグで分割するための正規表現
//		/// </summary>
//		public static readonly Regex SplitRegex =
//			new Regex(@"<br>", RegexOptions.IgnoreCase | RegexOptions.Compiled);

		/// <summary>
		/// レス参照(例: >>1-5 形式)を検索するための正規表現
		/// </summary>
		public static readonly Regex RefRegex =
			new Regex(@"(?<ref>&gt;&gt;(?<num>[1-9]+[\d\-\+,]*))", RegexOptions.Compiled);　// 2011.12.16 水玉さん

//			new Regex(@"(?<ref>&gt;&gt;(?<num>\d+\-?\d*))", RegexOptions.Compiled);

		/// <summary>
		/// レス参照(例: >>10-15,20,30,40-50形式) を検索するための正規表現。ただし">>10-15"のレス番は取れない。
		/// </summary>
		public static readonly Regex ExRefRegex =
			new Regex(@"(?<=&gt;&gt;[\d\-\,]+?(,|\+))(?<num>\d+\-?\d*)", RegexOptions.Compiled);
		//			new Regex(@"(?<=&gt;&gt;[^\s]+?(,|\+))(?<num>\d+\-?\d*)", RegexOptions.Compiled);

		/// <summary>
		/// ttp://のURLを検索する正規表現
		/// </summary>
		public static readonly Regex ttpToRegex =
// 1/15		new Regex(@"(?<!h)(?<link>(ttp://[\w\.]+?/wiki/[^\s\<]+)|(ttps?://[a-zA-Z\d/_@#%&+*:;=~',.!()|?[\]\-]+))", 
			new Regex(@"((?<!h)(?<link>(ttp://[\w\.]+?/wiki/[^\s\<]+)|(ttps?://[a-zA-Z\d/_@#%&+*:;=~',.!()|?[\]\-]+))(?=h?ttp://))|((?<!h)(?<link>(ttp://[\w\.]+?/wiki/[^\s\<]+)|(ttps?://[a-zA-Z\d/_@#%&+*:;=~',.!()|?[\]\-]+)))",
				RegexOptions.Compiled);

		/// <summary>
		/// 基本的なURLを検索する正規表現
		/// </summary>
		public static readonly Regex LinkRegex =
			new Regex(@"(?<link>((http://[\w\.]+?/wiki/[^\s\<]+)|((https?|ftps?|mms|rts?p)://[a-zA-Z\d/_@#%&+*:;=~',.!()|?[\]\-]+)))", 
				RegexOptions.Compiled);

		/// <summary>
		/// 基本的なURLを検索する正規表現 (h抜きも検索)
		/// </summary>
		public static readonly Regex LinkRegex2 =
// 1/15		new Regex(@"(?<link>(h?ttps?|ftps?|mms|rts?p)://(?<url>[a-zA-Z\d/_@#%&+*:;=~',.!()|?[\]\-]+))", 
			new Regex(@"((?<link>(h?ttps?|ftps?|mms|rts?p)://(?<url>[a-zA-Z\d/_@#%&+*:;=~',.!|?[\]\-]+))(?=h?ttp://))|((?<link>(h?ttps?|ftps?|mms|rts?p)://(?<url>[a-zA-Z\d/_@#%&+*:;=~',.!|?[\]\-]+)))", 
				RegexOptions.Compiled);

		/// <summary>
		/// 省略形のHTTPなURLかどうかを判断
		/// </summary>
		public static readonly Regex IsShortHttpUrl =
			new Regex(@"^((ttp|tp)://)|(www.[^/]+\.)", RegexOptions.Compiled);

		// IDの正規表現
		public static readonly Regex IDRegex = new Regex(@"[a-zA-Z0-9+/]{8,9}");

		/// <summary>
		/// 指定したhtml内のURLをAタグで挟みリンクする
		/// </summary>
		/// <param name="html"></param>
		/// <returns></returns>
		public static string Linking(string html)
		{
			if (html == null) {
				throw new ArgumentNullException("html");
			}
			string r = html;
			r = LinkRegex.Replace(r, "<a href=\"${link}\" target=\"_blank\">${link}</a>");
			r = ttpToRegex.Replace(r, "<a href=\"h${link}\" target=\"_blank\">${link}</a>");

			return r;
		}

		/// <summary>
		/// 半角数字を全角数字に変換
		/// </summary>
		/// <param name="text"></param>
		/// <returns></returns>
		public static string HanToZen(string text)
		{
			if (text == null)
			{
				throw new ArgumentNullException("text");
			}

			StringBuilder buffer = new StringBuilder(text);
			char[] zenChars = { '０', '１', '２', '３', '４', '５', '６', '７', '８', '９' };
			char[] hanChars = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };

			for (int i = 0; i < 10; i++)
				buffer.Replace(hanChars[i], zenChars[i]);

			return buffer.ToString();
		}

		/// <summary>
		/// 全角数字を半角数字に変換
		/// </summary>
		/// <param name="text"></param>
		/// <returns></returns>
		public static string ZenToHan(string text)
		{
			if (text == null) {
				throw new ArgumentNullException("text");
			}

			StringBuilder buffer = new StringBuilder(text);
			char[] zenChars = {'０','１','２','３','４','５','６','７','８','９'};
			char[] hanChars = {'0','1','2','3','4','5','6','7','8','9'};

			for (int i = 0; i < 10; i++)
				buffer.Replace(zenChars[i], hanChars[i]);

			return buffer.ToString();
		}

		/// <summary>
		/// 指定した文字列が数字かどうかを判断
		/// </summary>
		/// <param name="text"></param>
		/// <returns></returns>
		public static bool IsDigit(string text)
		{
			if (text == null)
				return false;
			return IsDigitRegex.IsMatch(text);
		}

		/// <summary>
		/// タグを取り除く
		/// </summary>
		/// <param name="html"></param>
		/// <returns></returns>
		public static string RemoveTag(string html)
		{
			if (html == null)
				return String.Empty;
			return SearchTagRegex.Replace(html, "");
		}

		/// <summary>
		/// 指定したタグを取り除きます。
		/// </summary>
		/// <param name="html"></param>
		/// <param name="tagName">除去するタグの名前。OR演算子により複数指定できます。大文字小文字は区別しません。</param>
		/// <returns></returns>
		public static string RemoveTag(string html, string tagName)
		{
			return Regex.Replace(html, "</?(" + tagName + ")[^>]*>", String.Empty, RegexOptions.IgnoreCase);
		}

		/// <summary>
		/// HTML文字列をテキストに変換
		/// </summary>
		/// <param name="html"></param>
		/// <returns></returns>
		public static string HtmlToText(string html)
		{
			html = Regex.Replace(html, "<br>", Environment.NewLine, RegexOptions.IgnoreCase);
			html = Regex.Replace(html, "<[^>]+>", "");
			html = Regex.Replace(html, "&gt;", ">", RegexOptions.IgnoreCase);
			html = Regex.Replace(html, "&lt;", "<", RegexOptions.IgnoreCase);

			return html;
		}

		/// <summary>
		/// textの両端に含まれる空白を削除
		/// </summary>
		/// <param name="text"></param>
		/// <returns></returns>
		public static string RemoveSpace(string text)
		{
			if (text == null)
				throw new ArgumentNullException("text");

			return Regex.Replace(text, @"\s*([^\s]+)\s*", "${1}");
		}

		/// <summary>
		/// タグおよびスペースを除去
		/// </summary>
		/// <param name="html"></param>
		/// <returns></returns>
		public static string TrimTag(string html)
		{
			return Regex.Replace(html, @"\s*|</?[^>]*/?>", String.Empty);
		}

		internal static string HanToZen(int dec)
		{
			throw new NotImplementedException();
		}
	}
}
