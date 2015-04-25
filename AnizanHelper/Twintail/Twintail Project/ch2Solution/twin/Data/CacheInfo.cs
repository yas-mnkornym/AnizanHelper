// CacheInfo.cs

namespace Twin
{
	using System;
	using System.IO;

	/// <summary>
	/// 板のキャッシュ情報を表す
	/// </summary>
	public class CacheInfo
	{
		private string folderPath;
		private long totalSize;
		private int totalCount;

		/// <summary>
		/// キャッシュフォルダへのパスを取得
		/// </summary>
		public string FolderPath {
			get { return folderPath; }
		}

		/// <summary>
		/// キャッシュの合計サイズを取得
		/// </summary>
		public long TotalSize {
			get { return totalSize; }
		}

		/// <summary>
		/// キャッシュの合計数を取得
		/// </summary>
		public int TotalCount {
			get { return totalCount; }
		}

		/// <summary>
		/// CacheInfoクラスのインスタンスを初期化
		/// </summary>
		/// <param name="cache"></param>
		/// <param name="board"></param>
		public CacheInfo(Cache cache, BoardInfo board)
		{
			// 
			// TODO: コンストラクタ ロジックをここに追加してください。
			//
			if (cache == null) {
				throw new ArgumentNullException("cache");
			}
			if (board == null) {
				throw new ArgumentNullException("board");
			}

			// 板へのパスを取得
			folderPath = cache.GetFolderPath(board);

			DirectoryInfo dir = new DirectoryInfo(folderPath);
			FileInfo[] fileInfoArray = dir.GetFiles("*.idx");

			// 合計数を取得
			totalCount = fileInfoArray.Length;
			totalSize = 0;

			// 合計サイズを取得
			foreach (FileInfo info in fileInfoArray)
				totalSize += info.Length;
		}
	}
}
