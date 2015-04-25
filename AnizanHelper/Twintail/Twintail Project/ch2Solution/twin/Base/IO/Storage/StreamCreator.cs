// GzipStreamCreator.cs

namespace Twin.IO
{
	using System;
	using System.IO;
    using System.IO.Compression;
	using Twin.Util;

	/// <summary>
	/// Gzip圧縮を利用した入出力ストリームの初期化を行う
	/// </summary>
	public class StreamCreator
	{
		/// <summary>
		/// CreateDir
		/// </summary>
		/// <param name="filePath"></param>
		private static void CreateDir(string filePath)
		{
			string dir = Path.GetDirectoryName(filePath);
			if (!Directory.Exists(dir))
				Directory.CreateDirectory(dir);
		}

		/// <summary>
		/// ファイルを読み込むリーダーを初期化
		/// </summary>
		/// <param name="filePath">開くファイル名</param>
		/// <param name="useGzip">Gzip圧縮されているならtrue、そうでなければfalse</param>
		/// <returns></returns>
		public static Stream CreateReader(string filePath, bool useGzip)
		{
			CreateDir(filePath);

			Stream baseStream = new FileStream(
				filePath, FileMode.OpenOrCreate, FileAccess.Read);

			if (useGzip)
			{
                using (GZipStream input = new GZipStream(baseStream, CompressionMode.Decompress))
                {
                    baseStream = FileUtility.CreateMemoryStream(input);
                }
			}

			return baseStream;
		}

		/// <summary>
		/// ファイルに書き込むストリームを初期化
		/// </summary>
		/// <param name="filePath">書き込み先ファイル名</param>
		/// <param name="useGzip">書き込み時にGzip圧縮を使用する場合はtrue</param>
		/// <param name="append">追加書き込みを行うならtrue</param>
		/// <returns></returns>
		public static Stream CreateWriter(string filePath, bool useGzip, bool append)
		{
			Stream baseStream = null;
			CreateDir(filePath);

			if (useGzip)
			{
				byte[] bytes = new byte[0];

				if (append)
				{
					// 一端すべて解凍しバッファに詰める
					baseStream = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Read);
					using (GZipStream inp = new GZipStream(baseStream, CompressionMode.Decompress))
					{
						bytes = FileUtility.ReadBytes(inp);
						//inp.Close();
					}
				}

				// すべて解凍が終わったら再度ストリームを開き、
				// 読み込んだデータを圧縮
				baseStream = new FileStream(filePath, FileMode.Create, FileAccess.Write);
				baseStream = new GZipStream(baseStream, CompressionMode.Compress);

				// 解凍されたバッファを書き込む
				if (append)
				{
					baseStream.Write(bytes, 0, bytes.Length);
				}
			}
			else
			{
				baseStream =
					new FileStream(filePath, append ? FileMode.Append : FileMode.Create, FileAccess.Write);
			}

			return baseStream;
		}
	}
}
