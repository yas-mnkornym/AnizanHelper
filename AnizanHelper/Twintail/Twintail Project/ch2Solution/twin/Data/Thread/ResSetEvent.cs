// ResSetEvent.cs

namespace Twin
{
	using System;

	/// <summary>
	/// ResSetEventHandlerデリゲート
	/// </summary>
	public delegate void ResSetEventHandler(object sender, ResSetEventArgs e);

	/// <summary>
	/// ResSetEventHandlerメソッドのデータを提供
	/// </summary>
	public class ResSetEventArgs : EventArgs
	{
		private readonly ResSetCollection resSets;

		/// <summary>
		/// ResSetコレクションを取得
		/// </summary>
		public ResSetCollection Items {
			get { return resSets; }
		}

		/// <summary>
		/// ResSetEventArgsクラスのインスタンスを初期化
		/// </summary>
		/// <param name="items"></param>
		public ResSetEventArgs(ResSetCollection items)
		{
			if (items == null) {
				throw new ArgumentNullException("items");
			}

			this.resSets = items;
		}

		public ResSetEventArgs(ResSet res)
		{
			this.resSets = new ResSetCollection();
			this.resSets.Add(res);
		}
	}
}
