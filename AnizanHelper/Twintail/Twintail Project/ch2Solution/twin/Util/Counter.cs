// Counter.cs

namespace Twin.Util
{
	using System;
	using System.Diagnostics;
	using System.Windows.Forms;

	/// <summary>
	/// 簡単なカウンタークラス
	/// </summary>
	public class Counter
	{
		private static readonly string[] names = new string[32];
		private static readonly int[] ticks = new int[32];
		private static int position = 0;

		/// <summary>
		/// カウントを開始
		/// </summary>
		public static void Start(string name)
		{
			ticks[position] = Environment.TickCount;
			names[position] = name;
			position++;
		}

		/// <summary>
		/// カウント中止
		/// </summary>
		public static void Stop()
		{
			Stop(false);
		}

		/// <summary>
		/// カウント中止
		/// </summary>
		public static void Stop(bool msgBox)
		{
			position--;

			int count = Environment.TickCount - ticks[position];
			Trace.WriteLine(String.Format("{0}\t{1}ms", names[position], count));

			if (msgBox)
				MessageBox.Show(count.ToString() + "ms");
		}
	}
}
