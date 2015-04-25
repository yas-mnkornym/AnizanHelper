// NumberClickEvent.cs

namespace Twin
{
	using System;
	using Twin;

	/// <summary>
	/// ThreadViewer.NumberClickイベントを処理するメソッドを表す
	/// </summary>
	public delegate void NumberClickEventHandler(object sender, 
					NumberClickEventArgs e);

	/// <summary>
	/// ThreadViewer.NumberClickイベントのデータを提供
	/// </summary>
	public class NumberClickEventArgs : EventArgs
	{
		private readonly ThreadHeader header;
		private readonly ResSet resSet;

		/// <summary>
		/// スレッドのヘッダ情報を取得
		/// </summary>
		public ThreadHeader Header {
			get { return header; }
		}

		/// <summary>
		/// 選択されたレスを取得
		/// </summary>
		public ResSet ResSet {
			get { return resSet; }
		}

		/// <summary>
		/// NumberClickEventArgsクラスのインスタンスを初期化
		/// </summary>
		/// <param name="header"></param>
		/// <param name="res"></param>
		public NumberClickEventArgs(ThreadHeader header, ResSet res)
		{
			this.header = header;
			this.resSet = res;
		}
	}
}
