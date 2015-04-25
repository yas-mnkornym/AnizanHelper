// ThreadHeaderComparer.cs

namespace Twin
{
	using System;
	using System.Collections;
	using System.Windows.Forms;

	/// <summary>
	/// ThreadHeaderComparer の概要の説明です。
	/// </summary>
	public class ThreadHeaderComparer : IComparer
	{
		public ThreadHeaderComparer()
		{
			// 
			// TODO: コンストラクタ ロジックをここに追加してください。
			//
		}

		public int Compare(object x, object y)
		{
			if (x.Equals(y)) return 0;
			return (x.GetHashCode() > y.GetHashCode()) ? 1 : -1;
		}
	}
}
