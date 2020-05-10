// AaHeaderCollection.cs

namespace Twin.Aa
{
	using System;
	using System.Collections;

	/// <summary>
	/// AaHeaderをコレクション管理
	/// </summary>
	public class AaHeaderCollection : CollectionBase
	{
		/// <summary>
		/// 指定したindex位置のアイテムを取得または設定
		/// </summary>
		public AaHeader this[int index] {
			set {
				List[index] = value;
			}
			get { return (AaHeader)List[index]; }
		}

		/// <summary>
		/// AaHeaderCollectionクラスのインスタンスを初期化
		/// </summary>
		public AaHeaderCollection()
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
		public int Add(AaHeader item)
		{
			return List.Add(item);
		}

		/// <summary>
		/// itemsをコレクションに追加
		/// </summary>
		/// <param name="items"></param>
		public void AddRange(AaHeaderCollection items)
		{
			foreach (AaHeader item in items)
				List.Add(item);
		}

		/// <summary>
		/// itemsをコレクションに追加
		/// </summary>
		/// <param name="items"></param>
		public void AddRange(AaHeader[] items)
		{
			InnerList.AddRange(items);
		}

		/// <summary>
		/// コレクションの指定したindexにitemを挿入
		/// </summary>
		/// <param name="index"></param>
		/// <param name="item"></param>
		public void Insert(int index, AaHeader item)
		{
			List.Insert(index, item);
		}

		/// <summary>
		/// itemをコレクションから削除
		/// </summary>
		/// <param name="item"></param>
		public void Remove(AaHeader item)
		{
			List.Remove(item);
		}

		/// <summary>
		/// コレクション内のAaItemをソート
		/// </summary>
		public void Sort()
		{
			InnerList.Sort(new AaComparer.AaHeaderComparer());
		}
	}
}
