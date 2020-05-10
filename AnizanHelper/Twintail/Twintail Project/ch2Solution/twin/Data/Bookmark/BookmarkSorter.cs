// BookmarkSorter.cs

namespace Twin
{
	using System;
	using System.Collections;
	using System.Windows.Forms;

	/// <summary>
	/// BookmarkCollectionの要素をソートするクラス
	/// </summary>
	public class BookmarkSorter : IComparer
	{
		private BookmarkSortObject obj;
		private SortOrder order;

		/// <summary>
		/// BookmarkSorterクラスのインスタンスを初期化
		/// </summary>
		/// <param name="obj"></param>
		/// <param name="order"></param>
		public BookmarkSorter(BookmarkSortObject obj, SortOrder order)
		{
			// 
			// TODO: コンストラクタ ロジックをここに追加してください。
			//
			this.obj = obj;
			this.order = order;
		}

		/// <summary>
		/// xとyを比較
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <returns></returns>
		public int Compare(object x, object y)
		{
			BookmarkEntry item_x = (BookmarkEntry)x;
			BookmarkEntry item_y = (BookmarkEntry)y;

			switch (obj)
			{
				// 名前順でソート
			case BookmarkSortObject.Name:
				return CompareInternal(item_x, item_y);
			}

			throw new Exception();
		}

		/// <summary>
		/// xとyを比較
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <returns></returns>
		private int CompareInternal(BookmarkEntry entry1, BookmarkEntry entry2)
		{
			// フォルダ対お気に入り、またはお気に入り対フォルダの場合は、
			// フォルダを優先する。
			if (entry1 is BookmarkFolder && entry2 is BookmarkThread)
				return -1;

			if (entry1 is BookmarkThread && entry2 is BookmarkFolder)
				return 1;
			// ---------------------------------------

			// お気に入り対お気に入り
			return (order == SortOrder.Ascending) ?
				String.Compare(entry1.Name, entry2.Name) :
				String.Compare(entry2.Name, entry1.Name);
		}
	}
}
