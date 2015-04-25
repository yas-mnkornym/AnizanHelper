// AutoRefreshTimerBase.cs

namespace Twin.Tools
{
	using System;

	/// <summary>
	/// ThreadControlをタイマーで定期的に更新する基本クラス
	/// </summary>
	public abstract class AutoRefreshTimerBase
	{
		/// <summary>
		/// 更新間隔をミリ秒単位で取得または設定。
		/// </summary>
		public abstract int Interval {
			set;
			get;
		}

		/// <summary>
		/// AutoRefreshTimerBaseクラスのインスタンスを初期化
		/// </summary>
		protected AutoRefreshTimerBase()
		{}

		/// <summary>
		/// リストにクライアントを追加。
		/// 既に同じクライアントが登録されていれば何もしない。
		/// </summary>
		/// <param name="client">自動更新の対象とするクライアント</param>
		public abstract void Add(ThreadControl client);

		/// <summary>
		/// リストからクライアントを削除。
		/// 指定したクライアントがリストに存在しなければ何もしない。
		/// </summary>
		/// <param name="client">自動更新の対象から外すクライアント</param>
		public abstract void Remove(ThreadControl client);

		/// <summary>
		/// 指定したクライアントがリスト内に存在するかどうかを判断
		/// </summary>
		/// <param name="client">検索するクライアント</param>
		/// <returns>リスト内に存在すればtrue、存在しなければfalseを返す</returns>
		public abstract bool Contains(ThreadControl client);

		/// <summary>
		/// 指定したクライアントの更新間隔を取得。
		/// </summary>
		/// <param name="client"></param>
		/// <returns>次の更新までの秒数。client がタイマーに登録されていない、
		/// またはタイマーが停止している場合は -1 を返す。</returns>
		public abstract int GetInterval(ThreadControl client);

		/// <summary>
		/// 指定したクライアントの次の更新までの残り秒数を返す。
		/// </summary>
		/// <param name="client"></param>
		/// <returns>次の更新までの秒数。client がタイマーに登録されていない、
		/// またはタイマーが停止している場合は -1 を返す。</returns>
		public abstract int GetTimeleft(ThreadControl client);

		public abstract ITimerObject GetTimerObject(ThreadControl client);

		/// <summary>
		/// すべてのタイマーを削除
		/// </summary>
		public abstract void Clear();
	}
}
