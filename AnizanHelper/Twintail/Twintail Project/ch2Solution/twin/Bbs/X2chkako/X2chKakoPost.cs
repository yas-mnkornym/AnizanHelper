// X2chKakoPost.cs

namespace Twin.Bbs
{
	using System;

	/// <summary>
	/// X2chKakoPost の概要の説明です。
	/// </summary>
	public class X2chKakoPost : X2chPost
	{
		/// <summary>
		/// このプロパティは常にfalseを返す
		/// </summary>
		public override bool CanPostRes {
			get { return false; }
		}

		/// <summary>
		/// このプロパティは常にfalseを返す
		/// </summary>
		public override bool CanPostThread {
			get { return false; }
		}

		/// <summary>
		/// X2chKakoPostクラスのインスタンスを初期化
		/// </summary>
		public X2chKakoPost()
		{
			// 
			// TODO: コンストラクタ ロジックをここに追加してください。
			//
		}

		/// <summary>
		/// 新規スレッドを投稿
		/// </summary>
		/// <param name="board">投稿先の板</param>
		/// <param name="thread">投稿する内容</param>
		public override void Post(BoardInfo board, PostThread thread)
		{
			throw new NotSupportedException("このメソッドはサポートしていません");
		}

		/// <summary>
		/// メッセージを投稿
		/// </summary>
		/// <param name="header">投稿先のスレッド</param>
		/// <param name="res">投稿する内容</param>
		public override void Post(ThreadHeader header, PostRes res)
		{
			throw new NotSupportedException("過去ログには書き込みできません");
		}
	}
}
