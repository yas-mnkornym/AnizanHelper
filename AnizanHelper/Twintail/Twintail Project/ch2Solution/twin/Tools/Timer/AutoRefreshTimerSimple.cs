// AutoRefreshTimerSimple.cs

namespace Twin.Tools
{
	using System;
	using System.Timers;
	using System.Collections;

	/// <summary>
	/// ThreadControlをタイマーで定期的に更新する機能を提供。
	/// 特に小細工なしのシンプルな構造。
	/// </summary>
	public class AutoRefreshTimerSimple : AutoRefreshTimerBase
	{
		private ArrayList list;
		private Timer timer;
		private int interval;

		/// <summary>
		/// 更新間隔をミリ秒単位で取得または設定。
		/// 最小値は5000 (5秒)。
		/// </summary>
		public override int Interval {
			set {
				interval = Math.Max(5000, value);
				timer.Interval = interval;
			}
			get { return interval; }
		}

		/// <summary>
		/// AutoRefreshTimerSimpleクラスのインスタンスを初期化
		/// </summary>
		public AutoRefreshTimerSimple()
		{
			// 
			// TODO: コンストラクタ ロジックをここに追加してください。
			//
			list = ArrayList.Synchronized(new ArrayList());
			timer = new Timer();
			timer.Elapsed += new ElapsedEventHandler(OnTimer);

			// 初期値は10秒
			Interval = 10000;
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

			if (list.IndexOf(client) == -1)
			{
				// 完了イベントに登録
				client.Complete += new CompleteEventHandler(OnComplete);
				list.Add(client);
			}

			if (! timer.Enabled)
				timer.Start();
		}

		/// <summary>
		/// リストからクライアントを削除。
		/// 指定したクライアントがリストに存在しなければ何もしない。
		/// </summary>
		/// <param name="client">自動更新の対象から外すクライアント</param>
		public override void Remove(ThreadControl client)
		{
			if (list.Contains(client))
			{
				list.Remove(client);
				client.Complete -= new CompleteEventHandler(OnComplete);
			}

			if (list.Count == 0)
				timer.Stop();
		}

		/// <summary>
		/// 指定したクライアントがリスト内に存在するかどうかを判断
		/// </summary>
		/// <param name="client">検索するクライアント</param>
		/// <returns>リスト内に存在すればtrue、存在しなければfalseを返す</returns>
		public override bool Contains(ThreadControl client)
		{
			return list.Contains(client);
		}

		/// <summary>
		/// すべてのタイマーを削除
		/// </summary>
		public override void Clear()
		{
			timer.Dispose();
			list.Clear();
		}

		/// <summary>
		/// タイマーが発生したらキューから取り出し更新
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnTimer(object sender, ElapsedEventArgs e)
		{
			timer.Stop();

			if (list.Count > 0)
			{
				// 更新対象のアイテムを取得
				ThreadControl thread = (ThreadControl)list[0];

				// スレッドが開かれていて、読み込み中でない場合のみ更新
				if (thread.IsOpen)
				{
					thread.Reload();
				}
				// スレッドが開かれていなければ削除
				else {
					list.Remove(thread);
				}
			}
		}

		/// <summary>
		/// 更新が完了したら再度キューに追加
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnComplete(object sender, CompleteEventArgs e)
		{
			// 読み込み完了したら末尾に追加
			list.Remove(sender);
			list.Add(sender);
			timer.Start();
		}
	}
}
