// IPatrolable.cs

namespace Twin.Tools
{
	using System;
	using System.Net;
	using System.Collections.Generic;

	/// <summary>
	/// 巡回するための基本クラスを表す
	/// </summary>
	public abstract class PatrolBase
	{
		/// <summary>
		/// 非同期巡回用のデリゲート
		/// </summary>
		protected delegate void PatrolInvoker();

		private List<ThreadHeader> itemColleciton;

		private PatrolInvoker method;
		private Cache cache;

		/// <summary>
		/// キャッシュ情報を取得
		/// </summary>
		protected Cache Cache {
			get { return cache; }
		}

		/// <summary>
		/// 巡回対象のスレッドコレクションを取得
		/// </summary>
		public List<ThreadHeader> Items {
			get { return itemColleciton; }
		}

		private bool patrolling;
		public bool IsPatrolling
		{
			get
			{
				return patrolling;
			}
		}
	

		/// <summary>
		/// 状態が更新されたときに発生
		/// </summary>
		public event StatusTextEventHandler StatusTextChanged;

		/// <summary>
		/// アイテムを巡回中に発生
		/// </summary>
		public event PatrolEventHandler Patroling;

		/// <summary>
		/// 更新されたアイテムがあったときに発生
		/// </summary>
		public event PatrolEventHandler Updated;

		/// <summary>
		/// PatrolBaseクラスのインスタンスを初期化
		/// </summary>
		/// <param name="cacheInfo"></param>
		protected PatrolBase(Cache cacheInfo)
		{
			itemColleciton = new List<ThreadHeader>();
			cache = cacheInfo;
			method = null;
			patrolling = false;
		}

		/// <summary>
		/// 巡回対象のスレッドアイテムを設定
		/// </summary>
		/// <param name="items"></param>
		public void SetItems(List<ThreadHeader> items)
		{
			if (items == null) {
				throw new ArgumentNullException("items");
			}
			itemColleciton.Clear();
			itemColleciton.AddRange(items);

			// スレッド情報を最新の状態にする
			for (int i = 0; i < itemColleciton.Count; i++)
				ThreadIndexer.Read(cache, itemColleciton[i]);
		}

		/// <summary>
		/// 巡回開始
		/// </summary>
		public abstract void Patrol();

		/// <summary>
		/// 非同期で巡回する
		/// </summary>
		public IAsyncResult BeginPatrol(AsyncCallback callback, object stateObject)
		{
			method = new PatrolInvoker(Patrol);
			patrolling = true;
			return method.BeginInvoke(callback, stateObject);
		}

		/// <summary>
		/// 非同期な巡回を終了するまでブロック
		/// </summary>
		public void EndPatrol(IAsyncResult ar)
		{
			if (ar == null) {
				throw new ArgumentNullException("ar");
			}
			if (method == null) {
				throw new InvalidOperationException("非同期で巡回が開始されていません");
			}

			method.EndInvoke(ar);
			patrolling = false;
			method = null;
		}

		/// <summary>
		/// StatusTextChangedイベントを発生させる
		/// </summary>
		/// <param name="e"></param>
		protected void OnStatusTextChanged(string text)
		{
			if (StatusTextChanged != null)
				StatusTextChanged(this, new StatusTextEventArgs(text));
		}

		/// <summary>
		/// Patrolingイベントを発生させる
		/// </summary>
		/// <param name="e"></param>
		protected void OnPatroling(PatrolEventArgs e)
		{
			if (Patroling != null)
				Patroling(this, e);
		}

		/// <summary>
		/// Updatedイベントを発生させる
		/// </summary>
		/// <param name="e"></param>
		protected void OnUpdated(PatrolEventArgs e)
		{
			if (Updated != null)
				Updated(this, e);
		}
	}
}
