// JbbsThreadParser.cs

namespace Twin.Bbs
{
	using System;
	using System.Text;
	using System.Text.RegularExpressions;

	/// <summary>
	/// JbbsThreadParser の概要の説明です。
	/// </summary>
	public class JbbsThreadParser : X2chThreadParser
	{
		/// <summary>
		/// JbbsThreadParserクラスのインスタンスを初期化
		/// </summary>
		public JbbsThreadParser()
			: base(BbsType.Jbbs, Encoding.GetEncoding("EUC-JP"))
		{
			base.elements = new string[7];

			// rawmode.cgi フォーマット
			// http://blog.livedoor.jp/bbsnews/archives/50283526.html
			// [レス番号]<>[名前]<>[メール]<>[日付]<>[本文]<>[スレッドタイトル]<>[ID]

			// BodyRegex =
			//	new Regex("(?<num>.+?)<>(?<name>.+?)<>(?<email>.+?)<>(?<date>.+?)<>(?<body>.+?)<>(?<threadname>.*?)<>(?<id>.+?)", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);


			// read.cgi フォーマット
			//BodyRegex =
			//    new Regex("<dt><a.*?>(?<num>\\d+)</a> 名前：((<font.*?><b>\\s?(?<name>.+?)\\s?</b></font>)|(<a href=\"mailto:(?<email>.+?)\"><b>\\s?(?<name>.+?)\\s?</B></a>))\\s*?投稿日：\\s*(?<date>.+?)<dd>(?<body>.+)", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);
		}

		protected override ResSet ParseResSet(string[] elements)
		{
			ResSet resSet = new ResSet(-1, "[ここ壊れてます]",
				String.Empty, "[ここ壊れてます]", "[ここ壊れてます]");

			// [レス番号]<>[名前]<>[メール]<>[日付]<>[本文]<>[スレッドタイトル]<>[ID]

			int index;
			Int32.TryParse(elements[0], out index);

			resSet.Index = index;
			resSet.Name = elements[1];
			resSet.Email = elements[2];
			resSet.DateString = String.Concat(elements[3], " ID:", elements[6]);
			resSet.Body = elements[4];
			resSet.Tag = elements[5];
			resSet.ID = elements[6];

			return resSet;
		}
	}
}
