// PostThread.cs

namespace Twin
{
	using System;

	/// <summary>
	/// 投稿するスレッドを表す
	/// </summary>
	public struct PostThread
	{
		private string _subject;
		private string _from;
		private string _email;
		private string _body;

		/// <summary>
		/// 投稿者の名前を取得または設定
		/// </summary>
		public string From {
			set {
				if (value == null)
					throw new ArgumentNullException("From");

				_from = value;
			}
			get { return _from; }
		}

		/// <summary>
		/// 投稿者のE-mailを取得または設定
		/// </summary>
		public string Email {
			set {
				if (value == null)
					throw new ArgumentNullException("Email");

				_email = value;
			}
			get { return _email; }
		}

		/// <summary>
		/// 本分を取得または設定
		/// </summary>
		public string Body {
			set {
				if (value == null)
					throw new ArgumentNullException("Body");

				_body = value;
			}
			get { return _body; }
		}

		/// <summary>
		/// スレッド名を取得または設定
		/// </summary>
		public string Subject {
			set {
				if (value == null)
					throw new ArgumentNullException("Subject");

				_subject = value;
			}
			get { return _subject; }
		}

		/// <summary>
		/// PostThreadクラスのインスタンスを初期化
		/// </summary>
		/// <param name="subj">新規スレッド名</param>
		/// <param name="name">投稿者の名前</param>
		/// <param name="email">投稿者のE-mail</param>
		/// <param name="body">本分</param>
		public PostThread(string subj, string from, string email, string body)
		{
			_subject = subj;
			_from = from;
			_email = email;
			_body = body;
		}

		/// <summary>
		/// PostThreadクラスのインスタンスを初期化
		/// </summary>
		/// <param name="subj">新規スレッド名</param>
		/// <param name="body">本分</param>
		public PostThread(string subj, string body)
			: this(subj, String.Empty, String.Empty, body)
		{
		}
	}
}
