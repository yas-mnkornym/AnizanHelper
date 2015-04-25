// X2chHtmlThreadParser.cs

namespace Twin.Bbs
{
	using System;
	using System.Text;
	using System.Text.RegularExpressions;
	using System.Collections.Generic;
	using Twin.Text;

	/// <summary>
	/// 2chの過去ログ化されたhtmlを解析するパーサ
	/// </summary>
	public class X2chHtmlThreadParser : ThreadParser
	{
		/// <summary>
		/// レスを検索するパターン
		/// </summary>
		private readonly Regex Pattern =
			new Regex("<dt>(?<index>\\d+) ：(<a href=\"mailto:(?<email>\\w+?)\">|(?<email>)).*<b>(?<name>\\w+?)</b></font> ：(?<dateid>.+?)<dd>(?<body>.+?)(<dt>|</dl>)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

		/// <summary>
		/// X2chHtmlThreadParserクラスのインスタンスを初期化
		/// </summary>
		public X2chHtmlThreadParser(BbsType bbs, Encoding enc) : base(bbs, enc)
		{
			// 
			// TODO: コンストラクタ ロジックをここに追加してください。
			//
		}

		protected override int GetEndToken(byte[] data, int index, int length, out int tokenLength)
		{
			const string token = "</dl>";

			for (int i = length - 1; i >= index; i--)
			{
				if (Compare(data, i, token))
				{
					tokenLength = token.Length;
					return i;
				}
			}

			tokenLength = 0;
			return -1;
		}

		protected override ResSet[] ParseData(string data)
		{
			MatchCollection matches = Pattern.Matches(data);
			List<ResSet> items = new List<ResSet>();

			foreach (Match m in matches)
			{
				int index;
				Int32.TryParse(m.Groups["index"].Value, out index);

				ResSet res = new ResSet(
					index,
					m.Groups["name"].Value,
					m.Groups["email"].Value,
					m.Groups["dateid"].Value,
					m.Groups["body"].Value);

				items.Add(res);
			};

			return items.ToArray();
		}

		private bool Compare(byte[] data, int index, string text)
		{
			for (int i = index; i < index + text.Length; i++)
			{
				if (Convert.ToChar(data[i]) != text[i])
					return false;
			}
			return true;
		}
	}
}
