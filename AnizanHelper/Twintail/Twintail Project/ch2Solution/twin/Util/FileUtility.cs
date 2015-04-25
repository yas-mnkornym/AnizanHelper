// FileUtility.cs

namespace Twin.Util
{
	using System;
	using System.IO;
	using System.Text;

	/// <summary>
	/// ファイル関係のユーティリティ群
	/// </summary>
	public class FileUtility
	{
		/// <summary>
		/// ファイルを読み込む
		/// （ファイルが存在しなければ空文字列を返す）
		/// </summary>
		/// <param name="filePath">読み込むファイルのパス</param>
		/// <returns></returns>
		public static string ReadToEnd(string filePath)
		{
			return ReadToEnd(filePath, TwinDll.DefaultEncoding);
		}

		/// <summary>
		/// ファイルを読み込む
		/// （ファイルが存在しなければ空文字列を返す）
		/// </summary>
		/// <param name="filePath">読み込むファイルのパス</param>
		/// <returns></returns>
		public static string ReadToEnd(string filePath, Encoding enc)
		{
			string result = String.Empty;

			if (File.Exists(filePath))
			{
				using (StreamReader sr = new StreamReader(filePath, TwinDll.DefaultEncoding))
					result = sr.ReadToEnd();
			}

			return result;
		}

		/// <summary>
		/// 指定したファイルにテキストを保存
		/// </summary>
		/// <param name="filePath">保存先ファイル名</param>
		/// <param name="text">書き込む文字列</param>
		/// <param name="append">追加書き込みを行う場合はtrue、そうでなければfalse</param>
		public static void Write(string filePath, string text, bool append)
		{
			Write(filePath, text, append, TwinDll.DefaultEncoding);
		}

		/// <summary>
		/// 指定したファイルにテキストを保存
		/// </summary>
		/// <param name="filePath">保存先ファイル名</param>
		/// <param name="text">書き込む文字列</param>
		/// <param name="append">追加書き込みを行う場合はtrue、そうでなければfalse</param>
		public static void Write(string filePath, string text, bool append, Encoding enc)
		{
			if (filePath == null) {
				throw new ArgumentNullException("filePath");
			}

			string directory =
				Path.GetDirectoryName(filePath);

			if (!Directory.Exists(directory))
				Directory.CreateDirectory(directory);

			using (StreamWriter sw = new StreamWriter(filePath, append, enc))
			{
				sw.Write(text);
				sw.Flush();
			}
		}

		/// <summary>
		/// すべてを読み込みメモリストリームを作成
		/// </summary>
		/// <param name="baseStream"></param>
		/// <returns></returns>
		public static MemoryStream CreateMemoryStream(Stream baseStream)
		{
			if (baseStream == null) {
				throw new ArgumentNullException("baseStream");
			}

			MemoryStream memory = new MemoryStream();
			byte[] buffer = new byte[4096];
			int read = -1, allCount = 0;

			while (read != 0)
			{
				read = baseStream.Read(buffer, 0, buffer.Length);
				memory.Write(buffer, 0, read);
				allCount += read;
			}

			buffer = null;
			memory.Position = 0;
			memory.SetLength(allCount);

			return memory;
		}

		/// <summary>
		/// すべてを読み込みbyte配列に格納
		/// </summary>
		/// <param name="baseStream"></param>
		/// <returns></returns>
		public static byte[] ReadBytes(Stream baseStream)
		{
			return CreateMemoryStream(baseStream).ToArray();
		}
	}
}
