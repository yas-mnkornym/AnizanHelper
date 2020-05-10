// PatrolEvent.cs

namespace Twin.Tools
{
	using System;

	/// <summary>
	/// PatrolBaseクラスのイベントを処理するメソッド
	/// </summary>
	public delegate void PatrolEventHandler(object sender, PatrolEventArgs e);

	/// <summary>
	/// PatrolBaseクラスのイベントデータを提供
	/// </summary>
	public class PatrolEventArgs : EventArgs
	{
		private readonly ThreadHeader header;
		private bool cancel;

		/// <summary>
		/// 巡回対象のスレッド情報を取得
		/// </summary>
		public ThreadHeader HeaderInfo {
			get { return header; }
		}

		/// <summary>
		/// 巡回をキャンセルする場合はtrueに設定
		/// </summary>
		public bool Cancel {
			set { cancel = value; }
			get { return cancel; }
		}

		/// <summary>
		/// PatrolEventArgsクラスのインスタンスを初期化
		/// </summary>
		/// <param name="header"></param>
		public PatrolEventArgs(ThreadHeader header)
		{
			// 
			// TODO: コンストラクタ ロジックをここに追加してください。
			//
			this.header = header;
			this.cancel = false;
		}
	}
}
