// ThreadIndexer.cs
// #2.0

namespace Twin
{
	using System;
	using System.IO;
	using System.Text;
	using System.Threading;
	using System.Diagnostics;
	using System.Collections.Generic;
	using CSharpSamples;
	using Twin.IO;

	/// <summary>
	/// スレッドのインデックス情報を管理・作成
	/// </summary>
	public class ThreadIndexer
	{

		public static void SavePastlog(Cache cache, ThreadHeader header, bool newValue)
		{
			SaveValue(cache, header, "Option", "Pastlog", newValue);
		}

		public static void SaveServerInfo(Cache cache, ThreadHeader header)
		{
			SaveValue(cache, header, "Board", "Server", header.BoardInfo.Server);
		}

		public static void SaveSirusi(Cache cache, ThreadHeader header)
		{
			SaveValue(cache, header, "Option", "Sirusi", header.Sirusi.ToArrayString());
		}

		public static void SaveBookmark(Cache cache, ThreadHeader header)
		{
			SaveValue(cache, header, "Option", "Shiori", header.Shiori);
		}

		public static void SavePosition(Cache cache, ThreadHeader header)
		{
			SaveValue(cache, header, "Option", "Position", header.Position);
		}

		public static void SaveLastWritten(Cache cache, ThreadHeader header)
		{
			SaveValue(cache, header, "Thread", "LastWritten", header.LastWritten);
		}

		public static void IncrementRefCount(Cache cache, ThreadHeader header)
		{
			string filePath = cache.GetIndexPath(header);
			int refCount = 0;

			if (File.Exists(filePath))
			{
				refCount = CSPrivateProfile.GetInt("Option", "RefCount", 0, filePath);
			}

			SaveValue(cache, header, "Option", "RefCount", ++refCount);
		}

		private static void SaveValue(Cache cache, ThreadHeader header,
			string section, string key, object value)
		{
			lock (typeof(ThreadIndexer))
			{
				string filePath = cache.GetIndexPath(header);

				if (File.Exists(filePath))
				{
					CSPrivateProfile.SetValue(section, key,
						value, filePath);
				}
			}
		}

		/// <summary>
		/// スレッドの既得情報を記録するインデックスを作成します。
		/// </summary>
		/// <param name="filePath">作成するインデックス情報へのパス</param>
		/// <param name="header">作成するインデックス情報が格納されたThreadHeaderクラス</param>
		/// <returns>作成に成功すればtrue、失敗すればfalseを返す</returns>
		public static bool Write(string filePath, ThreadHeader header)
		{
			if (filePath == null)
			{
				throw new ArgumentNullException("filePath");
			}

			if (header == null)
			{
				throw new ArgumentNullException("header");
			}

			lock (typeof(ThreadIndexer))
			{
				CSPrivateProfile profile = new CSPrivateProfile();

				// 板情報
				profile.SetValue("Board", "Server", header.BoardInfo.Server);
				profile.SetValue("Board", "Path", header.BoardInfo.Path);
				profile.SetValue("Board", "Name", header.BoardInfo.Name);
				profile.SetValue("Board", "BBS", header.BoardInfo.Bbs);

				// スレッド情報
				profile.SetValue("Thread", "ETag", header.ETag);
				profile.SetValue("Thread", "LastModified", header.LastModified);
				profile.SetValue("Thread", "LastWritten", header.LastWritten);
				profile.SetValue("Thread", "Subject", header.Subject);
				profile.SetValue("Thread", "ResCount", header.ResCount);
				profile.SetValue("Thread", "GotResCount", header.GotResCount);
				profile.SetValue("Thread", "GotByteCount", header.GotByteCount);
				profile.SetValue("Thread", "NewResCount", header.NewResCount);
				profile.SetValue("Thread", "Key", header.Key);

				// 拡張情報
				profile.SetValue("Option", "UseGzip", header.UseGzip);
				profile.SetValue("Option", "Pastlog", header.Pastlog);
				profile.SetValue("Option", "Position", header.Position);
				profile.SetValue("Option", "Shiori", header.Shiori);
				profile.SetValue("Option", "RefCount", header.RefCount);
				profile.SetValue("Option", "Sirusi", header.Sirusi.ToArrayString());

				profile.Write(filePath);
			}

			return true;
		}

		/// <summary>
		/// インデックスを作成
		/// </summary>
		/// <param name="cache"></param>
		/// <param name="header">作成するインデックス情報が格納されたThreadHeaderクラス</param>
		/// <returns>作成に成功すればtrue、失敗すればfalseを返す</returns>
		public static bool Write(Cache cache, ThreadHeader header)
		{
			if (cache == null)
			{
				throw new ArgumentNullException("cache");
			}

			string filePath = cache.GetIndexPath(header);

			// ディレクトリが存在しなければ作成
			string dir = Path.GetDirectoryName(filePath);

			if (!Directory.Exists(dir))
			{
				Directory.CreateDirectory(dir);
			}

			return Write(filePath, header);
		}

