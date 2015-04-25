// X2chServerTracer.cs

namespace Twin.Tools
{
	using System;
	using System.IO;
	using System.Text;
	using System.Text.RegularExpressions;
	using System.Net;
	using Twin.Text;

	/// <summary>
	/// 2chのサーバー移転追跡
	/// </summary>
	public class X2chServerTracer
	{
		private BoardInfoCollection traceList;
		private BoardInfo result;

		/// <summary>
		/// 板の追跡履歴を取得
		/// </summary>
		public BoardInfoCollection TraceList {
			get { return traceList; }
		}

		/// <summary>
		/// 移転先を取得
		/// </summary>
		public BoardInfo Result {
			get { return result; }
		}

		/// <summary>
		/// 板の追跡に成功したときに発生するイベント
		/// </summary>
		public event EventHandler<ServerChangeEventArgs> Tracing;

		/// <summary>
		/// X2chServerTracerクラスのインスタンスを初期化
		/// </summary>
		public X2chServerTracer()
		{
			// 
			// TODO: コンストラクタ ロジックをここに追加してください。
			//
			traceList = new BoardInfoCollection();
			result = null;
		}

		/// <summary>
		/// 指定した板の移転を追跡
		/// </summary>
		/// <param name="board">追跡する板</param>
		/// <param name="recursive">移転先がさらに移転していた場合、再起追跡するかどうか</param>
		/// <returns>追跡できればtrue、失敗すればfalseを返す</returns>
		public bool Trace(BoardInfo board, bool recursive)
		{
			if (board == null) {
				throw new ArgumentNullException("board");
			}

			traceList.Clear();
			result = null;
Check:
			// Htmlデータを取得
			HttpWebRequest req = (HttpWebRequest)WebRequest.Create(board.Url);
			req.UserAgent = TwinDll.UserAgent;
			req.AddRange(0, 499);

			HttpWebResponse res = (HttpWebResponse)req.GetResponse();
			string html;
			
			using (StreamReader sr = new StreamReader(res.GetResponseStream(), Encoding.GetEncoding("Shift_Jis")))
				html = sr.ReadToEnd();

			res.Close();
			
			// サーバー移転
			if (html.IndexOf("2chbbs..") >= 0)
			{
				TwinDll.Output("{0} が移転しています。", board.Url);

				// 移転先のURLを取得
				Match m = Regex.Match(html, "<a href=\"(?<url>.+?)\">GO !</a>", RegexOptions.IgnoreCase);
				if (m.Success)
				{
					string newUrl = m.Groups["url"].Value;

					TwinDll.Output("移転先 {0} を発見しました。", newUrl);

					result = URLParser.ParseBoard(m.Groups["url"].Value);
					if (result != null)
					{
						result.Name = board.Name;
						traceList.Add(result);

						OnTracing(new ServerChangeEventArgs(board, result));

						if (recursive)
						{
							board = result;
							html = null;

							goto Check;
						}
					}
				}
			}
			// 追跡終了した場合は板名を取得
			else if (result != null)
			{
				if (String.IsNullOrEmpty(result.Name))
				{
					Match m = Regex.Match(html, "<title>(?<t>.+?)</title>", RegexOptions.IgnoreCase);
					if (m.Success)
					{
						result.Name = m.Groups["t"].Value;
					}
				}
				TwinDll.Output("{0} の追跡に成功しました。", result.Name);
			}

			return (result != null) ? true : false;
		}

		/// <summary>
		/// Tracingイベントを発生させる
		/// </summary>
		/// <param name="e"></param>
		protected void OnTracing(ServerChangeEventArgs e)
		{
			if (Tracing != null)
				Tracing(this, e);
		}
	}
}
