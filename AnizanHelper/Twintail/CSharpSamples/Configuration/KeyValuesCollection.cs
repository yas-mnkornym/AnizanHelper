// KeyValueCollection.cs

namespace CSharpSamples
{
	using System;
	using System.Collections;
	using System.Collections.Specialized;
	using System.IO;
	using System.Text;

	/// <summary>
	/// KeyValues クラスをコレクション管理。
	/// 
	/// 例
	/// [section1]
	/// key1
	/// key2
	/// key3
	/// ...
	/// </summary>
	public class KeyValuesCollection : IEnumerable
	{
		private Hashtable hash;

		/// <summary>
		/// 指定したキーを持つ値のコレクションを取得
		/// </summary>
		public StringCollection this[string key]
		{
			get
			{
				if (this.hash.ContainsKey(key))
				{
					KeyValues kv = (KeyValues)this.hash[key];
					return kv.Values;
				}
				return null;
			}
		}

		/// <summary>
		/// KeyValuesCollectionクラスのインスタンスを初期化
		/// </summary>
		public KeyValuesCollection()
		{
			// 
			// TODO: コンストラクタ ロジックをここに追加してください。
			//
			this.hash = new Hashtable();
		}

		/// <summary>
		/// 指定したファイルからデータを読み込む
		/// </summary>
		/// <param name="filePath"></param>
		public void Read(string filePath)
		{
			if (filePath == null)
			{
				throw new ArgumentNullException("filePath");
			}

			StreamReader sr = null;
			KeyValues keyset = null;
			string text;

			try
			{
				sr = new StreamReader(filePath, Encoding.Default);
				this.Clear();

				while ((text = sr.ReadLine()) != null)
				{
					// 空文字は無視
					if (text == string.Empty)
					{
						continue;
					}

					if (text.StartsWith("[") && text.EndsWith("]"))
					{
						keyset = new KeyValues(text.Substring(1, text.Length - 2));
						this.Add(keyset);
					}
					else if (keyset != null)
					{
						keyset.Values.Add(text);
					}
				}
			}
			finally
			{
				if (sr != null)
				{
					sr.Close();
				}
			}
		}

		/// <summary>
		/// 指定したファイルにデータを保存
		/// </summary>
		/// <param name="filePath"></param>
		public void Write(string filePath)
		{
			if (filePath == null)
			{
				throw new ArgumentNullException("filePath");
			}

			StreamWriter sw = null;
			try
			{
				sw = new StreamWriter(filePath, false, Encoding.Default);

				foreach (KeyValues _set in this.hash.Values)
				{
					sw.Write('[');
					sw.Write(_set.Key);
					sw.Write(']');

					sw.WriteLine();

					foreach (string val in _set.Values)
					{
						sw.WriteLine(val);
					}

					sw.WriteLine();
				}
			}
			finally
			{
				if (sw != null)
				{
					sw.Close();
				}
			}
		}

		/// <summary>
		/// キーを追加
		/// </summary>
		/// <param name="obj"></param>
		public void Add(KeyValues obj)
		{
			if (obj == null)
			{
				throw new ArgumentNullException("obj");
			}

			this.hash.Add(obj.Key, obj);
		}

		/// <summary>
		/// 指定したキーを持つ設定を削除
		/// </summary>
		/// <param name="key"></param>
		public void Remove(string key)
		{
			this.hash.Remove(key);
		}

		/// <summary>
		/// 値をすべて取得
		/// </summary>
		public void Clear()
		{
			this.hash.Clear();
		}

		/// <summary>
		/// KeyValuesCollectionを反復処理する列挙子を返す
		/// </summary>
		/// <returns></returns>
		public IEnumerator GetEnumerator()
		{
			return this.hash.Values.GetEnumerator();
		}
	}

	/// <summary>
	/// キーと値コレクションのセット
	/// </summary>
	[Serializable]
	public class KeyValues// : ISerializable
	{
		private string key;
		private StringCollection values;

		/// <summary>
		/// キー名を取得または設定
		/// </summary>
		public string Key
		{
			set
			{
				if (value == null)
				{
					throw new ArgumentNullException("Key");
				}

				this.key = value;
			}
			get { return this.key; }
		}

		/// <summary>
		/// 値のコレクションを取得
		/// </summary>
		public StringCollection Values
		{
			get
			{
				return this.values;
			}
		}

		/// <summary>
		/// KeyValuesクラスのインスタンスを初期化
		/// </summary>
		public KeyValues()
		{
			this.key = null;
			this.values = new StringCollection();
		}

		/// <summary>
		/// KeyValuesクラスのインスタンスを初期化
		/// </summary>
		/// <param name="key"></param>
		public KeyValues(string key) : this()
		{
			if (key == null)
			{
				throw new ArgumentNullException("key");
			}

			this.key = key;
		}

		/// <summary>
		/// KeyValuesクラスのインスタンスを初期化
		/// </summary>
		/// <param name="key"></param>
		/// <param name="array"></param>
		public KeyValues(string key, string[] array) : this()
		{
			if (key == null)
			{
				throw new ArgumentNullException("key");
			}

			if (array == null)
			{
				throw new ArgumentNullException("array");
			}

			this.key = key;
			this.values.AddRange(array);
		}
	}
}
