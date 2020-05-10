// MachiThreadParser.cs

namespace Twin.Bbs
{
	using System;
	using System.IO;
	using System.Text;
	using System.Text.RegularExpressions;
	using System.Collections.Generic;
	using CSharpSamples.Text.Search;
	using Twin.Text;

	/// <summary>
	/// まちBBSのhtml形式を処理する
	/// </summary>
	public class MachiThreadParser : ThreadParser
	{
		/// <summary>
		/// タイトルを検索するための正規表現
		/// </summary>
		protected Regex SubjRegex =
			new Regex(@"<title>(?<subj>.*)</title>", RegexOptions.Compiled | RegexOptions.IgnoreCase);

		/// <summary>
		/// レス本文を検索するための正規表現
		/// </summary>
		protected Regex BodyRegex =
			new Regex("<dt>(?<num>\\d+) 名前：((<font.*?><b>\\s?(?<name>.+?)\\s?</b></font>)|(<a href=\"mailto:(?<email>.+?)\"><b>\\s?(?<name>.+?)\\s?</B></a>))\\s*?投稿日：(?<date>.+?)<br><dd>\\s?(?<body>.+)", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);
		
		protected Regex IDHostRegex =
			new Regex(@"ID:(?<idhost>(?<id>[^\s]+).+?\[(?<host>.+?)])</font>", RegexOptions.Compiled);

		/// <summary>
		/// aタグを検索する正規表現
		/// </summary>
		protected Regex AHrefRegex =
			new Regex(@"</?a.*?>", RegexOptions.Compiled | RegexOptions.IgnoreCase);

		/// <summary>
		/// レス開始位置を検索するための検索クラスを初期化
		/// </summary>
		protected ISearchable headSrch = new BmSearch2("<dt>");

		/// <summary>
		/// レス終了位置を検索するための検索クラスを初期化
		/// </summary>
		protected ISearchable tailSrch = new BmSearch2("<br><br>\n");

		private string subject;
		private int totalCount;

		/// <summary>
		/// 開始インデックスを取得または設定
		/// </summary>
		public int StartIndex {
			set { totalCount = value; }
			get { return totalCount; }
		}

		/// <summary>
		/// 掲示板の型とエンコーディングを指定して、
		/// MachiThreadParserクラスのインスタンスを初期化
		/// </summary>
		public MachiThreadParser(BbsType bbs, Encoding enc) : base(bbs, enc)
		{
			// 
			// TODO: コンストラクタ ロジックをここに追加してください。
			//
			subject = String.Empty;
			totalCount = 1;
		}

		/// <summary>
		/// MachiThreadParserクラスのインスタンスを初期化
		/// </summary>
		public MachiThreadParser() : this(BbsType.Machi, Encoding.GetEncoding("Shift_Jis"))
		{
		}

		protected override ResSet[] ParseData(string dataText)
		{
			if (dataText == null)
				throw new ArgumentNullException("dataText");

			// 2011/06/11 Mizutama
			dataText = dataText.Replace('\0', '*');

			List<ResSet> list = new List<ResSet>(300);
			int begin = 0, index;
			
			// スレッド名を検索
			Match m = SubjRegex.Match(dataText);
			if (m.Success) subject = m.Groups["subj"].Value;

			// レス開始位置を検索
			while (begin < dataText.Length && 
				(index = headSrch.Search(dataText, begin)) != -1)
			{
				// レス開始位置からレス終了位置を検索
				begin = index;
				int end = tailSrch.Search(dataText, begin);

				if (end != -1)
				{
					string lineData = dataText.Substring(begin, end - begin);
					begin = (end + tailSrch.Pattern.Length);

					ResSet res = ParseResSet(lineData);

					if (res.Index == 1)
						res.Tag = subject;

					// 解析した数と実際のレス番号が違えばあぼーんされていると思われるので、
					// その穴埋めとしてあぼーんレスを挿入
					if (totalCount != res.Index)
					{
						int length = res.Index - totalCount;
						totalCount += length;

						for (int i = 0; i < length; i++)
							list.Add(ResSet.ABoneResSet);
					} 

					totalCount++;
					list.Add(res);
				}
				else break;
			}

			return list.ToArray();
		}

		protected virtual ResSet ParseResSet(string data)
		{
			ResSet resSet = new ResSet(-1, "[ここ壊れてます]",
				String.Empty, "[ここ壊れてます]", "[ここ壊れてます]");

			Match m = BodyRegex.Match(data);
			if (m.Success)
			{
				int index;
				Int32.TryParse(m.Groups["num"].Value, out index);

				resSet.Index = index;
				resSet.DateString = m.Groups["date"].Value.Trim();
				resSet.Name = m.Groups["name"].Value;
				resSet.Body = m.Groups["body"].Value;
				resSet.Email = m.Groups["email"].Value;

				// 日付の改行を削除
				resSet.DateString = resSet.DateString.Replace("\n", "");

				// 本文のリンクを削除し、レス参照にリンクを張る
				resSet.Body = AHrefRegex.Replace(resSet.Body, String.Empty);
				//resSet.Body = HtmlTextUtility.RefRegex.Replace(resSet.Body, "<a href=\"/${num}\" target=\"_blank\">${ref}</a>");
			}

			return resSet;
		}

		protected override int GetEndToken(byte[] data, int index, int length, out int tokenLength)
		{
			if (data == null)
				throw new ArgumentNullException("data");

			if (index < 0)
				throw new ArgumentOutOfRangeException("index");

			const string key = "<br><br>\n";

			for (int i = length - key.Length; i >= 0; i--)
			{
				int pos;

				for (pos = 0; pos < key.Length; pos++)
					if (data[i+pos] != key[pos]) break;

				if (pos == key.Length)
				{
					tokenLength = key.Length;
					return i;
				}
			}

			tokenLength = 0;
			return -1;
		}
	}
}
