// X2chThreadParser.cs

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
	/// 2chのDat形式を処理する
	/// </summary>
	public class X2chThreadParser : ThreadParser
	{
		//		/// <summary>
		//		/// 日付を検索するための正規表現
		//		/// </summary>
		//		protected static readonly Regex DateRegex =
		//			new Regex(@"(?<date>[\d\s/:\(\)\p{IsCJKUnifiedIdeographs}]+)", RegexOptions.Compiled);

		/// <summary>
		/// 改行を検索するための検索クラスを初期化
		/// </summary>
		protected readonly ISearchable searcher = new KmpSearch("\n");
		/// <summary>
		/// <>を検索するための検索クラスを初期化
		/// </summary>
		protected readonly ISearchable s_token = new KmpSearch("<>");

		protected string[] elements;

		protected int resCount = 0;


		//		/// <summary>
		//		/// <>を検索するための検索クラスを初期化
		//		/// </summary>
		//		protected static readonly Regex splitRegex = new Regex("<>", RegexOptions.Compiled);

		/// <summary>
		/// 掲示板の型とエンコーディングを指定して、
		/// X2chThreadParserクラスのインスタンスを初期化
		/// </summary>
		/// <param name="enc"></param>
		public X2chThreadParser(BbsType bbs, Encoding enc)
			: base(bbs, enc)
		{
			// 
			// TODO: コンストラクタ ロジックをここに追加してください。
			//
			elements = new string[5];
			resCount = 0;
		}

		/// <summary>
		/// X2chThreadParserクラスのインスタンスを初期化
		/// </summary>
		public X2chThreadParser()
			: this(BbsType.X2ch, Encoding.GetEncoding("Shift_Jis"))
		{
			// 
			// TODO: コンストラクタ ロジックをここに追加してください。
			//
		}

		protected override ResSet[] ParseData(string dataText)
		{
			if (dataText == null)
				throw new ArgumentNullException("dataText");

			//string[] lines = Regex.Split(dataText, "\r\n|\r|\n");
			List<ResSet> list = new List<ResSet>(100);
			int begin = 0, index;

			// レス内に'\0'文字を含む場合があるので、それを*に置き換える
			if (dataText.IndexOf('\0') >= 0)
				dataText = dataText.Replace('\0', '*');

			// 旧式レスフォーマットの場合の特殊処理
			if (dataText.IndexOf("<>") < 0)
				dataText = dataText.Replace(",", "<>");

			while ((index = searcher.Search(dataText, begin)) != -1)
			{
				string lineData = dataText.Substring(begin, index - begin);

				if (list.Count >= 200)
				{
					string s = "hoge";
					if (s == "hoge")
					{
					}
				}

				// 次の検索開始位置を設定
				begin = index + searcher.Pattern.Length;

				// 正規表現使わない方が速い
				//string[] elements = splitRegex.Split(lineData);
				
				for (int i = 0; i < elements.Length; i++)
					elements[i] = String.Empty;

				int ret, beg = 0, idx = 0;
				while ((ret = s_token.Search(lineData, beg)) != -1 && idx < elements.Length)
				{
					elements[idx++] = lineData.Substring(beg, ret - beg);
					beg = ret + 2; // 2 == "<>".Length
				}
				if (beg < lineData.Length && idx < elements.Length)
					elements[idx] = lineData.Substring(beg, lineData.Length - beg);
				// -----------------------

				resCount++;

				list.Add(ParseResSet(elements));
			}

			return list.ToArray();
		}

		protected virtual ResSet ParseResSet(string[] elements)
		{
			ResSet resSet = new ResSet(-1, "[ここ壊れてます]",
				String.Empty, "[ここ壊れてます]", "[ここ壊れてます]");

			try
			{
				// name=0、email=1、date=2、message=3、subject=4
				resSet.Name = elements[0];
				resSet.Email = elements[1];
				resSet.DateString = elements[2];
				resSet.Body = elements[3];//HtmlTextUtility.RefRegex.Replace(elements[3], "<a href=\"../${num}\" target=\"_blank\">${ref}</a>");

				if (elements.Length >= 5 &&
					elements[4] != String.Empty)
				{
					resSet.Tag = elements[4];
				}
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.Write(ex);
			}

			return resSet;
		}

		protected override int GetEndToken(byte[] data, int index, int length, out int tokenLength)
		{
			if (data == null)
				throw new ArgumentNullException("data");

			if (index < 0)
				throw new ArgumentOutOfRangeException("index");

			for (int i = length - 1; i >= 0; i--)
			{
				if (data[i] == '\n')
				{
					tokenLength = 1;
					return i;
				}
			}

			tokenLength = 0;
			return -1;
		}
	}
}
