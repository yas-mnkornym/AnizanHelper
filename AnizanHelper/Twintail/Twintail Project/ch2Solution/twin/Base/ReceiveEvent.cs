// ReceiveEvent.cs

namespace Twin
{
	using System;

	/// <summary>
	/// ClientBase.Receiveイベントを処理するメソッドを表す
	/// </summary>
	public delegate void ReceiveEventHandler(object sender, ReceiveEventArgs e);

	/// <summary>
	/// ClientBase.Receiveイベントのデータを提供
	/// </summary>
	public class ReceiveEventArgs : EventArgs
	{
		private readonly int length;
		private readonly int position;
		private readonly int receive;
		
		/// <summary>
		/// ストリームの長さを取得
		/// </summary>
		public int Length {
			get { return length; }
		}
		
		/// <summary>
		/// 現在のストリーム位置を取得
		/// </summary>
		public int Position {
			get { return position; }
		}

		/// <summary>
		/// 受信されたサイズを取得
		/// </summary>
		public int Receive {
			get { return receive; }
		}

		/// <summary>
		/// ReceiveEventArgsクラスのインスタンスを初期化
		/// </summary>
		/// <param name="len">受信対象の総サイズ</param>
		/// <param name="pos">受信済みサイズ</param>
		/// <param name="recv">今回読み込まれたサイズ</param>
		public ReceiveEventArgs(int len, int pos, int recv)
		{
			// 
			// TODO: コンストラクタ ロジックをここに追加してください。
			//
			length = len;
			position = pos;
			receive = recv;
		}
	}
}
