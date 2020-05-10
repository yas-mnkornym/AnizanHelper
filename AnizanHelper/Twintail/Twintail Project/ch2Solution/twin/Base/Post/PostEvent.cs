// PostEvent.cs

namespace Twin
{
	using System;

	/// <summary>
	/// IPost.BeginPostメソッドの非同期処理をするためのデリゲートを表す
	/// </summary>
	internal delegate void PostThreadHandler(BoardInfo board, PostThread thread);

	/// <summary>
	/// IPost.BeginPostメソッドの非同期処理をするためのデリゲートを表す
	/// </summary>
	internal delegate void PostResHandler(ThreadHeader header, PostRes res);

	/// <summary>
	/// IPost.Postedイベントを処理するメソッドを表す
	/// </summary>
	public delegate void PostEventHandler(object sender, PostEventArgs e);

	/// <summary>
	/// IPost.Errorイベントを処理するメソッドを表す
	/// </summary>
	public delegate void PostErrorEventHandler(object sender, PostErrorEventArgs e);

	/// <summary>
	/// IPost.Postedイベントのデータを提供
	/// </summary>
	public class PostEventArgs : EventArgs
	{
		private readonly PostResponse response;
		private readonly string text;
		private readonly string title;
		private readonly string cookie;
		private readonly int sambaCount;
		private bool retry;

		/// <summary>
		/// 再度投稿する場合はtrueに設定
		/// </summary>
		public bool Retry
		{
			set
			{
				if (retry != value)
					retry = value;
			}
			get
			{
				return retry;
			}
		}

		/// <summary>
		/// サーバーから返されたクッキーを取得
		/// </summary>
		public string Cookie
		{
			get
			{
				return cookie;
			}
		}

		/// <summary>
		/// Sambaエラー時の時のみ、サーバーのSamba秒数を取得
		/// </summary>
		public int SambaCount
		{
			get
			{
				return sambaCount;
			}
		}

		/// <summary>
		/// 投稿時にサーバーから帰ってきた状態を取得
		/// </summary>
		public PostResponse Response
		{
			get
			{
				return response;
			}
		}

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
		/// 投稿時にサーバーから帰ってきたメッセージを取得
		/// </summary>
		public string Text
		{
			get
			{
				return text;
			}
		}

		/// <summary>
		/// PostEventArgsクラスのインスタンスを初期化
		/// </summary>
		/// <param name="res">サーバーからの応対状態を表す</param>
		/// <param name="title">メッセージのタイトルを表す</param>
		/// <param name="message">メッセージの本分を表す</param>
		public PostEventArgs(PostResponse res, string title, string message, string cookie, int samba)
		{
			// 
			// TODO: コンストラクタ ロジックをここに追加してください。
			//
			this.retry = false;
			this.title = title;
			this.cookie = cookie;
			this.response = res;
			this.text = message;
			this.sambaCount = samba;
		}
	}

	/// <summary>
	/// IPost.Errorイベントのデータを提供
	/// </summary>
	public class PostErrorEventArgs : EventArgs
	{
		private readonly Exception exception;

		/// <summary>
		/// 発生した例外を取得
		/// </summary>
		public Exception Exception
		{
			get
			{
				return exception;
			}
		}

		/// <summary>
		/// PostErrorEventArgsクラスのインスタンスを初期化
		/// </summary>
		/// <param name="ex">例外クラスのインスタンス</param>
		public PostErrorEventArgs(Exception ex)
		{
			// 
			// TODO: コンストラクタ ロジックをここに追加してください。
			//
			this.exception = ex;
		}
	}
}
