// IThreadListFormatter.cs

namespace Twin.Text
{
	using System;
	using System.Collections.Generic;

	/// <summary>
	/// スレッド一覧の書式化を行う基本抽象クラス
	/// </summary>
	public abstract class ThreadListFormatter
	{
		/// <summary>
		/// 指定したヘッダーを書式化して文字列に変換
		/// </summary>
		public abstract string Format(ThreadHeader header);

		/// <summary>
		/// 指定したヘッダーコレクションを書式化して文字列に変換
		/// </summary>
		public abstract string Format(List<ThreadHeader> items);
	}
}
