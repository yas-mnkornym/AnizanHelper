// PostBase.cs

namespace Twin
{
	using System;
	using System.Collections;
	using System.Net;
	using System.Text;
	using System.Web;

	/// <summary>
	/// 投稿クラスの基本クラス
	/// </summary>
	public abstract class PostBase
	{
		private PostResHandler methodR;
		private PostThreadHandler methodT;

		private DateTime time;
		private Encoding encoding;
		private IWebProxy proxy;
		private string userAgent;

		/// <summary>
		/// 新規スレッドの投稿に対応しているかどうかを取得
		/// </summary>
		public abstract bool CanPostThread
		{
			get;
		}

		/// <summary>
		/// レスの投稿に対応しているかどうかを取得
		/// </summary>
		public abstract bool CanPostRes
		{
			get;
		}

		/// <summary>
		/// サーバーからの応対を取得
		/// </summary>
		public abstract PostResponse Response
		{
			get;
		}

		/// <summary>
		/// メッセージのエンコーディングを取得または設定
		/// </summary>
		public Encoding Encoding
		{
			set
			{
				if (value == null)
					throw new ArgumentNullException("Encoding");
				encoding = value;
			}
			get
			{
				return encoding;
			}
		}

		/// <summary>
		/// 使用するプロキシを取得または設定
		/// </summary>
		public IWebProxy Proxy
		{
			set
			{
				proxy = (value != null) ?
					value : WebRequest.DefaultWebProxy;
			}
			get
			{
				return proxy;
			}
		}

		/// <summary>
		/// 使用するUser-Agentヘッダを取得または設定
		/// </summary>
		public string UserAgent
		{
			set
			{
				if (value == null)
					throw new ArgumentNullException("UserAgent");
				userAgent = value;
			}
			get
			{
				return userAgent;
			}
		}

		/// <summary>
		/// スレ立て時に使用する時刻を取得または設定
		/// </summary>
		public DateTime Time
		{
			set
			{
				time = value;
			}
			get
			{
				return time;
			}
		}

		/// <summary>
		/// 投稿したときに発生
		/// </summary>
		public event PostEventHandler Posted;

		/// <summary>
		/// 投稿エラーが発生したときに発生
		/// </summary>
		public event PostErrorEventHandler Error;

		/// <summary>
		/// PostBaseクラスのインスタンスを初期化
		/// </summary>
		protected PostBase()
		{
			userAgent = TwinDll.UserAgent;
			encoding = TwinDll.DefaultEncoding;
			proxy = WebRequest.DefaultWebProxy;
			time = DateTime.MinValue;
		}

		/// <summary>
		/// 新規スレッドを投稿
		/// </summary>
		/// <param name="board">投稿先の板</param>
		/// <param name="thread">投稿する内容</param>
		public abstract void Post(BoardInfo board, PostThread thread);

		/// <summary>
		/// メッセージを投稿
		/// </summary>
		/// <param name="header">投稿先のスレッド</param>
		/// <param name="res">投稿する内容</param>
		public abstract void Post(ThreadHeader header, PostRes res);

		/// <summary>
		/// 投稿をキャンセル
		/// </summary>
		public abstract void Cancel();

		/// <summary>
		/// 非同期で新規スレッドを投稿
		/// </summary>
		/// <param name="board">投稿先の板</param>
		/// <param name="thread">投稿する内容</param>
		/// <param name="callback">投稿完了時に呼ばれるコールバック</param>
		/// <param name="state">ユーザー指定のオブジェクト</param>
		/// <returns></returns>
		public virtual IAsyncResult BeginPost(BoardInfo board, PostThread thread,
			AsyncCallback callback, object state)
		{
			if (board == null)
				throw new ArgumentNullException("board");

			if (methodR != null ||
				methodT != null)
			{
				throw new InvalidOperationException("一度に複数の非同期呼び出しは出来ません");
			}

			methodT = new PostThreadHandler(Post);
			return methodT.BeginInvoke(board, thread, callback, state);
		}

		/// <summary>
		/// 非同期でメッセージを投稿
		/// </summary>
		/// <param name="header">投稿先のスレッド</param>
		/// <param name="res">投稿する内容</param>
		/// <param name="callback">投稿完了時に呼ばれるコールバック</param>
		/// <param name="state">ユーザー指定のオブジェクト</param>
		/// <returns></returns>
		public virtual IAsyncResult BeginPost(ThreadHeader header, PostRes res,
			AsyncCallback callback, object state)
		{
			if (header == null)
				throw new ArgumentNullException("header");

			if (methodR != null ||
				methodT != null)
			{
				throw new InvalidOperationException("一度に複数の非同期呼び出しは出来ません");
			}

			methodR = new PostResHandler(Post);
			return methodR.BeginInvoke(header, res, callback, state);
		}

		/// <summary>
		/// 投稿が完了するまで待機
		/// </summary>
		/// <param name="ar"></param>
		public virtual void EndPost(IAsyncResult ar)
		{
			if (ar == null)
			{
				throw new ArgumentNullException("ar");
			}

			if (methodR != null)
				methodR.EndInvoke(ar);

			else if (methodT != null)
				methodT.EndInvoke(ar);

			else
			{
				throw new InvalidOperationException("非同期呼び出しが行われていません");
			}

			methodR = null;
			methodT = null;
		}

		/// <summary>
		/// Postedイベントを発生させる
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected virtual void OnPosted(object sender, PostEventArgs e)
		{
			if (Posted != null)
				Posted(sender, e);
		}

		/// <summary>
		/// Errorイベントを発生させる
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected virtual void OnError(object sender, PostErrorEventArgs e)
		{
			if (Error != null)
				Error(sender, e);
		}

		/// <summary>
		/// HttpUtility.UrlEncodeを使用してtextをエンコードする
		/// </summary>
		/// <param name="text"></param>
		/// <returns></returns>
		protected string UrlEncode(string text)
		{
			return HttpUtility.UrlEncode(text, encoding);
		}

		/// <summary>
		/// 投稿時のtime値を取得
		/// </summary>
		/// <param name="baseTime">基になる日時</param>
		/// <returns></returns>
		public static int GetTime(DateTime baseTime)
		{
			TimeSpan t = baseTime.ToUniversalTime().Subtract(new DateTime(1970, 1, 1));
			return (int)(t.TotalSeconds);
		}
	}
}
