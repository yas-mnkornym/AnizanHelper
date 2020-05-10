// AaItemCollection.cs

namespace Twin.Aa
{
	using System;
	using System.Collections;

	/// <summary>
	/// AaItemをコレクション管理
	/// </summary>
	public class AaItemCollection : CollectionBase
	{
		/// <summary>
		/// 指定したindex位置のアイテムを取得または設定
		/// </summary>
		public AaItem this[int index] {
			set {
				List[index] = value;
			}
			get { return (AaItem)List[index]; }
		}

		/// <summary>
		/// アイテムが追加されたときに発生
		/// </summary>
		internal event AaItemSetEventHandler ItemSet;

		/// <summary>
		/// AaItemCollectionクラスのインスタンスを初期化
		/// </summary>
		public AaItemCollection()
		{
			// 
			// TODO: コンストラクタ ロジックをここに追加してください。
			//
			InnerList.Capacity = 50;
		}

		/// <summary>
		/// itemをコレクションに追加
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		public int Add(AaItem item)
		{
			OnSetItemEvent(this, new AaItemSetEventArgs(item));
			return List.Add(item);
		}

		/// <summary>
		/// itemsをコレクションに追加
		/// </summary>
		/// <param name="items"></param>
		public void AddRange(AaItemCollection items)
		{
			foreach (AaItem item in items)
				Add(item);
		}

		/// <summary>
		/// itemsをコレクションに追加
		/// </summary>
		/// <param name="items"></param>
		public void AddRange(AaItem[] items)
		{
			foreach (AaItem item in items)
				Add(item);
			//InnerList.AddRange(items);
		}

		/// <summary>
		/// コレクションの指定したindexにitemを挿入
		/// </summary>
		/// <param name="index"></param>
		/// <param name="item"></param>
		public void Insert(int index, AaItem item)
		{
			List.Insert(index, item);
			OnSetItemEvent(this, new AaItemSetEventArgs(item));
		}

		/// <summary>
		/// itemをコレクションから削除
		/// </summary>
		/// <param name="item"></param>
		public void Remove(AaItem item)
		{
			List.Remove(item);
			item.parent = null;
		}

		/// <summary>
		/// itemを検索しインデックス値を取得
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		public int IndexOf(AaItem item)
		{
			return List.IndexOf(item);
		}

		/// <summary>
		/// コレクション内のAaItemをソート
		/// </summary>
		public void Sort()
		{
			InnerList.Sort(new AaComparer.AaItemComparer());
		}

//		なんか呼ばれない…
		protected override void OnSetComplete(int index, object oldValue, object newValue)
		{
			base.OnSetComplete(index, oldValue, newValue);
			OnSetItemEvent(this, new AaItemSetEventArgs((AaItem)newValue));
		}

		private void OnSetItemEvent(object sender, AaItemSetEventArgs e)
		{
			if (ItemSet != null)
				ItemSet(sender, e);
		}
	}
}
