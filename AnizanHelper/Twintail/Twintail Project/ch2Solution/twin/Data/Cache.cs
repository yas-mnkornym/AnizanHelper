// Cache.cs

namespace Twin
{
	using System;
	using System.Diagnostics;
	using System.IO;
	using Twin.IO;

	/// <summary>
	/// ログのキャッシュ情報を管理
	/// </summary>
	public class Cache
	{
		private string baseDirectory;

		/// <summary>
		/// 基本となるディレクトリを取得または設定
		/// </summary>
		public string BaseDirectory {
			set {
				if (!Directory.Exists(value))
					Directory.CreateDirectory(value);

				baseDirectory = value;
			}
			get { return baseDirectory; }
		}

		private bool newStruct = false;

		public bool NewStructMode
		{
			get
			{
				return newStruct;
			}
			set
			{
				newStruct = value;
			}
		}
	
		/// <summary>
		/// Cacheクラスのインスタンスを初期化
		/// </summary>
		/// <param name="baseDir">基本となるディレクトリを指定</param>
		public Cache(string baseDir)
		{
			if (baseDir == null) {
				throw new ArgumentNullException("baseDir");
			}
			baseDirectory = baseDir;
		}

		/// <summary>
		/// すべてのログを削除
		/// </summary>
		public virtual void Clear()
		{
			string[] allFolders = 
				Directory.GetDirectories(baseDirectory);

			foreach (string folder in allFolders)
				Directory.Delete(folder, true);
		}

		/// <summary>
		/// 空のフォルダを削除
		/// </summary>
		public virtual void ClearEmptyFolders()
		{
			string[] subdirs = Directory.GetDirectories(baseDirectory);
			foreach (string sub in subdirs)
				ClearEmptyFolders(sub);
		}

		/// <summary>
		/// 指定したディレクトリ内の空フォルダを削除
		/// </summary>
		/// <param name="directory"></param>
		private void ClearEmptyFolders(string directory)
		{
			// 再起を利用してサブフォルダも検索
			string[] subdirs = Directory.GetDirectories(directory);
			foreach (string sub in subdirs)
				ClearEmptyFolders(sub);

			// idxファイルとサブディレクトリが１つもなければ
			// フォルダを削除
			string[] indices = Directory.GetFiles(directory, "*.idx");
			subdirs = Directory.GetDirectories(directory);

			if (indices.Length == 0 && subdirs.Length == 0)
				Directory.Delete(directory, true);
		}

		/// <summary>
		/// 指定した板のログと書込履歴を削除
		/// </summary>
		/// <param name="board"></param>
		/// <returns></returns>
		public virtual void Remove(BoardInfo board)
		{
			if (board != null)
			{
				try {
					string folder = GetFolderPath(board);
					Directory.Delete(folder, true);
				}
				catch (Exception ex) {
					TwinDll.Output(ex);
				}
			}
		}

		/// <summary>
		/// 指定したスレッドのログを削除
		/// </summary>
		/// <param name="header"></param>
		public virtual bool Remove(ThreadHeader header)
		{
			if (header != null)
			{
				// 既得インデックス一覧から削除
				GotThreadListIndexer.Remove(this, header);

				// 書き込み履歴インデックス一覧から削除
//				WroteHistoryIndexer.Remove(this, new WroteThreadHeader(header));

				// ログファイルを削除
				string idx = GetIndexPath(header);
				string dat = GetDatPath(header);

				header.GotByteCount = 0;
				header.GotResCount = 0;
				header.NewResCount = 0;
				header.Position = 0;
				header.ETag = String.Empty;
				header.LastModified = new DateTime(0);

				try
				{
					File.Delete(idx);
					File.Delete(dat);
				}
				catch
				{
				}
			}
			return false;
		}

