// ThreadUtil.cs

namespace Twin.Util
{
	using System;
	using System.IO;
	using System.Text;
	using Twin.IO;
	using Twin.Bbs;
	using Twin.Conv;
	using Twin.Text;

	/// <summary>
	/// ThreadUtil の概要の説明です。
	/// </summary>
	public class ThreadUtil
	{
		/// <summary>
		/// スレッドをdat形式で保存
		/// </summary>
		/// <param name="cache">キャッシュ情報</param>
		/// <param name="header">保存するスレッド</param>
		/// <param name="filePath">保存先ファイルパス</param>
		public static void SaveDat(Cache cache, ThreadHeader header, string filePath)
		{
			if (cache == null)
				throw new ArgumentNullException("cache");
		
			if (header == null)
				throw new ArgumentNullException("header");

			if (filePath == null)
				throw new ArgumentNullException("filePath");

			// datの存在するパスを取得
			string fromPath = cache.GetDatPath(header);

			ThreadStorage reader = null;
			StreamWriter writer = null;
			
			ResSetCollection items = new ResSetCollection();
			X2chThreadFormatter formatter = new X2chThreadFormatter();

			try {
				// 読み込みストリームを開く
				reader = new LocalThreadStorage(cache, header, StorageMode.Read);
				// 書き込みストリームを開く
				writer = new StreamWriter(filePath, false, Encoding.GetEncoding("Shift_Jis"));

				// すべて読み込む
				while (reader.Read(items) != 0);
				writer.Write(formatter.Format(items));
			}
			finally {
				if (reader != null) reader.Close();
				if (writer != null) writer.Close();
			}
		}

		/// <summary>
		/// スレッドをhtml形式で保存
		/// </summary>
		/// <param name="cache">キャッシュ情報</param>
		/// <param name="header">保存するスレッド</param>
		/// <param name="filePath">保存先ファイルパス</param>
		public static void SaveHtml(Cache cache, ThreadHeader header, string filePath, ThreadSkinBase skin)
		{
			if (filePath == null)
				throw new ArgumentNullException("filePath");
				
			if (cache == null)
				throw new ArgumentNullException("cache");
		
			if (header == null)
				throw new ArgumentNullException("header");

			// datの存在するパスを取得
			string fromPath = cache.GetDatPath(header);

			ThreadStorage reader = null;
			StreamWriter writer = null;
			
			try {
				// 読み込みストリームを開く
				reader = new LocalThreadStorage(cache, header, StorageMode.Read);
				// 書き込みストリームを開く
				writer = new StreamWriter(filePath, false, TwinDll.DefaultEncoding);
				
				ResSetCollection items = new ResSetCollection();
				
				if (skin == null)
					skin = new HtmlSkin();

				// ヘッダを書き込む
				writer.WriteLine(skin.GetHeader(header));

				// 本文を書き込む
				while (reader.Read(items) != 0);
				writer.WriteLine(skin.Convert(items));

				// フッタを書き込む
				writer.WriteLine(skin.GetFooter(header));
			}
			finally {
				if (reader != null) reader.Close();
				if (writer != null) writer.Close();
			}
		}

		/// <summary>
		/// スレッドをmonalog形式で保存
		/// </summary>
		/// <param name="cache">キャッシュ情報</param>
		/// <param name="header">保存するスレッド</param>
		/// <param name="filePath">保存先ファイルパス</param>
		public static void SaveMonalog(Cache cache, ThreadHeader header, string filePath)
		{
			if (filePath == null)
				throw new ArgumentNullException("filePath");
				
			if (cache == null)
				throw new ArgumentNullException("cache");
		
			if (header == null)
				throw new ArgumentNullException("header");

			// datの存在するパスを取得
			string fromPath = cache.GetDatPath(header);
			MonalogConverter conv = new MonalogConverter();

			ThreadStorage reader = null;
			ResSetCollection items = new ResSetCollection();

			try {
				reader = new LocalThreadStorage(cache, header, StorageMode.Read);
				while (reader.Read(items) != 0);
				conv.Write(filePath, header, items);
			}
			finally {
				if (reader != null)
					reader.Close();
			}
		}

		/// <summary>
		/// 単独のdatファイル開き、を既得キャッシュとして保存
		/// </summary>
		/// <param name="cache"></param>
		/// <param name="target">単独のdatと関連づける板</param>
		/// <param name="filePath">datファイルのファイルパス</param>
		/// <param name="datNumber">dat番号</param>
		/// <param name="gzip">datファイルがgzip圧縮されていればtrue、そうでなければfalseを指定する</param>
		/// <returns>キャッシュされたスレッドのヘッダ情報を返す</returns>
		public static ThreadHeader OpenDat(Cache cache, BoardInfo target,
			string filePath, string datNumber, bool gzip)
		{
			// ヘッダー情報を作成
			ThreadHeader header = TypeCreator.CreateThreadHeader(target.Bbs);
			header.BoardInfo = target;
			header.Key = datNumber;
			header.UseGzip = gzip;
			header.Subject = String.Empty;

			ResSetCollection resItems = new ResSetCollection();

			using (Stream stream = StreamCreator.CreateReader(filePath, gzip))
			{
				X2chThreadParser parser = new X2chThreadParser();

				byte[] buffer = new byte[4096];
				bool first = true;
				int offset = 0, read, parsed;

				do {
					// バッファに読み込む
					read = stream.Read(buffer, 0, buffer.Length);
					offset += read;
					
					// 解析しResSet構造体の配列を作成
					ResSet[] array = parser.Parse(buffer, read, out parsed);
					resItems.AddRange(array);

					// スレタイを取得しておく
					if (first && array.Length > 0)
					{
						header.Subject = array[0].Tag as String;
						first = false;
					}

				} while (read != 0);

				// 既得バイト数とレス数を設定
				header.GotByteCount = offset;
				header.GotResCount = resItems.Count;
			}

			// datファイルの最終更新日を取得
			header.LastModified = File.GetLastWriteTime(filePath);
			
			// 読み込んだレスをキャッシュとして保存
			using (LocalThreadStorage storage = 
					   new LocalThreadStorage(cache, header, StorageMode.Write))
			{
				storage.Write(resItems);
			}

			// インデックス情報を生成
			ThreadIndexer.Write(cache, header);

			return header;
		}

		/// <summary>
		/// monalog形式のスレッドをキャッシュとして保存
		/// </summary>
		/// <param name="cache"></param>
		/// <param name="filePath"></param>
		/// <returns></returns>
		public static ThreadHeader OpenMonalog(Cache cache, string filePath)
		{
			ResSetCollection resItems = null;
			ThreadHeader header = null;

			MonalogConverter conv = new MonalogConverter();
			conv.Read(filePath, out header, out resItems);

			using (LocalThreadStorage storage = 
					   new LocalThreadStorage(cache, header, StorageMode.Write))
			{
				storage.Write(resItems);
			}

			ThreadIndexer.Write(cache, header);

			return header;
		}
	}
}
