// AutoRefreshTimerCollection2.cs
// #2.0

namespace Twin.Tools
{
	using System;
	using System.Timers;
	using System.Collections.Generic;

	/// <summary>
	/// 各スレッドごとに状態を保持してタイマーの間隔を調整する機能を持つ。
	/// ちょっと変更。
	/// </summary>
	public class AutoRefreshTimerCollection2 : AutoRefreshTimerBase
	{
		private List<TimerObject> timerList = new List<TimerObject>();
		private int interval = 30000; // 30秒

		public const int MinInterval = 10000;
		public const int MaxInterval = 60000 * 15;

		/// <summary>
		/// 更新間隔をミリ秒単位で取得または設定します。最小は 10 秒です。
		/// </summary>
		public override int Interval
		{
			set
			{
				if (MinInterval > value) value = MinInterval;
				if (MaxInterval < value) value = MaxInterval;

				interval = value;
			}
			get
			{
				return interval;
			}
		}

		#region InnerClass
		/// <summary>
		/// スレッドを更新するタイマーを管理
		/// </summary>
		public class TimerObject : IDisposable, ITimerObject
		{
			private const int DefaultRetryCount = 12;

			private int defInterval;
			private Timer timer;
			private ThreadControl thread;
			private bool disposed = false;

			private int retryCount = DefaultRetryCount;
			private bool timerEnabled = true;

			/// <summary>
			/// 更新間隔が最大値になった場合、またはタイムアウトで更新できなかった場合の
			/// 再試行回数を取得または設定します。
			/// </summary>
			public int RetryCount
			{
				set {
					if (value < 0)
						value = 0;

					retryCount = value;
				}
				get {
					return retryCount;
				}
			}

			/// <summary>
			/// このスレッドの更新間隔を取得または設定します。
			/// </summary>
			public int Interval
			{
				set
				{
					timer.Interval = Math.Max(defInterval, value);
				}
				get
				{
					return (int)timer.Interval;
				}
			}

			/// <summary>
			/// タイマーが有効かどうかを示す値を取得または設定します。
			/// </summary>
			public bool Enabled
			{
				set
				{
					timerEnabled = value;

					if (timerEnabled == false)
					{
						Stop();
					}
				}
				get
				{
					return timerEnabled;
				}
			}

			/// <summary>
			/// このタイマーが管理している ThreadControl を取得します。
			/// </summary>
			public ThreadControl Thread
			{
				get
				{
					return thread;
				}
			}

			private int startTick = -1;
			/// <summary>
			/// 次の更新までの残り秒数を取得します。
			/// </summary>
			public int Timeleft
			{
				get
				{
					if (startTick == -1)
						return -1;

					return (Interval - (Environment.TickCount - startTick)) / 1000;
				}
			}

			public event ElapsedEventHandler Elapsed;

			private void OnTimerInternal(object sender, ElapsedEventArgs e)
			{
				if (Elapsed != null)
					Elapsed(this, e);
			}

			/// <summary>
			/// TimerObjectクラスのインスタンスを初期化
			/// </summary>
			/// <param name="control">更新対象のスレッドコントロール</param>
			/// <param name="interval">更新間隔の初期値 (ミリ秒)</param>
			/// <param name="elapsed">時間経過時のイベントハンドラ</param>
			public TimerObject(ThreadControl control, int interval, ElapsedEventHandler elapsed)
			{
				timer = new Timer();
				timer.Interval = interval;
				timer.Elapsed += new ElapsedEventHandler(OnTimerInternal);
				defInterval = interval;
				thread = control;

				Elapsed += elapsed;
			}

			public void ResetRetryCount()
			{
				retryCount = DefaultRetryCount;
			}

			public void Start()
			{
				if (!disposed && timerEnabled)
				{
					timer.Start();
					startTick = Environment.TickCount;
				}
			}

			/// <summary>
			/// タイマーの状態をリセットして開始する
			/// </summary>
			public void ResetStart()
			{
				timerEnabled = true;
				timer.Interval = defInterval;
				Start();

			}

			public void Stop()
			{
				if (!disposed)
					timer.Stop();

				startTick = -1;
			}

			public void Dispose()
			{
				if (!disposed)
				{
					timer.Stop();
					timer.Dispose();
					startTick = -1;
				}
				timerEnabled = false;
				disposed = true;
			}
		}
		#endregion

		/// <summary>
		/// AutoRefreshTimerCollection2クラスのインスタンスを初期化
		/// </summary>
		public AutoRefreshTimerCollection2()
		{
		}

		public override ITimerObject GetTimerObject(ThreadControl client)
		{
			int i = IndexOf(client);

			return i == -1 ? null : timerList[i];
		}

		public override int GetInterval(ThreadControl client)
		{
			int index = IndexOf(client);

			if (index == -1)
				return -1;

			lock (timerList)
			{
				TimerObject obj = timerList[index];
				return obj.Interval;
			}
		}

