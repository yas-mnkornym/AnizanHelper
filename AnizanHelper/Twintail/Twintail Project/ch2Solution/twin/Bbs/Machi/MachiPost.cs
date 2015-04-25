// MachiPost.cs

namespace Twin.Bbs
{
	using System;
	using System.Text;
	using System.IO;
	using System.Net;

	/// <summary>
	/// まちBBS (www.machi.to) に投稿する機能を提供
	/// </summary>
	public class MachiPost : PostBase
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
		/// MachiPostクラスのインスタンスを初期化
		/// </summary>
		public MachiPost()
		{
			// 
			// TODO: コンストラクタ ロジックをここに追加してください。
			//
			response = PostResponse.None;
			Encoding = Encoding.GetEncoding("Shift_Jis");
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

				// CGIの存在するURLを作成
				string uri = String.Format("http://{0}/bbs/write.cgi", board.Server);

				// 送信データを作成
				StringBuilder sb = new StringBuilder();
				sb.Append("SUBJECT=" + UrlEncode(thread.Subject));
				sb.Append("&submit=" + UrlEncode("新規書き込み"));
				sb.Append("&NAME=" + UrlEncode(thread.From));
				sb.Append("&MAIL=" + UrlEncode(thread.Email));
				sb.Append("&MESSAGE=" + UrlEncode(thread.Body));
				sb.Append("&BBS=" + board.Path);
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
				// 投稿時刻を作成
				int time = GetTime(header.LastModified);

				// 送信データを作成
				string uri = String.Format("http://{0}/bbs/write.cgi", header.BoardInfo.Server);
				StringBuilder sb = new StringBuilder();
				sb.Append("submit=" + UrlEncode("書き込む"));
				sb.Append("&NAME=" + UrlEncode(res.From));
				sb.Append("&MAIL=" + UrlEncode(res.Email));
				sb.Append("&MESSAGE=" + UrlEncode(res.Body));
				sb.Append("&BBS=" + header.BoardInfo.Path);
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

		/// <summary>
		/// dataをサーバーに送信しレスポンスを得る
		/// </summary>
		/// <param name="data"></param>
		protected virtual PostResponse Posting(BoardInfo board, byte[] data, string uri, ref bool retried)
		{
			if (board == null) {
				throw new ArgumentNullException("board");
			}
			if (data == null) {
				throw new ArgumentNullException("data");
			}

			HttpWebRequest req = null;
			HttpWebResponse res = null;
			PostResponseParser parser;

			// リファラを作成
			string referer = board.Url + "index.html";

			try {
				req = (HttpWebRequest)WebRequest.Create(uri);
				req.Method = "POST";
				req.Accept = "*/*";
				req.ContentType = "application/x-www-form-urlencoded";
				req.ContentLength = data.Length;
				req.Referer = referer;
				req.Timeout = 30000;
				req.Proxy = Proxy;
				req.UserAgent = UserAgent;
				req.AllowAutoRedirect = false;

				Stream st = req.GetRequestStream();
				st.Write(data, 0, data.Length);
				st.Close();

				res = (HttpWebResponse)req.GetResponse();

				// レスポンスを解析
				using (StreamReader sr = 
						   new StreamReader(res.GetResponseStream(), TwinDll.DefaultEncoding))
				{
					parser = new PostResponseParser(sr.ReadToEnd());
					response = parser.Response;
					res.Close();

					// <TITLE>302 Found</TITLE>が返ってきたら書き込み成功
					if (res.StatusCode == HttpStatusCode.Found)
						response = PostResponse.Success;
				}

				// 投稿イベントを発生させる
				PostEventArgs e = new PostEventArgs(response, parser.Title, parser.PlainText, null, -1);
				OnPosted(this, e);

				// 既にリトライされていたら無限ループ防止のためfalseに設定
				retried = retried ? false : e.Retry;
			}
			catch (WebException ex) {
				if (((HttpWebResponse)ex.Response).StatusCode != HttpStatusCode.Found)
					OnError(this, new PostErrorEventArgs(ex));
			}
			catch (Exception ex) {
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
