// Range.cs

namespace Twin
{
	using System;

	/// <summary>
	/// Range の概要の説明です。
	/// </summary>
	public struct Range
	{
		public int Start;
		public int End;

		/// <summary>
		/// Rangeクラスのインスタンスを初期化
		/// </summary>
		/// <param name="start"></param>
		/// <param name="end"></param>
		public Range(int start, int end)
		{
			// 
			// TODO: コンストラクタ ロジックをここに追加してください。
			//
			Start = start;
			End = end;
		}
	}
}
