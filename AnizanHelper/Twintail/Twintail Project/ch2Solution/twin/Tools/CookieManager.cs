// CookieManager.cs

namespace Twin.Tools
{
	using System;
	using System.IO;
	using System.Text;
	using System.Net;
	using System.Runtime.Serialization.Formatters.Binary;
	using Twin.Util;
	using CSharpSamples;

	/// <summary>
	/// 掲示板に書き込む際に必要なクッキーを管理
	/// </summary>
	public class CookieManager
	{
		private const string CookieFile = "cookie.bin2";
		private Cache cache;
		
		/// <summary>
		/// CookieManagerクラスのインスタンスを初期化
		/// </summary>
		/// <param name="cache"></param>
		public CookieManager(Cache cache)
		{
			if (cache == null)
				throw new ArgumentNullException("cache");
			// 
			// TODO: コンストラクタ ロジックをここに追加してください。
			//
			this.cache = cache;
		}
		/*
		private string InternalGetCookieFilePath(BoardInfo bi)
		{
			return Path.Combine(cache.GetBbsRootDirectory(bi), CookieFile);
			//return Path.Combine(cache.GetFolderPath(bi), CookieFile);
		}*/

		/// <summary>
		/// 指定した板のクッキー情報を取得
		/// </summary>
		/// <param name="board">クッキー取得対象の板</param>
		/// <returns>クッキーが存在し取得できればtrue、そうでなければfalse</returns>
		public bool GetCookie(BoardInfo board)
		{
			if (board == null)
				throw new ArgumentNullException("board");
			/*NTwin 2011/05/31
			string filePath = InternalGetCookieFilePath(board);

			if (!File.Exists(filePath))
				return false;

			BinaryFormatter formatter = new BinaryFormatter();
			Stream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);

			try {
				CookieCollection coll = formatter.Deserialize(stream) as CookieCollection;
				board.CookieContainer.Add(coll);
			} finally {
				stream.Close();
			}*/
			// NTwin 2011/05/31
			board.CookieContainer = gCookies;

			return true;
		}

		/// <summary>
		/// 指定した板のクッキーを保存
		/// </summary>
		/// <param name="board">クッキー保存対象の板</param>
		public void SetCookie(BoardInfo board)
		{/*
			if (board == null)
				throw new ArgumentNullException("board");

			if (board.CookieContainer == null)
				return;

			string filePath = InternalGetCookieFilePath(board);

			CookieCollection coll = board.CookieContainer.GetCookies(new Uri(board.Url));

			BinaryFormatter formatter = new BinaryFormatter();
			Stream stream = new FileStream(filePath, FileMode.Create, FileAccess.Write);
			try {
				formatter.Serialize(stream, coll);
			} finally {
				stream.Close();
			}*/
		}

		// NTwin 2011/05/31
		public static CookieContainer gCookies = new CookieContainer();

		public static void LoadCookie()
		{
			string path = System.IO.Path.Combine(Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath), "cookie.bin");
			if (System.IO.File.Exists(path))
			{
				System.Runtime.Serialization.Formatters.Binary.BinaryFormatter formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
				using (System.IO.FileStream stream = new System.IO.FileStream(path, System.IO.FileMode.Open))
				{
					gCookies = formatter.Deserialize(stream) as System.Net.CookieContainer;
				}
			}
		}

		public static void SaveCookie()
		{
			if (gCookies != null)
			{
				string path = System.IO.Path.Combine(Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath), "cookie.bin");
				System.Runtime.Serialization.Formatters.Binary.BinaryFormatter formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
				using (System.IO.Stream stream = new System.IO.FileStream(path, System.IO.FileMode.Create, System.IO.FileAccess.Write))
				{
					formatter.Serialize(stream, gCookies);
				}
			}
		}
	}
}
