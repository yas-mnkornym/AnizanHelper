// CacheRemoveUtility.cs
// #2.0

namespace Twin.Tools
{
	using System;
	using System.IO;
	using System.Collections.Generic;
	using System.Windows.Forms;

	/// <summary>
	/// キャッシュを削除するユーティリティ
	/// </summary>
	public class CacheRemoveUtility
	{
		private Cache cache;

		/// <summary>
		/// CacheRemoveUtilityクラスのインスタンスを初期化
		/// </summary>
		/// <param name="cache"></param>
		public CacheRemoveUtility(Cache cache)
		{
			this.cache = cache;
		}

		/// <summary>
		/// お気に入り以外の既得スレッドを削除します。
		/// </summary>
		/// <param name="bookmarkRoot"></param>
		public void RemoveWithoutBookmarks(IBoardTable table, BookmarkRoot bookmarkRoot)
		{
			if (bookmarkRoot == null) {
				throw new ArgumentNullException("bookmarkRoot");
			}

			// すべての板の既得インデックスを読み込む
			List<ThreadHeader> targets = Twin.IO.GotThreadListReader.GetAllThreads(cache, table);
			int count1 = targets.Count;

			// 読み込まれたインデックスで、お気に入りに登録されている物は除外
			for (int i = 0; i < targets.Count;)
			{
				ThreadHeader h = targets[i];
				BookmarkThread match = bookmarkRoot.Search(h);

				if (match != null)
					targets.RemoveAt(i);

				else i++;
			}

			int count2 = count1 - targets.Count;

			foreach (ThreadHeader h in targets)
				cache.Remove(h);

			cache.ClearEmptyFolders();

#if DEBUG
			int count3 = Twin.IO.GotThreadListReader.GetAllThreads(cache, table).Count;
			MessageBox.Show(String.Format("削除前={0}, お気に入りの数={1}, 削除後のログ数={2}", count1, count2, count3));
#endif
		}
	}
}
