// Kotehan.cs

namespace Twin.Tools
{
	using System;

	/// <summary>
	/// １つのコテハンを表す
	/// </summary>
	public class Kotehan
	{
		private string name;
		private string email;
		private bool be;

		/// <summary>
		/// 名前を取得または設定
		/// </summary>
		public string Name {
			set {
				if (value == null)
					throw new ArgumentNullException("Name");
				name = value;
			}
			get { return name; }
		}

		/// <summary>
		/// E-mailアドレスを取得または設定
		/// </summary>
		public string Email {
			set {
				if (value == null)
					throw new ArgumentNullException("Email");
				email = value;
			}
			get { return email; }
		}

		/// <summary>
		/// BeIDを送信するなら true、それ以外は false を表す。
		/// </summary>
		public bool Be {
			set {
				be = value;
			}
			get { return be; }
		}

		/// <summary>
		/// 空のコテハンかどうかを判断
		/// </summary>
		public bool IsEmpty {
			get {
				return (name == "" && email == "" && be == false) ? true : false;
			}
		}

		/// <summary>
		/// Kotehanクラスのインスタンスを初期化
		/// </summary>
		public Kotehan()
		{
			// 
			// TODO: コンストラクタ ロジックをここに追加してください。
			//
			name = String.Empty;
			email = String.Empty;
			be = false;
		}

		/// <summary>
		/// Kotehanクラスのインスタンスを初期化
		/// </summary>
		/// <param name="name">名前</param>
		/// <param name="email">Emailアドレス</param>
		/// <param name="be">BeのOn/Off</param>
		public Kotehan(string name, string email, bool be)
		{
			if (name == null)
				throw new ArgumentNullException("name");
			if (email == null)
				throw new ArgumentNullException("email");

			this.name = name;
			this.email = email;
			this.be = be;
		}

		/// <summary>
		/// このインスタンスを文字列形式に変換
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			return String.Format("名前: {0}, E-mail: {1}, Be={2}",
				name, email, be);
		}
	}
}
