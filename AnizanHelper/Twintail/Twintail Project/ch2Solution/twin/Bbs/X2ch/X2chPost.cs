// X2chPost.cs

namespace Twin.Bbs
{
	using System;
	using System.Net;
	using System.IO;
	using System.Text;
	using System.Text.RegularExpressions;
	using System.Diagnostics;
	using System.Threading;
	using Twin.Tools;
	using System.Collections.Generic;

	/// <summary>
	/// ２ちゃんねるに投稿する機能を提供
	/// </summary>
	public class X2chPost : PostBase
	{
		private PostResponse response;
		private bool sendBeID;
		protected readonly string postCGIPath;
		protected readonly string postSubCGIPath;

		public virtual bool SendBeID {
			set { sendBeID = value; }
			get { return sendBeID; }
		}

		/// <summary>
		/// 新規スレッドの投稿に対応しているかどうかを取得
		/// </summary>
		public override bool CanPostThread {
			get { return true; }
		}

		/// <summary>
		/// レスの投稿に対応しているかどうかを取得
		/// </summary>
		public override bool CanPostRes {
			get { return true; }
		}

		/// <summary>
		/// 投稿時のサーバーからのレスポンスを表す値を取得
		/// </summary>
		public override PostResponse Response {
			get { return response; }
		}

		/// <summary>
		/// X2chPostクラスのインスタンスを初期化
		/// </summary>
		public X2chPost()
		{
			// 
			// TODO: コンストラクタ ロジックをここに追加してください。
			//
			sendBeID = false;
			postCGIPath = "test/bbs.cgi";
			response = PostResponse.None;
			Encoding = Encoding.GetEncoding("Shift_Jis");
		}

		protected X2chPost(Encoding encoding) : this()
		{
			Encoding = encoding;
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

				// 送信データを作成
				bool retried = false;
Retry:
				if (retried)
					Thread.Sleep(1000);

				// CGIの存在するURLを作成
				string uri = String.Format("http://{0}/{1}", board.Server, postCGIPath);

				StringBuilder sb = new StringBuilder();
				sb.Append("subject=" + UrlEncode(thread.Subject));
//				sb.Append("&submit=" + (retried ? UrlEncode("全責任を負うことを承諾して書き込む") : UrlEncode("新規スレッド作成")));
				sb.Append("&submit=" + UrlEncode("新規スレッド作成"));
				sb.Append("&FROM=" + UrlEncode(thread.From));
				sb.Append("&mail=" + UrlEncode(thread.Email));
				sb.Append("&MESSAGE=" + UrlEncode(thread.Body));
				sb.Append("&bbs=" + board.Path);
				sb.Append("&time=" + time);

				sb.Append(TwinDll.AditionalAgreementField);
				sb.Append(TwinDll.AddWriteSection);

				AddSessionId(sb);

				byte[] bytes = Encoding.GetBytes(sb.ToString());
				Posting(board, bytes, uri, ref retried);

				if (retried)
					goto Retry;
			}
			catch (Exception ex) {
				TwinDll.Output(ex);
				throw ex;
			}
		}