		public override int GetTimeleft(ThreadControl client)
		{
			int index = IndexOf(client);

			if (index == -1)
				return -1;

			lock (timerList)
			{
				TimerObject obj = timerList[index];
				return obj.Timeleft;
			}
		}

		/// <summary>
		/// リストにクライアントを追加。
		/// 既に同じクライアントが登録されていれば何もしない。
		/// </summary>
		/// <param name="client">自動更新の対象とするクライアント</param>
		public override void Add(ThreadControl client)
		{
			if (client == null)
			{
				throw new ArgumentNullException("client");
			}

			if (IndexOf(client) == -1)
			{
				client.Complete += new CompleteEventHandler(OnComplete);

				TimerObject timer =
					new TimerObject(client, Interval, new ElapsedEventHandler(OnTimer));

				lock (timerList)
				{
					timerList.Add(timer);
				}

				timer.Start();
			}
		}

		/// <summary>
		/// リストからクライアントを削除。
		/// 指定したクライアントがリストに存在しなければ何もしない。
		/// </summary>
		/// <param name="client">自動更新の対象から外すクライアント</param>
		public override void Remove(ThreadControl client)
		{
			if (client == null)
			{
				throw new ArgumentNullException("client");
			}

			int index = IndexOf(client);

			if (index != -1)
			{
				// イベントを削除
				client.Complete -= new CompleteEventHandler(OnComplete);
				
				lock (timerList)
				{
					TimerObject timer = timerList[index];
					timer.Dispose();

					timerList.Remove(timer);
				}

			}
		}

		/// <summary>
		/// 指定したクライアントがリスト内に存在するかどうかを判断
		/// </summary>
		/// <param name="client">検索するクライアント</param>
		/// <returns>リスト内に存在すればtrue、存在しなければfalseを返す</returns>
		public override bool Contains(ThreadControl client)
		{
			if (client == null)
			{
				throw new ArgumentNullException("client");
			}
			return IndexOf(client) != -1;
		}

		/// <summary>
		/// すべてのタイマーを削除
		/// </summary>
		public override void Clear()
		{
			lock (timerList)
			{
				foreach (TimerObject timer in timerList)
					timer.Dispose();

				timerList.Clear();
			}
		}

		/// <summary>
		/// 指定したクライアントのコレクション内の位置を取得
		/// </summary>
		/// <param name="client"></param>
		/// <returns></returns>
		private int IndexOf(ThreadControl client)
		{
			lock (timerList)
			{
				foreach (TimerObject obj in timerList)
				{
					if (obj.Thread == client)
						return timerList.IndexOf(obj);
				}
			}
			return -1;
		}

		/// <summary>
		/// 更新イベント発生
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnTimer(object sender, ElapsedEventArgs e)
		{
			TimerObject timer = (TimerObject)sender;

			timer.Stop();
			ThreadControl thread = timer.Thread;

			if (thread.IsOpen)
				thread.Reload();
		}

		/// <summary>
		/// スレッド更新完了イベント発生
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnComplete(object sender, CompleteEventArgs e)
		{
			ThreadControl thread = (ThreadControl)sender;
			TimerObject timer = null;

			int index = IndexOf(thread);

			if (index >= 0)
			{
				lock (timerList)
				{
					timer = timerList[index];
				}

				if (e.Status == CompleteStatus.Success)
				{
					bool modified = (thread.HeaderInfo.NewResCount > 0);

					// 新着が無く、最大レス数を越えていた場合、
					// または再試行回数が 0 回になっていればタイマーから外す
					if (thread.HeaderInfo.IsLimitOverThread && !modified)
					{
						Remove(thread);
					}
					else if (timer.RetryCount == 0 && !modified)
					{
						Remove(thread);
					}
					else {
						// 新着があれば間隔を縮める、なければ間隔をのばす
						timer.Interval = modified ? (int)(timer.Interval / 1.5) : (int)(timer.Interval * 1.5);

						// 新着があった場合は再試行回数をリセット。
						if (modified)
						{
							timer.ResetRetryCount();
							timer.Interval = interval;
						}
						// 最大 30分 を越えた場合は再試行回数を減らす
						else if (timer.Interval > MaxInterval)
						{
							timer.Interval = MaxInterval;
							timer.RetryCount--;
						}

						timer.Start();
					}
				}
				else {
					// 更新に失敗した場合は、３回だけ再試行してみる
					if (timer.RetryCount > 0)
					{
						timer.RetryCount--;
						timer.Start();
					}
					// それでもダメなら間隔を最大に延ばしてさらに３回だけ再試行。
					else if (timer.Interval < MaxInterval)
					{
						timer.ResetRetryCount();
						timer.Interval = MaxInterval;
						timer.Start();
					}
					// それでもダメならオートリロード停止
					else
					{
						Remove(thread);
					}
				}
			}
		}
	}
}
