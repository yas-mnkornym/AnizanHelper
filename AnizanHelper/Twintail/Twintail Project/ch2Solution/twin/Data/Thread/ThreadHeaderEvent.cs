// ThreadHeaderEvent.cs

namespace Twin
{
	using System;
	using System.Collections.Generic;

	/// <summary>
	/// ThreadHeaderEventHandlerデリゲート
	/// </summary>
	public delegate void ThreadHeaderEventHandler(object sender, ThreadHeaderEventArgs e);

	/// <summary>
	/// ThreadHeaderEvent の概要の説明です。
	/// </summary>
	public class ThreadHeaderEventArgs : EventArgs
	{
		private readonly List<ThreadHeader> headerCollection;

		/// <summary>
		/// ThreadHeaderのコレクションを取得
		/// </summary>
		public List<ThreadHeader> Items {
			get {
				return headerCollection;
			}
		}

		/// <summary>
		/// ThreadHeaderEventArgsクラスのインスタンスを初期化
		/// </summary>
		public ThreadHeaderEventArgs(List<ThreadHeader> items)
		{
			if (items == null) {
				throw new ArgumentNullException("items");
			}
			// 
			// TODO: コンストラクタ ロジックをここに追加してください。
			//
			headerCollection = items;
		}

		/// <summary>
		/// ThreadHeaderEventArgsクラスのインスタンスを初期化
		/// </summary>
		public ThreadHeaderEventArgs(ThreadHeader header)
		{
			if (header == null) {
				throw new ArgumentNullException("header");
			}
			headerCollection = new List<ThreadHeader>();
			headerCollection.Add(header);
		}
	}
}
