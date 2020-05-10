// X2chRetryKakologException.cs

using System;

namespace Twin.Bbs
{
	/// <summary>
	/// X2chRetryKakologException の概要の説明です。
	/// </summary>
	public class X2chRetryKakologException : ApplicationException
	{
		private BoardInfo retryBoard;

		public BoardInfo RetryBoard {
			get {
				return retryBoard;
			}
		}

		/// <summary>
		/// X2chRetryKakologException クラスのインスタンスを初期化
		/// </summary>
		public X2chRetryKakologException(BoardInfo board)
		{
			// 
			// TODO: コンストラクタ ロジックをここに追加してください。
			//
			this.retryBoard = board;
		}
	}
}