		/// <summary>
		/// インデックスを読み込む
		/// </summary>
		/// <param name="header">基本的な情報が格納されたThreadHeader</param>
		/// <returns>読み込みに成功すればThreadHeaderのインスタンス、失敗すればnull</returns>
		public static ThreadHeader Read(Cache cache, ThreadHeader header)
		{
			if (cache == null)
			{
				throw new ArgumentNullException("cache");
			}

			if (header == null)
			{
				throw new ArgumentNullException("header");
			}

			// インデックスファイルへのパス
			string filePath = cache.GetIndexPath(header);
			ThreadHeader result = Read(filePath);

			if (result != null)
			{
				// 参照はそのままで値だけをコピーする
				result.Tag = header.Tag;
				result.CopyTo(header);
			}
			else
			{
				header = null;
			}

			return header;
		}

		/// <summary>
		/// インデックスを読み込む
		/// </summary>
		/// <param name="filePath">インデックスファイルへのファイルパス</param>
		/// <returns>読み込みに成功すればThreadHeaderのインスタンス、失敗すればnull</returns>
		public static ThreadHeader Read(string filePath)
		{
			if (filePath == null)
			{
				throw new ArgumentNullException("filePath");
			}

			ThreadHeader result = null;

			lock (typeof(ThreadIndexer))
			{
				// インデックスファイルへのパス

				if (File.Exists(filePath))
				{
					try
					{
						CSPrivateProfile profile = new CSPrivateProfile();
						profile.Read(filePath);

						// 重要なセクションがなければエラー
						if (!profile.Sections.ContainsSection("Board") ||
							!profile.Sections.ContainsSection("Thread"))
						{
							return null;
						}

						BbsType bbs = (BbsType)Enum.Parse(typeof(BbsType), profile.GetString("Board", "BBS", "X2ch"));

						// 板情報
						result = TypeCreator.CreateThreadHeader(bbs);
						result.BoardInfo.Server = profile.GetString("Board", "Server", "Error");
						result.BoardInfo.Path = profile.GetString("Board", "Path", "Error");
						result.BoardInfo.Name = profile.GetString("Board", "Name", "Error");
						result.BoardInfo.Bbs = bbs;

						// スレッド情報
						result.ETag = profile.GetString("Thread", "ETag", String.Empty);
						result.LastWritten = profile.GetDateTime("Thread", "LastWritten");
						result.LastModified = profile.GetDateTime("Thread", "LastModified");
						result.Subject = profile.GetString("Thread", "Subject", "Error");
						result.ResCount = profile.GetInt("Thread", "ResCount", 0);
						result.GotResCount = profile.GetInt("Thread", "GotResCount", 0);
						result.GotByteCount = profile.GetInt("Thread", "GotByteCount", 0);
						result.NewResCount = profile.GetInt("Thread", "NewResCount", 0);
						result.Key = profile.GetString("Thread", "Key", "Error");

						// そのほかの情報
						result.Position = profile.GetFloat("Option", "Position", 0);
						result.Pastlog = profile.GetBool("Option", "Pastlog", false);
						result.UseGzip = profile.GetBool("Option", "UseGzip", false);
						result.Shiori = profile.GetInt("Option", "Shiori", 0);
						result.RefCount = profile.GetInt("Option", "RefCount", 0);
						result.Sirusi.FromArrayString(profile.GetString("Option", "Sirusi", ""));
					}
					catch (Exception ex)
					{
						TwinDll.Output(ex);
					}
				}
			}

			return result;
		}

		/// <summary>
		/// 指定したスレッドのインデックスを削除
		/// </summary>
		public static void Delete(Cache cache, ThreadHeader header)
		{
			if (cache == null)
			{
				throw new ArgumentNullException("cache");
			}
			if (header == null)
			{
				throw new ArgumentNullException("header");
			}

			lock (typeof(ThreadIndexer))
			{
				// インデックスファイルへのパス
				string filePath = cache.GetIndexPath(header);
				File.Delete(filePath);
			}
		}

		/// <summary>
		/// インデックスが存在するかどうかを判断
		/// </summary>
		/// <param name="header">基本的な情報が格納されたThreadHeader</param>
		/// <returns>存在すればtrue、存在しなければfalseを返す</returns>
		public static bool Exists(Cache cache, ThreadHeader header)
		{
			if (cache == null)
			{
				throw new ArgumentNullException("cache");
			}
			if (header == null)
			{
				throw new ArgumentNullException("header");
			}

			bool exists = false;

			lock (typeof(ThreadIndexer))
			{
				// インデックスファイルへのパス
				string filePath = cache.GetIndexPath(header);
				exists = File.Exists(filePath);
			}

			return exists;
		}

		/// <summary>
		/// 指定した板のインデックスを作成し直す
		/// </summary>
		/// <param name="cache"></param>
		/// <param name="board"></param>
		public static void Indexing(Cache cache, BoardInfo board)
		{
			OfflineThreadListReader reader = new OfflineThreadListReader(cache);
			List<ThreadHeader> items = new List<ThreadHeader>();

			if (reader.Open(board))
			{
				// ローカルに存在するすべてのファイルを読み込む
				while (reader.Read(items) != 0)
					;
				reader.Close();

				// サーバー情報をすべて書き換える
				foreach (ThreadHeader h in items)
				{
					h.BoardInfo.Server = board.Server;
					ThreadIndexer.SaveServerInfo(cache, h);
				}

				// 読み込んだスレッド情報を元に既得インデックス一覧を作成
				GotThreadListIndexer.Write(cache, board, items);
			}
		}
	}
}
