// Draft.cs

namespace Twin.Tools
{
	using System;

	/// <summary>
	/// メッセージの草稿を表す
	/// </summary>
	public class Draft
	{
		private ThreadHeader headerInfo;
		private PostRes postRes;

		/// <summary>
		/// 投稿先のスレッド情報を取得
		/// </summary>
		public ThreadHeader HeaderInfo {
			get { return headerInfo; }
		}

		/// <summary>
		/// 投稿するメッセージを取得
		/// </summary>
		public PostRes PostRes {
			get { return postRes; }
		}

		/// <summary>
		/// Draftクラスのインスタンスを初期化
		/// </summary>
		/// <param name="header">投稿先のスレッド情報</param>
		/// <param name="res">投稿メッセージ</param>
		public Draft(ThreadHeader header, PostRes res)
		{
			// 
			// TODO: コンストラクタ ロジックをここに追加してください。
			//
			this.headerInfo = header;
			this.postRes = res;
		}
	}
}
