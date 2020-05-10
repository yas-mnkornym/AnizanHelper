// ThreadListEvent.cs

namespace Twin
{
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;

	/// <summary>
	/// ThreadListEventArgs の概要の説明です。
	/// </summary>
	public class ThreadListEventArgs : EventArgs
	{
		private ReadOnlyCollection<ThreadHeader> collection;

		/// <summary>
		/// ThreadHeaderのコレクションを取得
		/// </summary>
		public ReadOnlyCollection<ThreadHeader> Items
		{
			get {
				return collection;
			}
		}

		public ThreadListEventArgs(ReadOnlyCollection<ThreadHeader> collection)
		{
			this.collection = collection;
		}

		/// <summary>
		/// ThreadListEventArgsクラスのインスタンスを初期化
		/// </summary>
		public ThreadListEventArgs(List<ThreadHeader> list)
		{
			if (list == null)
			{
				throw new ArgumentNullException("list");
			}
			// 
			// TODO: コンストラクタ ロジックをここに追加してください。
			//
			collection = new ReadOnlyCollection<ThreadHeader>(list);
		}

		/// <summary>
		/// ThreadListEventArgsクラスのインスタンスを初期化
		/// </summary>
		public ThreadListEventArgs(ThreadHeader header)
		{
			if (header == null) {
				throw new ArgumentNullException("header");
			}

			List<ThreadHeader> list = new List<ThreadHeader>();
			list.Add(header);

			collection = new ReadOnlyCollection<ThreadHeader>(list);
		}
	}
}
