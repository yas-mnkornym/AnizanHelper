// TabButtonEvent.cs

using System;

namespace CSharpSamples
{
	/// <summary>
	/// TabButtonControl.SelectedChanged イベントを処理するメソッドです。
	/// </summary>
	public delegate void TabButtonEventHandler(object sender,
		TabButtonEventArgs e);

	/// <summary>
	/// TabButtonEventHandler メソッドの引数を表します。
	/// </summary>
	public class TabButtonEventArgs : EventArgs
	{
		/// <summary>
		/// 新しく選択された TabButton を取得します。
		/// </summary>
		public TabButton Button
		{
			get
			{
				return this.button;
			}
		}
		private TabButton button;

		/// <summary>
		/// TabButtonEventEventArgs クラスのインスタンスを初期化。
		/// </summary>	
		public TabButtonEventArgs(TabButton button)
		{
			// 
			// TODO: コンストラクタ ロジックをここに追加してください。
			//
			this.button = button;
		}
	}
}
