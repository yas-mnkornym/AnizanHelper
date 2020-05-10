// AaItemEvent.cs

namespace Twin.Aa
{
	using System;

	/// <summary>
	/// AaItemEventHandler デリゲート
	/// </summary>
	public delegate void AaItemEventHandler(object sender, AaItemEventArgs e);

	/// <summary>
	/// AaItemEventArgs の概要の説明です。
	/// </summary>
	public class AaItemEventArgs : EventArgs
	{
		private readonly AaItem item;

		/// <summary>
		/// AaItemを取得
		/// </summary>
		public AaItem Item {
			get { return item; }
		}

		/// <summary>
		/// AaItemEventArgsクラスのインスタンスを初期化
		/// </summary>
		/// <param name="aa"></param>
		public AaItemEventArgs(AaItem aa)
		{
			// 
			// TODO: コンストラクタ ロジックをここに追加してください。
			//
			item = aa;
		}
	}
}
