// ServerChangeEvent.cs
// #2.0

namespace Twin
{
	using System;

	/// <summary>
	/// ThreadListReader.ServerChange イベントのデータを提供します。
	/// </summary>
	public class ServerChangeEventArgs : EventArgs
	{
		private BoardInfo old;
		private BoardInfo _new;
		private BoardInfoCollection traceList;

		/// <summary>
		/// 移転元の板情報を取得します。
		/// </summary>
		public BoardInfo OldBoard
		{
			get
			{
				return old;
			}
		}

		/// <summary>
		/// 移転先の板情報を取得します。
		/// </summary>
		public BoardInfo NewBoard
		{
			get
			{
				return _new;
			}
		}

		/// <summary>
		/// 板を追跡した場合はここに追跡履歴が格納されます。
		/// </summary>
		public BoardInfoCollection TraceList
		{
			get
			{
				return traceList;
			}
		}

		/// <summary>
		/// ServerChangeEventArgsクラスのインスタンスを初期化
		/// </summary>
		/// <param name="old"></param>
		/// <param name="_new"></param>
		public ServerChangeEventArgs(BoardInfo old, BoardInfo _new)
		{
			// 
			// TODO: コンストラクタ ロジックをここに追加してください。
			//
			this.old = old;
			this._new = _new;
			this.traceList = null;
		}

		/// <summary>
		/// ServerChangeEventArgsクラスのインスタンスを初期化
		/// </summary>
		/// <param name="old"></param>
		/// <param name="_new"></param>
		/// <param name="history"></param>
		public ServerChangeEventArgs(BoardInfo old, BoardInfo _new, BoardInfoCollection history)
			: this(old, _new)
		{
			// 
			// TODO: コンストラクタ ロジックをここに追加してください。
			//
			this.traceList = history;
		}
	}
}
