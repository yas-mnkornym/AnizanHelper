// UriClickEvent.cs

namespace Twin
{
	using System;

	/// <summary>
	/// ThreadViewer.UriClickイベントを処理するメソッドを表す
	/// </summary>
	public delegate void UriClickEventHandler(object sender, UriClickEventArgs e);

	/// <summary>
	/// ThreadViewer.UriClickイベントのデータを提供
	/// </summary>
	public class UriClickEventArgs : EventArgs
	{
		private readonly string uri;
		private readonly LinkInfo info;

		/// <summary>
		/// クリックされたURIを取得
		/// </summary>
		public string Uri {
			get { return uri; }
		}

		/// <summary>
		/// このリンクに関連づけられている情報を取得
		/// </summary>
		public LinkInfo Information {
			get { return info; }
		}

		/// <summary>
		/// UriClickEventArgsクラスのインスタンスを初期化
		/// </summary>
		public UriClickEventArgs(string uri)
		{
			// 
			// TODO: コンストラクタ ロジックをここに追加してください。
			//
			this.uri = uri;
			this.info = null;
		}

		/// <summary>
		/// UriClickEventArgsクラスのインスタンスを初期化
		/// </summary>
		public UriClickEventArgs(string uri, LinkInfo info)
		{
			// 
			// TODO: コンストラクタ ロジックをここに追加してください。
			//
			this.uri = uri;
			this.info = info;
		}
	}
}
