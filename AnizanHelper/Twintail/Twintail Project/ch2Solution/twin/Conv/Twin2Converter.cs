// Twin2Converter.cs

namespace Twin.Conv
{
	using System;
	using System.Text;
	using System.IO;
	using System.Collections;
	using Twin.IO;
	using Twin.Text;
	using Twin.Bbs;

	/// <summary>
	/// twintail2 のログ相互コンバーター
	/// </summary>
	public class Twin2Converter : IConvertible
	{
		private bool useGzip;

		/// <summary>
		/// Twin2Converterクラスのインスタンスを初期化
		/// </summary>
		public Twin2Converter()
		{
			// 
			// TODO: コンストラクタ ロジックをここに追加してください。
			//
			useGzip = false;
		}

		public void Read(string filePath, out ThreadHeader header,
			out ResSetCollection resCollection)
		{
			// .idxファイルへのパスを求める
			string indexPath = GetIndexPath(filePath);

			if (!File.Exists(indexPath))
				throw new FileNotFoundException("インデックスファイルが存在しません");

			// インデックス情報を読み込む
			header = ThreadIndexer.Read(indexPath);
			if (header == null)
				throw new ConvertException("インデックスファイルの読み込みに失敗しました");

			resCollection = ReadFile(filePath, header.UseGzip);
		}

		private ResSetCollection ReadFile(string filePath, bool gzip)
		{
			ResSetCollection resCollection = new ResSetCollection();
			Stream stream = StreamCreator.CreateReader(filePath, gzip);
			ThreadParser dataParser = new X2chThreadParser();

			try {
				byte[] buffer = new byte[10240];
				int readCount = 0, index = 1;

				do {
					// バッファにデータを読み込む
					readCount = stream.Read(buffer, 0, buffer.Length);
					int parsed;

					// 解析してコレクションに格納
					ICollection collect = dataParser.Parse(buffer, readCount, out parsed);

					foreach (ResSet resSet in collect)
					{
						ResSet res = resSet;
						res.Index = index++;

						resCollection.Add(res);
					}
				} while (readCount != 0);
			}
			finally {
				if (stream != null)
					stream.Close();
			}

			return resCollection;
		}

		private string GetIndexPath(string filePath)
		{
			string indexPath = null;

			if (filePath.EndsWith(".dat.gz"))
				indexPath = filePath.Substring(0, filePath.Length - 7);

			else if (filePath.EndsWith(".dat"))
				indexPath = filePath.Substring(0, filePath.Length - 4);
			
			else { // 不正な拡張子
				throw new NotSupportedException(filePath + "\r\nこのファイルの拡張子はサポートしていません");
			}

			indexPath += ".idx";
			return indexPath;
		}
		
		public void Write(string filePath, ThreadHeader header,
			ResSetCollection resCollection)
		{
			Stream stream = StreamCreator.CreateWriter(filePath, useGzip, true);
			header.UseGzip = useGzip;

			try {
				ThreadFormatter formatter = new X2chThreadFormatter();
				string textData = formatter.Format(resCollection);
				byte[] byteData = TwinDll.DefaultEncoding.GetBytes(textData);

				stream.Write(byteData, 0, byteData.Length);
			}
			finally {
				if (stream != null)
					stream.Close();
			}

			// インデックスファイルを作成
			string indexPath = GetIndexPath(filePath);
			ThreadIndexer.Write(indexPath, header);
		}
	}
}
