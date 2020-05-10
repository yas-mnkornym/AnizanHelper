// Connect.cs

namespace Twin.Bbs
{
	using System;
	using System.Collections;
	using System.Threading;
	using System.Diagnostics;
	using System.Runtime.CompilerServices;
	using Twin.IO;

	/// <summary>
	/// 接続制限
	/// </summary>
	public sealed class Connect
	{
		private int queue;
		private int interval;

		/// <summary>
		/// 現在管理している接続数を取得
		/// </summary>
		public int Count {
			get {
				return queue;
			}
		}

		/// <summary>
		/// Connectクラスのインスタンスを初期化
		/// </summary>
		public Connect()
		{
			// 
			// TODO: コンストラクタ ロジックをここに追加してください。
			//
			interval = 1000;
			queue = 0;
		}

		/// <summary>
		/// 新しい接続を追加し、許可されるまで待機
		/// </summary>
		public void Wait(object id)
		{
			// 他のキューが終了するまで待つ
			while (queue > 0)
				Thread.Sleep(interval);

			Interlocked.Increment(ref queue);
		}

		/// <summary>
		/// 現在の接続を解放する
		/// </summary>
		//[MethodImpl(MethodImplOptions.Synchronized)]
		public void Release(object id)
		{
			Interlocked.Decrement(ref queue);

			if (queue < 0)
				queue = 0;
		}
	}
}
