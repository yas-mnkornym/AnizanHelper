// PostResponseParser.cs

namespace Twin
{
	using System;
	using System.Text.RegularExpressions;
	using System.Diagnostics;
using System.Collections.Generic;

	/// <summary>
	/// サーバーからのレスポンスメッセージを解析する機能を提供
	/// </summary>
	public class PostResponseParser
	{
		protected string title;
		protected string plainText;
		protected string htmlText;
		protected int sambaCount;
		protected PostResponse response;

		/// <summary>
		/// タイトルを取得
		/// </summary>
		public string Title
		{
			get
			{
				return title;
			}
		}

		/// <summary>
		/// Html形式の本分を取得
		/// </summary>
		public string HtmlText
		{
			get
			{
				return htmlText;
			}
		}

		/// <summary>
		/// テキスト形式の本分を取得
		/// </summary>
		public string PlainText
		{
			get
			{
				return plainText;
			}
		}

		/// <summary>
		/// サーバーのSamba秒数を取得
		/// </summary>
		public int SambaCount
		{
			get
			{
				return sambaCount;
			}
		}

		/// <summary>
		/// サーバーからの応対状態を取得
		/// </summary>
		public PostResponse Response
		{
			get
			{
				return response;
			}
		}

		private Dictionary<string,string> hiddenParamsDic = new Dictionary<string,string>();
		public Dictionary<string,string> HiddenParams
		{
			get
			{
				return hiddenParamsDic;
			}
		}
	

		/// <summary>
		/// PostResponseParserクラスのインスタンスを初期化
		/// </summary>
		/// <param name="htmlData">解析するデータ</param>
		public PostResponseParser(string htmlData)
		{
			// 
			// TODO: コンストラクタ ロジックをここに追加してください。
			//
			htmlText = htmlData;
			response = PostResponse.Error;
			sambaCount = -1;
			plainText = Parse(htmlData);
		}

		/// <summary>
		/// textを解析
		/// </summary>
		/// <param name="data"></param>
		/// <returns></returns>
		protected virtual string Parse(string text)
		{
			if (text == null)
			{
				throw new ArgumentNullException("text");
			}

			try
			{
				// 2ch_Xを検索する正規表現
				Regex regex2chx = new Regex(@"<!--\s*?(2ch_X:\w+)\s*?-->",
					RegexOptions.IgnoreCase | RegexOptions.Singleline);

				// タイトルを検索する正規表現
				Regex regext = new Regex(@"<title>(?<t>.+?)</title>",
					RegexOptions.IgnoreCase | RegexOptions.Singleline);

				// 本文を検索する正規表現
				Regex regexb = new Regex(@"(<body.*?>(?<b>.+)</body>)|(</head>(?<b>.+)</body>)",
					RegexOptions.IgnoreCase | RegexOptions.Singleline);

				// タイトルを取得
				Match match0 = regext.Match(text);
				if (match0.Success)
				{
					title = match0.Groups["t"].Value;
					response = TitleToStatus(title);
				}

				// 2ch_Xを取得
				Match match1 = regex2chx.Match(text);
				if (match1.Success)
				{
					response = x2chXToStatus(match1.Value);
				}

				// 本文を取得（タグを取り除く）
				Match match2 = regexb.Match(text);
				if (match2.Success)
				{
					string result = match2.Groups["b"].Value;

					if (result.IndexOf("書き込み確認") != -1)
						response = PostResponse.Cookie;

					else if (result.IndexOf("クッキーをオンにしてちょ") != -1)
						response = PostResponse.Cookie;

					else if (result.IndexOf("ＥＲＲＯＲ - 593") != -1)
					{
						response = PostResponse.Samba;

						Match m = Regex.Match(result, "593 (?<cnt>\\d+) sec");
						if (m.Success)
							Int32.TryParse(m.Groups["cnt"].Value, out sambaCount);
					}

					result = Regex.Replace(result, "<br>", "\r\n", RegexOptions.IgnoreCase);
					result = Regex.Replace(result, "<hr>", "\r\n", RegexOptions.IgnoreCase);
					result = Regex.Replace(result, "</?[^>]+>", "");
					result = Regex.Replace(result, "\t", "");
					result = Regex.Replace(result, "(\r\n|\r|\n){2,}", "\r\n", RegexOptions.Singleline);
					plainText = result;


					if (response == PostResponse.Cookie)
					{
						MatchCollection matches = Regex.Matches(text, "<input type=hidden name=\"?(\\w+?)\"? value=\"?(\\w+?)\"?>", RegexOptions.IgnoreCase);

						foreach (Match m in matches)
						{
							string name = m.Groups[1].Value;
							string value = m.Groups[2].Value;

							hiddenParamsDic.Add(name, value);
#if DEBUG
							Console.WriteLine("{0}={1}", name, value);
#endif
						}
					}
				}

				if (plainText == String.Empty)
					plainText = htmlText;
			}
			catch (Exception ex)
			{
				TwinDll.Output(ex.ToString());
			}

			return plainText;
		}

