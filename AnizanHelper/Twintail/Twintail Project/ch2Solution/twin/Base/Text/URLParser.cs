using System;
using System.Text.RegularExpressions;

namespace Twin
{
	/// <summary>
	/// スレッド一覧やスレッドのURLを解析するためのパーサ
	/// </summary>
	public class URLParser
	{
		/// <summary>
		/// スレ一覧のURLを解析するための正規表現クラスのインスタンスを表す
		/// (http://www.2ch.net/board/ または http://www.2ch.net/board/index.html に一致
		/// </summary>
		protected static readonly Regex[] ParseListRegexArray =
			new Regex[] {
				new Regex(@"^h?ttp://(?<host>jbbs\.(shitaraba|livedoor)\.(com|jp))/(?<path>\w+/\d+)/?$", RegexOptions.Compiled),	// したらばの板アドレス
				new Regex(@"^h?ttp://(?<host>.+)/(?<path>[^/]+)/(index\d*\.html|\s*$)", RegexOptions.Compiled),			// 全般の板アドレス
			};

		/// <summary>
		/// スレッドのURLを解析するための正規表現
		/// </summary>
		protected static readonly Regex[] ParseThreadRegexArray =
			new Regex[] {
				new Regex(@"^h?ttp://(?<host>[\w\-~/_.]+)/test/(read\.cgi|r\.i)/(?<path>[^/]+)/(?<key>[^/]+)/?", RegexOptions.Compiled),	// 2chまたは2ch互換のURL
				new Regex(@"^h?ttp://(?<host>[\w\-~/_.]+)/test/read\.cgi\?bbs=(?<path>\w+?)&key=(?<key>\w+)", RegexOptions.Compiled),		// 旧2chURL
				new Regex(@"^h?ttp://(?<host>[^/]+)/(?<path>\w+?)/kako/[\d/]+?/(?<key>\d+)\.html", RegexOptions.Compiled),					// 2ch過去ログURL
				new Regex(@"^h?ttp://(?<host>[\w\-~/_.]+)/bbs/read\.pl\?BBS=(?<path>\w+?)&KEY=(?<key>\w+)", RegexOptions.Compiled),			// まちBBSのURL
				new Regex(@"^h?ttp://(?<host>\w+\.(shitaraba|livedoor)\.(com|jp))/bbs/read\.cgi/(?<path>\w+/\d+)/(?<key>[^/]+)/?", RegexOptions.Compiled),		// したらばのURL
			};

		/// <summary>
		/// URLを解析しBoardInfoクラスのインスタンスを初期化
		/// </summary>
		/// <param name="url"></param>
		/// <returns></returns>
		public static BoardInfo ParseBoard(string url)
		{
			foreach (Regex regex in ParseListRegexArray)
			{
				Match m = regex.Match(url);
				if (m.Success)
				{
					BoardInfo b = new BoardInfo();
					b.Server = m.Groups["host"].Value;
					b.Path = m.Groups["path"].Value;
					b.Bbs = BoardInfo.Parse(b.Server);
					return b;
				}
			}
			return null;
		}

		/// <summary>
		/// URLを解析しThreadHeaderクラスのインスタンスを初期化
		/// </summary>
		/// <param name="url"></param>
		/// <returns></returns>
		public static ThreadHeader ParseThread(string url)
		{
			foreach (Regex regex in ParseThreadRegexArray)
			{
				Match m = regex.Match(url);
				if (m.Success)
				{
					BoardInfo board = new BoardInfo();
					board.Server = m.Groups["host"].Value;
					board.Path = m.Groups["path"].Value;

					if (board.Bbs == BbsType.None)
						board.Bbs = BbsType.Dat;

					// 過去ログ倉庫のURLの場合
					if (ParseThreadRegexArray[2].Equals(regex))
						board.Bbs = BbsType.X2chKako;

					ThreadHeader h = TypeCreator.CreateThreadHeader(board.Bbs);
					h.Key = m.Groups["key"].Value;
					h.BoardInfo = board;

					return h;
				}
			}
			return null;
		}

		/// <summary>
		/// URLを解析しBoardInfoクラスのインスタンスを初期化
		/// </summary>
		/// <param name="url"></param>
		/// <returns></returns>
		public static bool IsBoardUrl(string url)
		{
			foreach (Regex regex in ParseListRegexArray)
			{
				Match m = regex.Match(url);
				if (m.Success)
					return true;
			}
			return false;
		}

		/// <summary>
		/// URLを解析しThreadHeaderクラスのインスタンスを初期化
		/// </summary>
		/// <param name="url"></param>
		/// <returns></returns>
		public static bool IsThreadUrl(string url)
		{
			foreach (Regex regex in ParseThreadRegexArray)
			{
				Match m = regex.Match(url);
				if (m.Success)
					return true;
			}
			return false;
		}
	}
}
