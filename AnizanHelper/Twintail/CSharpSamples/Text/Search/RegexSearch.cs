using System.Text.RegularExpressions;

namespace CSharpSamples.Text.Search
{
	/// <summary>
	/// Regexクラスを使用して正規表現検索を行う
	/// </summary>
	public class RegexSearch : ISearchable
	{
		private readonly Regex regex;
		private readonly string pattern;

		/// <summary>
		/// 検索パターンを取得
		/// </summary>
		public string Pattern
		{
			get
			{
				return this.pattern;
			}
		}

		public Regex Regex
		{
			get
			{
				return this.regex;
			}
		}

		/// <summary>
		/// RegexSearchクラスのインスタンスを初期化
		/// </summary>
		/// <param name="key"></param>
		public RegexSearch(string key, RegexOptions options)
		{
			this.regex = new Regex(key, options);
			this.pattern = key;
		}

		/// <summary>
		/// 文字列照合を行う
		/// </summary>
		/// <param name="text">検索文字列</param>
		/// <returns></returns>
		public int Search(string text)
		{
			return this.Search(text, 0);
		}

		/// <summary>
		/// 指定したインデックスから文字列照合を行う
		/// </summary>
		/// <param name="text">検索文字列</param>
		/// <param name="index">検索開始インデックス</param>
		/// <returns></returns>
		public int Search(string text, int index)
		{
			Match m = this.regex.Match(text, index);
			return (m.Success ? m.Index : -1);
		}
	}
}
