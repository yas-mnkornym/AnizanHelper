// NGWordCollection.cs

namespace Twin
{
	using System;
	using System.Collections.Generic;
	using System.Collections.Specialized;
	using Twin.Text;
	using CSharpSamples.Text.Search;
	using System.Text.RegularExpressions;
	using System.Text;

	/// <summary>
	/// NGワード文字列を検索用にコレクション管理
	/// </summary>
	public class NGWordCollection
	{
		private List<ISearchable> iSearchers;

		/// <summary>
		/// NGWordCollectionクラスのインスタンスを初期化
		/// </summary>
		public NGWordCollection()
		{
			iSearchers = new List<ISearchable>();
		}

		public bool IsMatch(string word)
		{
			string r;
			return IsMatch(word, out r);
		}

		public bool IsMatch(string word, out string matchWord)
		{
			return Search(word, out matchWord) >= 0;
		}

		/// <summary>
		/// wordがNGワードに一致するかどうかを判断
		/// </summary>
		/// <param name="word">検索する単語</param>
		/// <returns></returns>
		public int Search(string word, out string matchWord)
		{
			matchWord = "";
			foreach (ISearchable s in iSearchers)
			{
				int index = s.Search(word);
				if (index >= 0)
				{
					matchWord = s.Pattern;
					return index;
				}
			}
			return -1;
		}

		/// <summary>
		/// NGワードコレクションに文字列を追加
		/// </summary>
		/// <param name="str"></param>
		/// <returns></returns>
		public void Add(string str)
		{
			if (str == null)
				throw new ArgumentNullException("str");
			if (str == String.Empty)
				return;

			foreach (ISearchable a in iSearchers)
				if (a.Pattern == str)
					return;

			ISearchable s;

			if (str.StartsWith("$"))
			{
				s = new RegexSearch(ParseRegexPattern(str), ParseRegexOptions(str));
			}
			else
			{
				s = new BmSearch2(str);
			}

			iSearchers.Add(s);
		}

		/// <summary>
		/// 複数のNGワードを追加
		/// </summary>
		/// <param name="array"></param>
		public void AddRange(string[] array)
		{
			foreach (string str in array)
				Add(str);
		}

		/// <summary>
		/// 複数のNGワードを追加
		/// </summary>
		/// <param name="strings"></param>
		public void AddRange(StringCollection strings)
		{
			if (strings == null)
				return;

			foreach (string str in strings)
				Add(str);
		}

		/// <summary>
		/// nGWordsを追加
		/// </summary>
		/// <param name="nGWords"></param>
		public void AddRange(NGWordCollection nGWords)
		{
			iSearchers.AddRange(nGWords.iSearchers);
		}

		/// <summary>
		/// 一端すべての要素を削除しarrayを追加
		/// </summary>
		/// <param name="array"></param>
		public void SetRange(string[] array)
		{
			Clear();
			AddRange(array);
		}

		/// <summary>
		/// すべての検索パターンを取得
		/// </summary>
		/// <returns></returns>
		public string[] GetPatterns()
		{
			List<string> list = new List<string>();

			foreach (ISearchable s in iSearchers)
			{
				if (s is RegexSearch)
				{
					list.Add(GetRegexPattern((RegexSearch)s));
				}
				else
				{
					list.Add(s.Pattern);
				}
			}

			return list.ToArray();
		}

		/// <summary>
		/// コレクションをクリア
		/// </summary>
		public void Clear()
		{
			iSearchers.Clear();
		}

		private string GetRegexPattern(RegexSearch s)
		{
			StringBuilder sb = new StringBuilder();

			sb.Append("$").Append(s.Pattern);

			// オプションが設定されている場合
			if (s.Regex.Options != RegexOptions.None)
			{
				sb.Append('\t');
				sb.Append(s.Regex.Options.ToString());
			}
			return sb.ToString();
		}

		private string ParseRegexPattern(string pattern)
		{
			if (!pattern.StartsWith("$"))
				throw new ArgumentException();

			int tabIndex = pattern.LastIndexOf('\t');

			if (tabIndex == -1)
				tabIndex = pattern.Length;

			return pattern.Substring(1, tabIndex-1);
		}

		private RegexOptions ParseRegexOptions(string pattern)
		{
			int tabIndex = pattern.LastIndexOf('\t');

			if (tabIndex == -1)
				return RegexOptions.None;

			else
			{
				string[] flags = pattern.Substring(tabIndex + 1).Split(',');
				RegexOptions options = RegexOptions.None;

				foreach (string value in flags)
				{
					if (Enum.IsDefined(typeof(RegexOptions), value.Trim()))
					{
						options |= (RegexOptions)Enum.Parse(typeof(RegexOptions), value.Trim());
					}
				}

				return options;
			}
		}
	}
}
