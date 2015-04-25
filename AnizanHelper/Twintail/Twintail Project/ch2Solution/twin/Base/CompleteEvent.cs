// CompleteEvent.cs

namespace Twin
{
	using System;

	/// <summary>
	/// ClientBase.Completeイベントを処理するメソッドを表す
	/// </summary>
	public delegate void CompleteEventHandler(object sender,
					CompleteEventArgs e);

	/// <summary>
	/// ClientBase.Completeイベントのデータを提供
	/// </summary>
	public class CompleteEventArgs : EventArgs
	{
		private CompleteStatus status;

		/// <summary>
		/// 完了状態を表す
		/// </summary>
		public CompleteStatus Status {
			get { return status; }
		}

		/// <summary>
		/// CompleteEventArgsクラスのインスタンスを初期化
		/// </summary>
		/// <param name="status">クライアントの完了状態</param>
		public CompleteEventArgs(CompleteStatus status)
		{
			// 
			// TODO: コンストラクタ ロジックをここに追加してください。
			//
			this.status = status;
		}
	}
}
