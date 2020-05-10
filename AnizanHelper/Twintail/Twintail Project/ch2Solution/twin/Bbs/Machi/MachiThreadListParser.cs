// MachiThreadListParser.cs

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
	/// まちBBSのsubject.txtを処理する
	/// </summary>
	public class MachiThreadListParser : ThreadListParser
	{
		// スレッド一覧を解析する正規表現
//		protected static readonly Regex Pattern =
//			new Regex(@"^(?<key>.+?)\.cgi,(?<subj>.+?)\((?<res>\d+)\)$",
//			RegexOptions.Multiline | RegexOptions.Compiled);

		private ISearchable newline = new KmpSearch("\n");

		/// <summary>
		/// 掲示板の型とエンコーディングを指定して、
		/// MachiThreadListParserクラスのインスタンスを初期化
		/// </summary>
		public MachiThreadListParser(BbsType bbs, Encoding enc) : base(bbs, enc)
		{
		}

		/// <summary>
		/// MachiThreadListParserクラスのインスタンスを初期化
		/// </summary>
		public MachiThreadListParser() : base(BbsType.Machi, Encoding.GetEncoding("Shift_Jis"))
		{
		}

		protected override ThreadHeader[] ParseData(string dataText)
		{
			List<ThreadHeader> list = new List<ThreadHeader>(100);
			const string token = ".cgi,";
			int begin = 0, index;

			try {
				// 123456.cgi,スレッド名(123)
				while (begin < dataText.Length &&
					(index = newline.Search(dataText, begin)) != -1)
				{
					string data = dataText.Substring(begin, index - begin);
					int idxDot = data.IndexOf(token);
					int idxSep1 = data.LastIndexOf('(');
					int idxSep2 = data.LastIndexOf(')');
					int idxSubj = idxDot + token.Length;

					if (idxDot >= 0 && idxSep1 >= 0 && idxSep2 >= 0)
					{
						ThreadHeader header = TypeCreator.CreateThreadHeader(bbsType);
						header.Key = data.Substring(0, idxDot);
						header.Subject = data.Substring(idxSubj, idxSep1 - idxSubj);
						
						int resCnt;
						if (Int32.TryParse(data.Substring(idxSep1 + 1, idxSep2 - (idxSep1 + 1)), out resCnt))
							header.ResCount = resCnt;

						list.Add(header);
					}
					begin = (index + newline.Pattern.Length);
				}
			}
			catch (Exception ex) {
				TwinDll.Output(ex);
			}

			return list.ToArray();
		}

		protected override int GetEndToken(byte[] data, int index, int length, out int tokenLength)
		{
			if (data == null)
				throw new ArgumentNullException("data");

			if (index < 0)
				throw new ArgumentOutOfRangeException("index");

			for (int i = length-1; i >= 0; i--)
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