		/// <summary>
		/// レスをあぼーん
		/// </summary>
		/// <param name="header">あぼーんするスレッド情報</param>
		/// <param name="indices">あぼーんするレス番号の配列</param>
		/// <param name="visible">透明あぼーんの場合はfalse、そうでない場合はtrue</param>
		public virtual void ResABone(ThreadHeader header, int[] indices, bool visible)
		{
			if (header == null) {
				throw new ArgumentNullException("header");
			}
			if (indices == null) {
				throw new ArgumentNullException("indices");
			}

			ResSetCollection resSets = new ResSetCollection();
			ThreadStorage storage = null;

			try {
				storage = new LocalThreadStorage(this);
				
				// スレッドを読み込む
				if (storage.Open(header, StorageMode.Read))
				{
					while (storage.Read(resSets) != 0);
					storage.Close();
				}

				// レスの削除
				foreach (int index in indices)
					resSets.ABone(index, visible, visible ? ABoneType.Normal : ABoneType.Tomei, "");

				// ログを一端削除
				string dat = GetDatPath(header);
				File.Delete(dat);

				// 書き込む
				if (storage.Open(header, StorageMode.Write))
				{
					storage.Write(resSets);
					storage.Close();
				}
				storage = null;
			}
			finally {
				if (storage != null)
					storage.Close();
			}
		}

		/// <summary>
		/// 指定した板のフォルダが存在するかどうかを判断
		/// </summary>
		/// <param name="board"></param>
		/// <returns></returns>
		public virtual bool Exists(BoardInfo board)
		{
			return Directory.Exists(GetFolderPath(board, false));
		}

		/// <summary>
		///	指定した板の掲示板のルートディレクトリを取得（BbsType=X2ch: basedir/2ch.net, BbsType=bbspink: basedir/bbspink.com)
		/// </summary>
		/// <param name="bi"></param>
		/// <returns></returns>
		public virtual string GetBbsRootDirectory(BoardInfo bi)
		{
			return Path.Combine(this.BaseDirectory, bi.DomainName);
		}

		/// <summary>
		/// 指定した板のローカルフォルダへのパスを取得
		/// (フォルダが存在しなければ作成)
		/// </summary>
		/// <param name="board"></param>
		/// <returns></returns>
		public virtual string GetFolderPath(BoardInfo board)
		{
			return GetFolderPath(board, true);
		}

		/// <summary>
		/// 指定した板のローカルフォルダへのパスを取得
		/// </summary>
		/// <param name="board"></param>
		/// <param name="create">フォルダを作成する場合はtrue、作成しない場合はfalse</param>
		/// <returns></returns>
		public virtual string GetFolderPath(BoardInfo board, bool create)
		{
			if (board == null) {
				throw new ArgumentNullException("board");
			}

			System.Text.StringBuilder sb = new System.Text.StringBuilder(32);

			if (newStruct)
			{
				sb.Append(board.DomainPath.Replace('/', '\\')); // 2ch\\path
			}
			else
			{
				sb.Append(board.Server);						// hoge.2ch.net\\path 
				sb.Append('\\');
				sb.Append(board.Path);
			}

			string result =
				Path.Combine(baseDirectory, sb.ToString());

			if (create && !Directory.Exists(result))
				Directory.CreateDirectory(result);

			return result;
		}

		/// <summary>
		/// 指定したスレッドのローカル上のパスを取得
		/// </summary>
		/// <param name="header"></param>
		/// <returns></returns>
		public virtual string GetDatPath(ThreadHeader header)
		{
			if (header == null) {
				throw new ArgumentNullException("header");
			}

			string extension = String.Empty;
			string folder = GetFolderPath(header.BoardInfo, false);

			if (header.UseGzip)
				extension = ".gz";

			return Path.Combine(folder, header.Key + ".dat" + extension);
		}

		/// <summary>
		///  指定したスレッドの情報が存在するパスを取得
		/// </summary>
		/// <param name="header"></param>
		/// <returns></returns>
		public virtual string GetIndexPath(ThreadHeader header)
		{
			if (header == null) {
				throw new ArgumentNullException("header");
			}

			string folder = GetFolderPath(header.BoardInfo, false);
			return Path.Combine(folder, header.Key + ".idx");
		}
	}
}
