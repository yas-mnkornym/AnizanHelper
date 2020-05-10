// BookmarkEntryCollection.cs

namespace Twin
{
	using System;
	using System.Collections;
	using System.Windows.Forms;

	/// <summary>
	/// お気に入りエントリのコレクション
	/// </summary>
	public class BookmarkEntryCollection
	{
		private ArrayList innerList;
		private BookmarkEntry parent;

		/// <summary>
		/// コレクション内の要素数を取得
		/// </summary>
		public int Count {
			get {
				return innerList.Count;
			}
		}

		/// <summary>
		/// 指定したインデックスのお気に入りフォルダを取得
		/// </summary>
		public BookmarkEntry this[int index] {
			get {
				return (BookmarkEntry)innerList[index];
			}
		}

		/// <summary>
		/// BookmarkCollectionクラスのインスタンスを初期化
		/// </summary>
		public BookmarkEntryCollection(BookmarkEntry parent)
		{
			if (parent == null) {
				throw new ArgumentNullException("parent");
			}
			if (parent.IsLeaf) {
				throw new ArgumentException("葉を親にすることは出来ません");
			}
			this.innerList = ArrayList.Synchronized(new ArrayList());
			this.parent = parent;
		}

		/// <summary>
		/// お気に入りエントリを追加
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		public int Add(BookmarkEntry item)
		{
			item.Parent = parent;
			return innerList.Add(item);
		}

		/// <summary>
		/// 複数のお気に入りエントリを追加
		/// </summary>
		/// <param name="items"></param>
		public void AddRange(BookmarkEntryCollection items)
		{
			foreach (BookmarkEntry entry in items)
				Add(entry);
		}

		/// <summary>
		/// 指定したインデックスにお気に入りを挿入
		/// </summary>
		/// <param name="index"></param>
		/// <param name="item"></param>
		public void Insert(int index, BookmarkEntry item)
		{
			item.Parent = parent;
			innerList.Insert(index, item);
		}

		/// <summary>
		/// コレクションから指定したお気に入りを削除
		/// </summary>
		/// <param name="item"></param>
		public void Remove(BookmarkEntry item)
		{
			if (innerList.Contains(item))
			{
				item.Parent = null;
				innerList.Remove(item);
			}
		}

		/// <summary>
		/// 指定したインデックスにある要素を削除
		/// </summary>
		/// <param name="index"></param>
		public void RemoveAt(int index)
		{
			if (index < 0 || index >= innerList.Count)
				throw new ArgumentOutOfRangeException("index");

			BookmarkEntry entry = this[index];
			entry.Parent = null;

			innerList.RemoveAt(index);
		}

		/// <summary>
		/// 指定したエントリがコレクション内に含まれているかどうかを判断
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		public bool Contains(BookmarkEntry item)
		{
			return innerList.Contains(item);
		}

		/// <summary>
		/// 指定したエントリのコレクション内インデックスを取得
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		public int IndexOf(BookmarkEntry item)
		{
			return innerList.IndexOf(item);
		}

		/// <summary>
		/// お気に入りをソート
		/// </summary>
		/// <param name="sorter"></param>
		public void Sort(BookmarkSorter sorter)
		{
			innerList.Sort(sorter);
		}

		/// <summary>
		/// お気に入りをすべて削除
		/// </summary>
		public void Clear()
		{
			foreach (BookmarkEntry entry in innerList)
				entry.Parent = null;

			innerList.Clear();
		}

		/// <summary>
		/// BookmarkEntryCollectionを反復処理するための列挙氏を返す
		/// </summary>
		/// <returns></returns>
		public IEnumerator GetEnumerator()
		{
			return innerList.GetEnumerator();
		}
	}
}
