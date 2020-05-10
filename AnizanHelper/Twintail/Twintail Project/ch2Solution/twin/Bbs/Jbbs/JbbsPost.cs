// MachiPost.cs

namespace Twin.Bbs
{
	using System;
	using System.Text;
	using System.IO;
	using System.Net;
	using System.Diagnostics;

	/// <summary>
	/// jbbsに投稿する機能を提供
	/// </summary>
	public class JbbsPost : PostBase
	{
		private PostResponse response;

		/// <summary>
		/// レスを投稿できるかどうかを示す値を取得 (このプロパティは常にtrueを返す)
		/// </summary>
		public override bool CanPostRes {
			get { return true; }
		}

		/// <summary>
		/// スレッドを投稿できるかどうかを示す値を取得 (このプロパティは常にfalseを返す)
		/// </summary>
		public override bool CanPostThread {
			get { return true; }
		}

		/// <summary>
		/// サーバーからの応対を取得
		/// </summary>
		public override PostResponse Response {
			get { return response; }
		}

		/// <summary>
		/// JbbsPostクラスのインスタンスを初期化
		/// </summary>
		public JbbsPost()
		{
			// 
			// TODO: コンストラクタ ロジックをここに追加してください。
			//
			Encoding = Encoding.GetEncoding("EUC-JP");
			response = PostResponse.None;
		}

		/// <summary>
		/// 新規スレッドを投稿
		/// </summary>
		/// <param name="board">投稿先の板</param>
		/// <param name="thread">投稿する内容</param>
		public override void Post(BoardInfo board, PostThread thread)
		{
			try {
				// 投稿時刻を作成
				int time = GetTime(Time);

				string[] dirbbs = board.Path.Split('/');

				// CGIの存在するURLを作成
				string uri = String.Format("http://{0}/bbs/write.cgi/{1}/new/", board.Server, board.Path);

				// 送信データを作成
				StringBuilder sb = new StringBuilder();
				sb.Append("SUBJECT=" + UrlEncode(thread.Subject));
				sb.Append("&submit=" + UrlEncode("新規書き込み"));
				sb.Append("&NAME=" + UrlEncode(thread.From));
				sb.Append("&MAIL=" + UrlEncode(thread.Email));
				sb.Append("&MESSAGE=" + UrlEncode(thread.Body));
				sb.Append("&DIR=" + dirbbs[0]);
				sb.Append("&BBS=" + dirbbs[1]);
				sb.Append("&TIME=" + time);

				bool retried = false;
				byte[] bytes = Encoding.GetBytes(sb.ToString());

				Posting(board, bytes, uri, ref retried);
			}
			catch (Exception ex) {
				TwinDll.Output(ex);
				throw ex;
			}
		}

		/// <summary>
		/// メッセージを投稿
		/// </summary>
		/// <param name="header">投稿先のスレッド</param>
		/// <param name="res">投稿する内容</param>
		public override void Post(ThreadHeader header, PostRes res)
		{
			if (header == null) {
				throw new ArgumentNullException("header");
			}

			try {
				BoardInfo board = header.BoardInfo;

				// 投稿時刻を作成
				int time = GetTime(header.LastModified);

				// BoardInfo.PathをBBSとDIRに分割
				string[] dirbbs = board.Path.Split('/');
				Trace.Assert(dirbbs.Length == 2);

				// 送信データを作成
				string uri = String.Format("http://{0}/bbs/write.cgi/{1}/{2}/", board.Server, board.Path, header.Key);
				StringBuilder sb = new StringBuilder();
				sb.Append("submit=" + UrlEncode("書き込む"));
				sb.Append("&NAME=" + UrlEncode(res.From));
				sb.Append("&MAIL=" + UrlEncode(res.Email));
				sb.Append("&MESSAGE=" + UrlEncode(res.Body));
				sb.Append("&DIR=" + dirbbs[0]);
				sb.Append("&BBS=" + dirbbs[1]);
				sb.Append("&KEY=" + header.Key);
				sb.Append("&TIME=" + time);

				bool retried = false;
				byte[] bytes = Encoding.GetBytes(sb.ToString());

				Posting(header.BoardInfo, bytes, uri, ref retried);
			}
			catch (Exception ex) {
				TwinDll.Output(ex);
				throw ex;
			}
		}

		protected virtual PostResponse Posting(BoardInfo board, byte[] data, string uri, ref bool retried)
		{
			HttpWebResponse res = null;
			PostResponseParser parser;

			const int timeout = 30000;
			string referer = board.Url + "index.html";

			try {
				HttpWebRequest req = (HttpWebRequest)WebRequest.Create(uri);
				req.Method = "POST";
				req.Accept = "*/*";
				req.ContentType = "application/x-www-form-urlencoded";
				req.ContentLength = data.Length;
				req.Referer = referer;
				req.Timeout = timeout;
				req.ReadWriteTimeout = timeout;
				req.UserAgent = UserAgent;
				req.AllowAutoRedirect = false;
				req.Proxy = Proxy;

				Stream st = req.GetRequestStream();
				st.Write(data, 0, data.Length);
				st.Close();

				res = (HttpWebResponse)req.GetResponse();

				// レスポンスを解析
				using (TextReader reader = new StreamReader(res.GetResponseStream(), Encoding))
				{
					parser = new PostResponseParser(reader.ReadToEnd());
					response = parser.Response;

					// <TITLE>302 Found</TITLE>が返ってきたら書き込み成功
//					if (res.StatusCode == HttpStatusCode.Found)
//						response = PostResponse.Success;
				}

				// 投稿イベントを発生させる
				PostEventArgs e = new PostEventArgs(response, parser.Title, parser.PlainText, null, -1);
				OnPosted(this, e);

				// 既にリトライされていたら無限ループ防止のためfalseに設定
				retried = retried ? false : e.Retry;
			}
			catch (Exception ex)
			{
				TwinDll.Output(ex);
				OnError(this, new PostErrorEventArgs(ex));
			}
			finally {
				if (res != null)
					res.Close();
			}

			return response;
		}

		/// <summary>
		/// 投稿をキャンセルする
		/// </summary>
		public override void Cancel()
		{
			throw new NotSupportedException();
		}
	}
}
