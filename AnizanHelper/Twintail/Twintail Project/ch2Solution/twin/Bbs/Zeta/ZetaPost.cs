// ZetaPost.cs

namespace Twin.Bbs
{
	using System;
	using System.Text;

	/// <summary>
	/// Zetabbsに投稿するクラス
	/// </summary>
	public class ZetaPost : X2chPost
	{
		/// <summary>
		/// ZetaPostクラスのインスタンスを初期化
		/// </summary>
		public ZetaPost()
		{
			// 
			// TODO: コンストラクタ ロジックをここに追加してください。
			//
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

				// CGIの存在するURLを作成
				string uri = String.Format("http://{0}/cgi-bin/test/bbs.cgi", board.Server);

				StringBuilder sb = new StringBuilder();
				sb.Append("subject=" + UrlEncode(thread.Subject));
				sb.Append("&submit=" + UrlEncode("新規スレッド作成"));
				sb.Append("&FROM=" + UrlEncode(thread.From));
				sb.Append("&mail=" + UrlEncode(thread.Email));
				sb.Append("&MESSAGE=" + UrlEncode(thread.Body));
				sb.Append("&bbs=" + board.Path);
				sb.Append("&time=" + time);

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
				bool retried = false;

				string uri = String.Format("http://{0}/cgi-bin/test/bbs.cgi", header.BoardInfo.Server);
				StringBuilder sb = new StringBuilder();
				sb.Append("submit=" + UrlEncode("つっこむ"));
				sb.Append("&FROM=" + UrlEncode(res.From));
				sb.Append("&mail=" + UrlEncode(res.Email));
				sb.Append("&MESSAGE=" + UrlEncode(res.Body));
				sb.Append("&bbs=" + header.BoardInfo.Path);
				sb.Append("&key=" + header.Key);
				sb.Append("&time=" + time);

				byte[] bytes = Encoding.GetBytes(sb.ToString());
				Posting(header.BoardInfo, bytes, uri, ref retried);
			}
			catch (Exception ex) {
				TwinDll.Output(ex);
				throw ex;
			}
		}
	}
}
