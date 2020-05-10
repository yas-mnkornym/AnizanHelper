// IThreadFormatter.cs

namespace Twin.Text
{
	using System;

	/// <summary>
	/// スレッドの書式化を行う基本抽象クラス
	/// </summary>
	public abstract class ThreadFormatter
	{
		/// <summary>
		/// 指定したレスを書式化して文字列に変換
		/// </summary>
		public abstract string Format(ResSet resSet);

		/// <summary>
		/// 指定したレスコレクションを書式化して文字列に変換
		/// </summary>
		public abstract string Format(ResSetCollection resCollection);
	}
}
