// StatusTextEvent.cs

namespace Twin
{
	using System;

	/// <summary>
	/// ClientBase.StatusTextEventHandlerイベントを処理するメソッドを表す
	/// </summary>
	public delegate void StatusTextEventHandler(object sender, 
								StatusTextEventArgs e);

	/// <summary>
	/// ClientBase.StatusTextEventHandlerイベントのデータを提供
	/// </summary>
	public class StatusTextEventArgs : EventArgs
	{
		private readonly string text;

		/// <summary>
		/// ステータスメッセージを取得
		/// </summary>
		public string Text {
			get { return text; }
		}

		/// <summary>
		/// StatusTextEventArgsクラスのインスタンスを初期化
		/// </summary>
		/// <param name="text">ステータスメッセージ</param>
		public StatusTextEventArgs(string text)
		{
			// 
			// TODO: コンストラクタ ロジックをここに追加してください。
			//
			this.text = text;
		}
	}
}
