// WroteRes.cs

namespace Twin
{
	using System;
	using System.Text;

	/// <summary>
	/// 書き込み履歴を表す
	/// </summary>
	public class WroteRes
	{
		private DateTime date;
		private string from;
		private string email;
		private string message;

		public ThreadHeader HeaderInfo { get; set; }

		/// <summary>
		/// このレスを書いた日付・時刻で取得
		/// </summary>
		public DateTime Date {
			set { date = value; }
			get { return date; }
		}

		/// <summary>
		/// 投稿者の名前を取得または設定
		/// </summary>
		public string From {
			set {
				if (value == null) {
					throw new ArgumentNullException("From");
				}
				from = value;
			}
			get { return from; }
		}

		/// <summary>
		/// 投稿者のEmailを取得または設定
		/// </summary>
		public string Email {
			set {
				if (value == null) {
					throw new ArgumentNullException("Email");
				}
				email = value;
			}
			get { return email; }
		}

		/// <summary>
		/// メッセージを取得または設定
		/// </summary>
		public string Message {
			set {
				if (value == null) {
					throw new ArgumentNullException("Message");
				}
				message = value;
			}
			get { return message; }
		}

		/// <summary>
		/// Messageプロパティの長さをバイト単位で取得
		/// </summary>
		public int Length {
			get {
				return TwinDll.DefaultEncoding.GetByteCount(message);
			}
		}

		/// <summary>
		/// WroteResクラスのインスタンスを初期化
		/// </summary>
		public WroteRes()
		{
			// 
			// TODO: コンストラクタ ロジックをここに追加してください。
			//
			this.HeaderInfo = null;
			this.from = String.Empty;
			this.email = String.Empty;
			this.message = String.Empty;
		}

		/// <summary>
		/// WroteResクラスのインスタンスを初期化
		/// </summary>
		/// <param name="date">投稿日</param>
		/// <param name="from">投稿者名</param>
		/// <param name="email">投稿者のemail</param>
		/// <param name="msg">メッセージ</param>
		public WroteRes(ThreadHeader header, DateTime date, string from, string email, string msg) : this()
		{
			this.HeaderInfo = header;
			this.Date = date;
			this.From = from;
			this.Email = email;
			this.Message = msg;
		}

		public WroteRes(DateTime date, string from, string email, string msg)
			: this(null, date, from, email, msg)
		{
		}
	}
}