		private void AddSessionId(StringBuilder sb)
		{
			X2chAuthenticator authenticator = X2chAuthenticator.GetInstance();
			if (authenticator.HasSession)
			{
				sb.AppendFormat("&sid={0}", UrlEncode(authenticator.SessionId));
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
				bool retried = false;
Retry:
				if (retried)
					Thread.Sleep(100);

				string uri = String.Format("http://{0}/{1}", header.BoardInfo.Server, postCGIPath);
				StringBuilder sb = new StringBuilder();
//				sb.Append("submit=" + (retried ? UrlEncode("全責任を負うことを承諾して書き込む") : UrlEncode("書き込む")));
				sb.Append("submit=" + UrlEncode("書き込む"));
				sb.Append("&FROM=" + UrlEncode(res.From));
				sb.Append("&mail=" + UrlEncode(res.Email));
				sb.Append("&MESSAGE=" + UrlEncode(res.Body));
				sb.Append("&bbs=" + header.BoardInfo.Path);
				sb.Append("&key=" + header.Key);
				sb.Append("&time=" + time);
//				sb.Append("&hana=mogera");

				sb.Append(TwinDll.AditionalAgreementField);
				sb.Append(TwinDll.AddWriteSection);

				AddSessionId(sb);

				byte[] bytes = Encoding.GetBytes(sb.ToString());
				Posting(header.BoardInfo, bytes, uri, ref retried);

				if (retried)
					goto Retry;
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

			HttpWebResponse res = null;
			PostResponseParser parser = null;

			// タイムアウト値
			const int timeout = 15000; // 15秒

			// 再試行を行うかどうか
			bool is_retry = true;

			try
			{
				HttpWebRequest req = (HttpWebRequest)WebRequest.Create(uri);
				req.Method = "POST";
				req.ContentType = "application/x-www-form-urlencoded";
				req.ContentLength = data.Length;
				req.Referer = board.Url + "index.html";
				req.UserAgent = UserAgent;
				req.Timeout = timeout;
				req.ReadWriteTimeout = timeout;
				req.Proxy = Proxy;
				//				req.Accept = "text/html, */*";
				//				req.Expect = null;
				//				req.AllowAutoRedirect = false;
				//				req.ProtocolVersion = HttpVersion.Version10;

				// NTwin 2011/05/31
				//req.CookieContainer = GetCookie(board);

				req.CookieContainer = CookieManager.gCookies;
				
#if DEBUG
				foreach (Cookie c in req.CookieContainer.GetCookies(req.RequestUri))
					Console.WriteLine("{0}={1}", c.Name, c.Value);
#endif
				SetHttpWebRequest(req);

				Stream st = req.GetRequestStream();
				st.Write(data, 0, data.Length);
				st.Close();

				res = (HttpWebResponse)req.GetResponse();

				// レスポンスを解析するためのパーサを初期化。
				using (TextReader reader = new StreamReader(res.GetResponseStream(), Encoding))
				{
					parser = new PostResponseParser(reader.ReadToEnd());
					response = parser.Response;

					if (response == PostResponse.Cookie)
					{
						foreach (KeyValuePair<string, string> kv in parser.HiddenParams)
						{
							if (Regex.IsMatch(kv.Key, "subject|FROM|mail|MESSAGE|bbs|time|key") == false)
							{
								TwinDll.AditionalAgreementField = String.Format("&{0}={1}",
									kv.Key, kv.Value);

								Console.WriteLine(TwinDll.AditionalAgreementField);
								break;
							}
						}
					}
				}

				if (res.StatusCode == HttpStatusCode.Found)
					response = PostResponse.Success;

				/*
				board.CookieContainer = new CookieContainer();

				bool ponIsExist = false;

				foreach (Cookie c in req.CookieContainer.GetCookies(req.RequestUri))
				{
#if DEBUG
					Console.WriteLine("{0}={1}", c.Name, c.Value);
#endif
					if (c.Name == "PON")
						ponIsExist = true;
					board.CookieContainer.Add(c);
				}

				if (!ponIsExist && response == PostResponse.Cookie)
				{
					board.CookieContainer.Add(res.Cookies);
				}*/

				// 投稿イベントを発生させる
				PostEventArgs e = new PostEventArgs(response, parser.Title,
					parser.PlainText, null, parser.SambaCount);

				OnPosted(this, e);

				is_retry = e.Retry;
			}
			catch (Exception ex)
			{
#if DEBUG
				WebException webex = ex as WebException;
				if (webex != null)
				{
					TwinDll.ShowOutput("Status " + webex.Status + ", " + webex.ToString());
				}
				else
				{
					TwinDll.ShowOutput(ex);
				}
#endif
				// タイムアウトやそれ以外の例外が発生したら無条件でリトライを中止
				//is_retry = false;
				OnError(this, new PostErrorEventArgs(ex));
			}
			finally {
				if (res != null)
					res.Close();

				// クッキー確認などでの再試行処理
				// ※既に再試行されていたら無限ループ防止のためfalseに設定
				if (retried)
				{
					retried = false;
				}
				else {
					retried = is_retry;
				}
			}

			return response;
		}
		
		/// <summary>
		/// 投稿をキャンセル (未対応)
		/// </summary>
		public override void Cancel()
		{
			throw new NotSupportedException();
		}

		protected virtual void SetHttpWebRequest(HttpWebRequest req)
		{
		}

		//protected CookieContainer SetBeCookie(Uri reqUri, CookieContainer container)
		//{
		//    if (SendBeID && !TwinDll.Be2chCookie.IsEmpty)
		//    {
		//        container.SetCookies(reqUri, "DMDM=" + TwinDll.Be2chCookie.Dmdm);
		//        container.SetCookies(reqUri, "MDMD=" + TwinDll.Be2chCookie.Mdmd);
		//    }
		//    else
		//    {
		//        container.SetCookies(reqUri, "DMDM=");
		//        container.SetCookies(reqUri, "MDMD=");
		//    }

		//    return container;
		//}
		/*
		protected virtual CookieContainer GetCookie(BoardInfo board)
		{
			Uri uri = new Uri("http://" + board.Server + "/");		

			CookieContainer cContainer = new CookieContainer();
			CookieCollection others = new CookieCollection();
			CookieCollection cookies = board.CookieContainer.GetCookies(uri);

			bool dmdm=false, mdmd=false;

			foreach (Cookie c in cookies)
			{
				if (c.Name == "DMDM") dmdm = true;
				else if (c.Name == "MDMD") mdmd = true;
				else						others.Add(c);
			}

			if (SendBeID)
			{
				if (dmdm && mdmd)
				{
					cContainer.Add(cookies);
				}
				else if (!TwinDll.Be2chCookie.IsEmpty)
				{
					cContainer.Add(new Cookie("DMDM", TwinDll.Be2chCookie.Dmdm, "/", board.Server));
					cContainer.Add(new Cookie("MDMD", TwinDll.Be2chCookie.Mdmd, "/", board.Server));
					cContainer.Add(others);
				}
			}
			else {
				cContainer.Add(others);
			}

			return cContainer;
		}*/
	}
}
