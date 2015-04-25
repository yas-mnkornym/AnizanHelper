// BoardUpdateEvent.cs

namespace Twin
{
	using System;

	/// <summary>
	/// IBoardTable.OnlineUpdateメソッドに使用するデリゲート
	/// </summary>
	public delegate void BoardUpdateEventHandler(object sender, BoardUpdateEventArgs e);

	/// <summary>
	/// BoardUpdateEventHandlerメソッドのイベントデータ
	/// </summary>
	public class BoardUpdateEventArgs : EventArgs
	{
		private BoardUpdateEvent _event;
		private BoardInfo oldBoard;
		private BoardInfo newBoard;

		/// <summary>
		/// 板一覧イベントの内容を取得
		/// </summary>
		public BoardUpdateEvent Event
		{
			get
			{
				return _event;
			}
		}

		/// <summary>
		/// 更新前の板情報を取得 (EventがChangeの時のみ)
		/// </summary>
		public BoardInfo OldBoard
		{
			get
			{
				return oldBoard;
			}
		}

		/// <summary>
		/// 新しい板情報を取得
		/// </summary>
		public BoardInfo NewBoard
		{
			get
			{
				return newBoard;
			}
		}

		/// <summary>
		/// BoardUpdateEventArgsクラスのインスタンスを初期化
		/// </summary>
		/// <param name="_event">イベントの内容</param>
		/// <param name="old">板が移転したときのみ移転前の板情報を指定</param>
		/// <param name="_new">新しい板情報</param>
		public BoardUpdateEventArgs(BoardUpdateEvent _event, BoardInfo old, BoardInfo _new)
		{
			if (_event == BoardUpdateEvent.Change &&
				old == null)
			{
				throw new ArgumentNullException("old");
			}

			this._event = _event;
			this.oldBoard = old;
			this.newBoard = _new;
		}
	}

	/// <summary>
	/// 板更新イベントの内容
	/// </summary>
	public enum BoardUpdateEvent
	{
		/// <summary>
		/// 板のURLが変更された
		/// </summary>
		Change,
		/// <summary>
		/// 新しくいたが追加された
		/// </summary>
		New,
		/// <summary>
		/// 板の更新がキャンセルされた
		/// </summary>
		Cancelled,
	}
}
