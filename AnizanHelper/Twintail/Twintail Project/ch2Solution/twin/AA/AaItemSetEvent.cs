// AaItemSetEvent.cs

namespace Twin.Aa
{
	using System;

	/// <summary>
	/// AaItemCollection.ItemSetイベントを処理するメソッドを表す
	/// </summary>
	internal delegate void AaItemSetEventHandler(object sender, AaItemSetEventArgs e);

	/// <summary>
	/// AaItemCollection.ItemSetイベントのデータを提供
	/// </summary>
	internal class AaItemSetEventArgs : EventArgs
	{
		private readonly AaItem item;

		/// <summary>
		/// 新しく追加されたAaItemクラスのインスタンス
		/// </summary>
		public AaItem Item {
			get { return item; }
		}

		/// <summary>
		/// AaItemSetEventArgsクラスのインスタンスを初期化
		/// </summary>
		/// <param name="aa"></param>
		public AaItemSetEventArgs(AaItem aa)
		{
			if (aa == null) {
				throw new ArgumentNullException("aa");
			}
			// 
			// TODO: コンストラクタ ロジックをここに追加してください。
			//
			item = aa;
		}
	}
}
