// CSharpToolBarButtonEvent.cs

namespace CSharpSamples
{
	using System;

	/// <summary>
	/// CSharpToolBar.ButtonClickイベントを処理するデリゲート
	/// </summary>
	public delegate void CSharpToolBarButtonEventHandler(
			object sender, CSharpToolBarButtonEventArgs e);

	/// <summary>
	/// CSharpToolBar.ButtonClickイベントのデータを提供
	/// </summary>
	public class CSharpToolBarButtonEventArgs : EventArgs
	{
		private readonly CSharpToolBarButton button;

		/// <summary>
		/// クリックされたボタンを取得
		/// </summary>
		public CSharpToolBarButton Button
		{
			get
			{
				return this.button;
			}
		}

		/// <summary>
		/// CSharpToolBarButtonEventArgsクラスのインスタンスを初期化
		/// </summary>
		/// <param name="button">クリックされたボタン</param>
		public CSharpToolBarButtonEventArgs(CSharpToolBarButton button)
		{
			if (button == null)
			{
				throw new ArgumentNullException("button");
			}
			this.button = button;
		}
	}
}
