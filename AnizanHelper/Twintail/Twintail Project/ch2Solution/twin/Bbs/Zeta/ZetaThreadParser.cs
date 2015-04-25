// ZetaThreadListParser.cs

namespace Twin.Bbs
{
	using System;
	using System.Text;
	using System.Text.RegularExpressions;
	using System.Collections.Generic;
	using Twin.Text;

	/// <summary>
	/// Zetabbsのスレッドを解析 (2ch互換)
	/// </summary>
	public class ZetaThreadParser : X2chThreadParser
	{
		/// <summary>
		/// IDを検索するための正規表現
		/// </summary>
		private readonly Regex ZetaIDRegex =
			new Regex(@"(TC:(?<id>[^\s]+))", RegexOptions.Compiled);

//		/// <summary>
//		/// 日付を検索するための正規表現
//		/// </summary>
//		private readonly Regex ZetaDateRegex =
//			new Regex(@"(?<date>\d+/\d+/\d+ \(.+?\) \d+:\d+:\d+)", RegexOptions.Compiled);

		/// <summary>
		/// ZetaThreadParserクラスのインスタンスを初期化
		/// </summary>
		public ZetaThreadParser()
			: base(BbsType.Zeta, Encoding.GetEncoding("Shift_Jis"))
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
			List<ResSet> list = new List<ResSet>(300);
			int begin = 0, index;

			while ((index = searcher.Search(dataText, begin)) != -1)
			{
				string lineData = dataText.Substring(begin, index - begin);
				begin = index + searcher.Pattern.Length;
		
				ResSet resSet = new ResSet(-1, "[ここ壊れてます]",
					String.Empty, "[ここ壊れてます]", "[ここ壊れてます]");

				string[] elements = Regex.Split(lineData, "<>");

				if (elements.Length >= 4)
				{
					try {
						// IDを取得
						Match idmatch = ZetaIDRegex.Match(elements[2]);
						string id = idmatch.Success ? idmatch.Groups["id"].Value : "";

						// name=0、email=1、date=2、message=3、subject=4
						resSet.ID = id;
						resSet.Name = elements[0];
						resSet.Email = elements[1];
						resSet.DateString = elements[2];
						resSet.Body = elements[3];

						if (elements.Length >= 5 &&
							elements[4] != String.Empty)
						{
							resSet.Tag = elements[4];
						}
						list.Add(resSet);
					}
					catch (Exception ex) {
						System.Diagnostics.Debug.Write(ex);
					}
				}
			}

			return list.ToArray();
		}
	}
}
