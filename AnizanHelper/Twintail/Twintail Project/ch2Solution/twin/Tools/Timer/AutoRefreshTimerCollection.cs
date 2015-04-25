// AutoRefreshTimerCollection.cs

namespace Twin.Tools
{
	using System;
	using System.Timers;
	using System.Collections;

	/// <summary>
	/// ThreadControlをタイマーで自動更新する機能を持つ。
	/// AutoRefreshTimerSimpleとは違い、スレの早さによって更新間隔を増減することで無駄な更新を避ける。
	/// </summary>
	public class AutoRefreshTimerCollection : AutoRefreshTimerBase
	{
		private ArrayList timerCollection;
		private TimerObject current;
		private bool running;
		private int interval;
		private int index;

		/// <summary>
		/// 更新間隔をミリ秒単位で取得または設定。
		/// </summary>
		public override int Interval {
			set { interval = Math.Max(5000, value); }
			get { return interval; }
		}

		#region InnerClass
		/// <summary>
		/// スレッドを更新するタイマーを管理
		/// </summary>
		internal class TimerObject : IDisposable
		{
			private int defInterval;
			private Timer timer;
			private ThreadControl thread;
			private bool disposed = false;

			/// <summary>
			/// このスレッドの更新間隔を取得または設定
			/// </summary>
			public int Interval {
				set { timer.Interval = Math.Max(defInterval, value); }
				get { return (int)timer.Interval; }
			}

			/// <summary>
			/// タイマーが有効かどうかを判断
			/// </summary>
			public bool Enabled {
				get { return timer.Enabled; }
			}

			/// <summary>
			/// スレッドコントロールを取得
			/// </summary>
			public ThreadControl Thread {
				get { return thread; }
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
				timer.Elapsed += elapsed;
				defInterval = interval;
				thread = control;
			}

			/// <summary>
			/// タイマーを開始
			/// </summary>
			public void Start()
			{
				timer.Start();
			}

			/// <summary>
			/// タイマーを停止
			/// </summary>
			public void Stop()
			{
				timer.Stop();
			}

			/// <summary>
			/// 使用しているリソースを解放
			/// </summary>
			public void Dispose()
			{
				if (!disposed)
				{
					timer.Stop();
					timer.Dispose();
					GC.SuppressFinalize(this);
				}
				disposed = true;
			}
		}
		#endregion

		/// <summary>
		/// AutoRefreshTimerCollectionクラスのインスタンスを初期化
		/// </summary>
		public AutoRefreshTimerCollection()
		{
			timerCollection = ArrayList.Synchronized(new ArrayList());
			running = false;
			current = null;
			interval = 10000;
			index = 0;
		}

		/// <summary>
		/// 次のタイマーの位置へ進める
		/// </summary>
		private void Increment()
		{
			if (timerCollection.Count > 0)
			{
				if (index >= timerCollection.Count)
					index = 0;

				current = (TimerObject)timerCollection[index++];
				current.Start();
				running = true;
			}
			else {
				running = false;
			}
		}

		public override int GetInterval(ThreadControl client)
		{
			throw new Exception("The method or operation is not implemented.");
		}
		public override int GetTimeleft(ThreadControl client)
		{
			throw new Exception("The method or operation is not implemented.");
		}
		public override ITimerObject GetTimerObject(ThreadControl client)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		/// <summary>
		/// リストにクライアントを追加。
		/// 既に同じクライアントが登録されていれば何もしない。
		/// </summary>
		/// <param name="client">自動更新の対象とするクライアント</param>
		public override void Add(ThreadControl client)
		{
			if (client == null) {
				throw new ArgumentNullException("client");
			}

			if (IndexOf(client) == -1)
			{
				// イベントを登録
				client.Complete += new CompleteEventHandler(OnComplete);

				timerCollection.Add(new TimerObject(client, Interval,
					new ElapsedEventHandler(OnTimer)));
			}

			if (!running)
				Increment();
		}

		/// <summary>
		/// リストからクライアントを削除。
		/// 指定したクライアントがリストに存在しなければ何もしない。
		/// </summary>
		/// <param name="client">自動更新の対象から外すクライアント</param>
		public override void Remove(ThreadControl client)
		{
			if (client  == null) {
				throw new ArgumentNullException("client");
			}

			int index = IndexOf(client);

			if (index != -1)
			{
				// イベントを削除
				client.Complete -= new CompleteEventHandler(OnComplete);

				TimerObject timer = (TimerObject)timerCollection[index];
				timerCollection.Remove(timer);

				if (timer.Enabled)
				{
					timer.Stop();
					Increment();
				}

				timer.Dispose();
			}

			if (timerCollection.Count == 0)
				running = false;
		}

		/// <summary>
		/// 指定したクライアントがリスト内に存在するかどうかを判断
		/// </summary>
		/// <param name="client">検索するクライアント</param>
		/// <returns>リスト内に存在すればtrue、存在しなければfalseを返す</returns>
		public override bool Contains(ThreadControl client)
		{
			if (client  == null) {
				throw new ArgumentNullException("client");
			}
			return IndexOf(client) != -1;
		}

		/// <summary>
		/// すべてのタイマーを削除
		/// </summary>
		public override void Clear()
		{
			if (current != null)
			{
				current.Dispose();
				current = null;

				timerCollection.Clear();
			}
		}

		/// <summary>
		/// 指定したクライアントのコレクション内の位置を取得
		/// </summary>
		/// <param name="client"></param>
		/// <returns></returns>
		private int IndexOf(ThreadControl client)
		{
			foreach (TimerObject obj in timerCollection)
				if (obj.Thread == client)
					return timerCollection.IndexOf(obj);

			return -1;
		}

		/// <summary>
		/// 更新イベント発生
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnTimer(object sender, ElapsedEventArgs e)
		{
//			Timer timer = (Timer)sender;
//			timer.Stop();

			try {
				current.Stop();
				ThreadControl thread = current.Thread;

				if (thread.IsOpen)
					thread.Reload();
			}
			catch (Exception ex) {
				TwinDll.Output(ex);
			}
		}

		/// <summary>
		/// スレッド更新完了イベント発生
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnComplete(object sender, CompleteEventArgs e)
		{
			if (current == null)
				return;

			if (e.Status == CompleteStatus.Success)
			{
				// 同じインスタンスかどうかをチェック
				if (current.Thread.Equals(sender))
				{
					ThreadControl thread = current.Thread;

					// 新着があったかどうか
					bool notmodified = (thread.HeaderInfo.NewResCount == 0);

					// 新着があれば間隔を縮める、なければ間隔をのばす
					current.Interval += notmodified ? Interval : -Interval;

					// 最大レス数を越えていて新着がなければタイマーから外す
					if (thread.HeaderInfo.IsLimitOverThread && notmodified)
						Remove(current.Thread);

					current = null;

					// 次のスレッドへ
					Increment();
				}
			}
		}
	}
}
