// CryptoConfig.cs

namespace CSharpSamples
{
	using System;
	using System.IO;
	using System.Text;
	using System.Text.RegularExpressions;
	using System.Collections;
	using System.Security.Cryptography;
	using System.Diagnostics;

	/// <summary>
	/// データを簡単なXOR暗号化して保存、または暗号化されたデータを復元します。
	/// </summary>
	public class XorCryptoConfig
	{
		private Hashtable _hash;
		private byte[] _key;

		/// <summary>
		/// CryptoConfigクラスのインスタンスを初期化。
		/// </summary>
		public XorCryptoConfig() : 
			this("mc$r[olUa`![c|k5yt:]SW@#{xx,6!dt=8sGq]n#xl)5CFf(<Hd")
		{
		}

		/// <summary>
		/// CryptoConfigクラスのインスタンスを初期化。
		/// </summary>
		public XorCryptoConfig(string key)
		{
			// 
			// TODO: コンストラクタ ロジックをここに追加してください。
			//
			_hash = new Hashtable();
			_key = Encoding.Default.GetBytes(key);
		}

		protected string GetBase(string key)
		{
			if (_hash.Contains(key))
			{
				return (string)_hash[key];
			}
			else {
				return null;
			}
		}

		/// <summary>
		/// 文字列形式の値を取得します。
		/// </summary>
		/// <param name="key"></param>
		/// <param name="def"></param>
		/// <returns>キーが存在すれば取得した文字列、エラーなら def を返します。</returns>
		public string Get(string key, string def)
		{
			string ret = GetBase(key);
			return (ret != null) ? ret : def;
		}

		/// <summary>
		/// 数値形式の値を取得します。
		/// </summary>
		/// <param name="key"></param>
		/// <param name="def"></param>
		/// <returns>キーが存在すれば取得した数値、エラーなら def を返します。</returns>
		public int GetInt(string key, int def)
		{
			string ret = GetBase(key);
			return (ret != null) ? Int32.Parse(ret) : def;
		}

		/// <summary>
		/// 指定したキーに値を設定します。
		/// </summary>
		/// <param name="key"></param>
		/// <param name="value"></param>
		public void Set(string key, object value)
		{
			if (_hash.Contains(key))
			{
				_hash[key] = value;
			}
			else {
				_hash.Add(key, value);
			}
		}

		/// <summary>
		/// 設定をすべて削除します。
		/// </summary>
		public void Clear()
		{
			_hash.Clear();
		}

		/// <summary>
		/// ファイルから設定を読み込みます。
		/// </summary>
		/// <param name="fileName"></param>
		public void Save(string fileName)
		{
			if (fileName == null) {
				throw new ArgumentNullException("fileName");
			}

			// 文字列 → 暗号化 → ファイルストリーム

			Stream fileStream = null;
			StringBuilder sb = new StringBuilder();

			try {
				fileStream = new FileStream(
					fileName, FileMode.Create, FileAccess.Write);

				foreach (string key in _hash.Keys)
				{
					string data = String.Format("{0}={1}\r\n", key, _hash[key]);
					sb.Append(data);
				}
				
				byte[] bytes = Encoding.Default.GetBytes(sb.ToString());
				bytes = Xor(bytes, _key);
				
				fileStream.Write(bytes, 0, bytes.Length);
			}
			catch (Exception ex) {
				Debug.WriteLine(ex.ToString());
			}
			finally {
				if (fileStream != null)
					fileStream.Close();
			}
		}

		/// <summary>
		/// 設定をファイルに保存します。
		/// </summary>
		/// <param name="fileName"></param>
		public void Load(string fileName)
		{
			if (fileName == null) {
				throw new ArgumentNullException("fileName");
			}

			// ファイルストリーム → 復号化 → メモリストリーム → ストリームリーダー

			Stream fileStream = null;
			StreamReader reader = null;
			MemoryStream mem = new MemoryStream();

			try {
				fileStream = new FileStream(
					fileName, FileMode.OpenOrCreate, FileAccess.Read);
				
				byte[] buffer = new byte[fileStream.Length];
				int read = fileStream.Read(buffer, 0, buffer.Length);

				buffer = Xor(buffer, _key);
				mem.Write(buffer, 0, buffer.Length);
				mem.Seek(0L, SeekOrigin.Begin);

				reader = new StreamReader(mem, Encoding.Default);

				string text;
				while ((text = reader.ReadLine()) != null)
				{
					Match m = Regex.Match(text, "(?<key>\\w+?)=(?<value>.+)");
					if (m.Success)
					{
						string key = m.Groups["key"].Value;
						string val = m.Groups["value"].Value;

						Set(key, val);
					}
				}
			}
			catch (Exception ex) {
				Debug.WriteLine(ex.ToString());
			}
			finally {
				if (reader != null)
					reader.Close();

				if (fileStream != null)
					fileStream.Close();
			}
		}

		/// <summary>
		/// data を指定した key を使用して xor 反転を行います。
		/// </summary>
		/// <param name="data"></param>
		/// <param name="key"></param>
		/// <returns></returns>
		public static byte[] Xor(byte[] data, byte[] key)
		{
			if (data == null) {
				throw new ArgumentNullException("data");
			}
			if (key == null) {
				throw new ArgumentNullException("key");
			}

			byte[] result = new byte[data.Length];
			
			for (int i = 0, k = 0; i < data.Length; i++)
			{
				result[i] = (byte)(data[i] ^ key[k++]);
				
				if (k >= key.Length)
					k = 0;
			}
			
			return result;
		}
	}
}