		/// <summary>
		/// 2ch_XをPostResponse列挙体で取得
		/// </summary>
		/// <param name="x2chx"></param>
		/// <returns></returns>
		public static PostResponse x2chXToStatus(string x2chx)
		{
			/* http://members.jcom.home.ne.jp/monazilla/document/2ch_x.html
			１、エラー表示にタグを入れました。 
　				<html>の直後に<!-- 2ch_X:***** -->という形で入っています。 
　				区別については以下の通りです。 
　　　			正常終了：<!-- 2ch_X:true -->（正常に書き込みが終了） 
　　　			注意終了：<!-- 2ch_X:false -->（書き込みはしたが注意つき） 
　　　			エラー表示：<!-- 2ch_X:error -->（今はＥＲＲＯＲ！のタイトル） 
　　　			書き込み確認：<!-- 2ch_X:check -->（スレ立てなど書き込み別画面） 
　　　			クッキー確認：<!-- 2ch_X:cookie -->（クッキーを食べさせる画面） 
			２、総量規制を注意発生の場合だけ入れました。 
　				ただ秒数で注意するのではなく、この規制が発生するほど混んでいる時間帯に、 
　				ＡＡなどの大量書き込みや連続書き込みなど、負荷を上げる行為をした場合、 
　				書き込みログを取った上、エラーで書き込めません。 
　				ログインしている人は緩和されるので、あまりツールには関係ありませんが、 
　				エラーになった場合のアクセス規制条件は同じなので、 
　				必ず注意を表示していただければと思います。 
　				関連しているのは以下の２つです。 
　　　			<!-- 2ch_X:check -->：これ以上書くとアクセス禁止になります。。。 
　　　			<!-- 2ch_X:false -->：書き込みは完了しましたが、以下の注意が出ています。。。 
			*/

			Match m = Regex.Match(x2chx, "(?<a>\\w+:\\w+)");
			if (m.Success)
			{
				string a = m.Groups["a"].Value;

				switch (a)
				{
					case "2ch_X:true":
						return PostResponse.Success;
					case "2ch_X:false":
						return PostResponse.Attention;
					case "2ch_X:error":
						return PostResponse.Error;
					case "2ch_X:check":
						return PostResponse.Warning;
					case "2ch_X:cookie":
						return PostResponse.Cookie;
				}
			}
			return PostResponse.None;
		}

		/// <summary>
		/// タイトルをPostResponse列挙体で取得
		/// </summary>
		/// <param name="title"></param>
		/// <returns></returns>
		public static PostResponse TitleToStatus(string title)
		{
			if (title.IndexOf("書きこみました") >= 0)
			{
				return PostResponse.Success;
			}

			if (title.IndexOf("書き込み確認") >= 0 ||
				title.IndexOf("クッキー確認") >= 0 ||
				title.IndexOf("投稿確認") >= 0)
			{
				return PostResponse.Cookie;
			}

			if (title.IndexOf("ＥＲＲＯＲ") >= 0 ||
				title.IndexOf("お茶でも飲みましょう") >= 0)
			{
				return PostResponse.Error;
			}

			return PostResponse.None;

			/*
			switch (title)
			{
			case "書きこみました。":
				return PostResponse.Success;

			case "■ 書き込み確認 ■":
			case "クッキー確認！":
			case "書き込み確認":
			case "書き込み確認。":
			case "投稿確認":
				return PostResponse.Cookie;

			case "ＥＲＲＯＲ！":
			case "お茶でも飲みましょう。":
				return PostResponse.Error;

			default:
				return PostResponse.None;
			}
			*/
		}
	}
}
