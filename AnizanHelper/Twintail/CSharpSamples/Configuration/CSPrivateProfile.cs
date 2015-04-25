// CSPrivateProfile.cs

namespace CSharpSamples
{
	using System;
	using System.IO;
	using System.Text;
	using System.ComponentModel;
	using System.Drawing;
	using System.Collections;
	using System.Collections.Generic;

	/// <summary>
	/// Windowsのiniファイル形式を読み書きするクラス。
	/// 空の改行やコメント、設定の順番はそのまま維持するように変更。
	/// 
	/// 例
	/// -------------------------
	/// #
	/// # コメントです
	/// #
	/// [section1]
	/// key1=value1
	/// key2=value2
	/// -------------------------
	/// </summary>
	public class CSPrivateProfile
	{
		private CSPrivateProfileSectionCollection sections = null;

		/// <summary>
		/// セクションのコレクションを取得
		/// </summary>
		public CSPrivateProfileSectionCollection Sections {
			get {
				return sections;
			}
		}

		/// <summary>
		/// CSPrivateProfileクラスのインスタンスを初期化
		/// </summary>
		public CSPrivateProfile()
		{
			// 
			// TODO: コンストラクタ ロジックをここに追加してください。
			//
			sections = new CSPrivateProfileSectionCollection();
		}

		/// <summary>
		/// textがセクションかどうかを判断
		/// </summary>
		/// <param name="text"></param>
		/// <returns></returns>
		private bool IsSection(string text)
		{
			return text.StartsWith("[") && text.EndsWith("]");
		}

		private bool IsComment(string text)
		{
			return text.StartsWith("#");
		}

		private bool IsSpace(string text)
		{
			text = text.Trim();
			return text.Length == 0;
		}

		/// <summary>
		/// 現在のセクション情報をクリアして、ファイルを読み込む。
		/// ファイルが存在しなければなにもしない。
		/// </summary>
		/// <param name="filePath"></param>
		public virtual void Read(string filePath)
		{
			if (filePath == null) {
				throw new ArgumentNullException("filePath");
			}

			RemoveAll();

			if (File.Exists(filePath))
			{
				StreamReader sr = null;
				CSPrivateProfileSection sec = null;
				string text = null;
				List<string> comments = new List<string>();

				try {
					sr = new StreamReader(filePath, Encoding.Default);											

					while ((text = sr.ReadLine()) != null)
					{
						if (IsComment(text))
						{
							comments.Add(text.Substring(1));
						}
						else if (IsSpace(text))
						{
							comments.Add(null);
						}
						else if (IsSection(text))
						{
							string name = text.Substring(1, text.Length-2);
							sec = new CSPrivateProfileSection(name, comments);
							sections.Add(sec);

							comments.Clear();
						}
						else if (sec != null)
						{
							int token = text.IndexOf('=');
							if (token >= 0)
							{
								CSPrivateProfileKeyValue kv = 
									new CSPrivateProfileKeyValue(text.Substring(0, token), text.Substring(token+1), comments);
								
								comments.Clear();

								sec.Add(kv);
							}
						}
					}
				}
				finally {
					if (sr != null)
						sr.Close();
				}
			}
		}

		/// <summary>
		/// ファイルに書き込む
		/// </summary>
		/// <param name="filePath"></param>
		public virtual void Write(string filePath)
		{
			if (filePath == null) {
				throw new ArgumentNullException("filePath");
			}

			StreamWriter sw = null;

			try {
				sw = new StreamWriter(filePath, false, Encoding.Default);

				foreach (CSPrivateProfileSection sec in sections)
				{
					if (sec.Count > 0)
					{
						WriteComments(sw, sec.Comments);

						sw.Write('[');
						sw.Write(sec.Name);
						sw.WriteLine(']');

						foreach (CSPrivateProfileKeyValue v in sec)
						{
							WriteComments(sw, v.Comments);

							sw.Write(v.Key);
							sw.Write('=');
							sw.WriteLine(v.Value);
						}
					}
				}
			}
			finally {
				if (sw != null)
				{
					sw.Flush();
					sw.Close();
				}
			}
		}

		private void WriteComments(TextWriter w, List<string> comments)
		{
			foreach (string text in comments)
			{
				// null の場合は改行のみ
				if (text == null)
				{
					w.WriteLine();
				}
				else
				{
					w.Write('#');
					w.WriteLine(text);
				}
			}
		}

		/// <summary>
		/// すべてのセクションを削除
		/// </summary>
		public void RemoveAll()
		{
			sections.Clear();
		}

		/// <summary>
		/// 指定したセクションの指定したキーに値を設定
		/// </summary>
		/// <param name="section"></param>
		/// <param name="key"></param>
		/// <param name="value"></param>
		public void SetValue(string section, string key, object value)
		{
			if (section == null)
				throw new ArgumentNullException("section");
			if (key == null)
				throw new ArgumentNullException("key");

			CSPrivateProfileSection sec = sections[section];

			if (value != null)
			{
				TypeConverter con = TypeDescriptor.GetConverter(value);
				sec[key] = con.ConvertToString(value);
			}
			else {
				sec[key] = null;
			}
		}

		/// <summary>
		/// byte[]型データをbase64にエンコード後に値を設定
		/// </summary>
		/// <param name="section"></param>
		/// <param name="key"></param>
		/// <param name="data"></param>
		public void SetBase64(string section, string key, byte[] data)
		{
			string base64 = Convert.ToBase64String(data);
			SetValue(section, key, base64);
		}

		/// <summary>
		/// 指定したセクションとキーに設定されているstring値を取得
		/// </summary>
		/// <param name="section"></param>
		/// <param name="key"></param>
		/// <param name="def"></param>
		/// <returns></returns>
		protected string GetValueInternal(string section, string key)
		{
			if (section == null)
				throw new ArgumentNullException("section");
			if (key == null)
				throw new ArgumentNullException("key");

			if (!sections.ContainsSection(section))
				return null;

			CSPrivateProfileSection sec = sections[section];
			return (sec.ContainsKey(key) ? sec[key] : null);
		}

		/// <summary>
		/// 文字列型の値を取得
		/// </summary>
		/// <param name="section"></param>
		/// <param name="key"></param>
		/// <returns>値が存在しない場合はnullを返す</returns>
		public string GetString(string section, string key)
		{
			return GetString(section, key, null);
		}

		/// <summary>
		/// 文字列型の値を取得
		/// </summary>
		/// <param name="section"></param>
		/// <param name="key"></param>
		/// <param name="def"></param>
		/// <returns></returns>
		public string GetString(string section, string key, string def)
		{
			string r = GetValueInternal(section, key);
			return (r != null) ? r : def;
		}

		/// <summary>
		/// 指定したセクションとキーに設定されているint値を取得
		/// </summary>
		/// <param name="section"></param>
		/// <param name="key"></param>
		/// <returns>値が存在しない場合は0を返す</returns>
		public int GetInt(string section, string key)
		{
			return GetInt(section, key, 0);
		}

		/// <summary>
		/// 指定したセクションとキーに設定されているint値を取得
		/// </summary>
		/// <param name="section"></param>
		/// <param name="key"></param>
		/// <param name="def"></param>
		/// <returns></returns>
		public int GetInt(string section, string key, int def)
		{
			string r = GetValueInternal(section, key);
			return (r != null) ? Convert.ToInt32(r, 10) : def;
		}

		/// <summary>
		/// 指定したセクションとキーに設定されているbool値を取得
		/// </summary>
		/// <param name="section"></param>
		/// <param name="key"></param>
		/// <returns>大河存在しない場合は false を返す</returns>
		public bool GetBool(string section, string key)
		{
			return GetBool(section, key, false);
		}

		/// <summary>
		/// 指定したセクションとキーに設定されているbool値を取得
		/// </summary>
		/// <param name="section"></param>
		/// <param name="key"></param>
		/// <param name="def"></param>
		/// <returns></returns>
		public bool GetBool(string section, string key, bool def)
		{
			string r = GetValueInternal(section, key);
			return (r != null) ? Convert.ToBoolean(r) : def;
		}

		/// <summary>
		/// 指定したセクションとキーに設定されているfloat値を取得
		/// </summary>
		/// <param name="section"></param>
		/// <param name="key"></param>
		/// <returns>値が存在しない場合は0を返す</returns>
		public float GetFloat(string section, string key)
		{
			return GetFloat(section, key, 0.0f);
		}


		/// <summary>
		/// 指定したセクションとキーに設定されているfloat値を取得
		/// </summary>
		/// <param name="section"></param>
		/// <param name="key"></param>
		/// <param name="def"></param>
		/// <returns></returns>
		public float GetFloat(string section, string key, float def)
		{
			string r = GetValueInternal(section, key);
			return (r != null) ? Convert.ToSingle(r) : def;
		}

		/// <summary>
		/// char型の値を取得
		/// </summary>
		/// <param name="section"></param>
		/// <param name="key"></param>
		/// <returns>値が存在しない場合は\0を返す</returns>
		public char GetChar(string section, string key)
		{
			return GetChar(section, key, '\0');
		}

		/// <summary>
		/// char型の値を取得
		/// </summary>
		/// <param name="section"></param>
		/// <param name="key"></param>
		/// <param name="def"></param>
		/// <returns></returns>
		public char GetChar(string section, string key, char def)
		{
			string r = GetValueInternal(section, key);
			return (r != null) ? Convert.ToChar(r) : def;
		}

		/// <summary>
		/// byte型の値を取得
		/// </summary>
		/// <param name="section"></param>
		/// <param name="key"></param>
		/// <returns>値が存在しない場合は0を返す</returns>
		public byte GetByte(string section, string key)
		{
			return GetByte(section, key, 0);
		}

		/// <summary>
		/// byte型の値を取得
		/// </summary>
		/// <param name="section"></param>
		/// <param name="key"></param>
		/// <param name="def"></param>
		/// <returns></returns>
		public byte GetByte(string section, string key, byte def)
		{
			string r = GetValueInternal(section, key);
			return (r != null) ? Convert.ToByte(r) : def;
		}

		/// <summary>
		/// DateTime型の値を取得
		/// </summary>
		/// <param name="section"></param>
		/// <param name="key"></param>
		/// <returns>値が存在しない場合は DateTime.MinValue を返す</returns>
		public DateTime GetDateTime(string section, string key)
		{
			return GetDateTime(section, key, DateTime.MinValue);
		}

		/// <summary>
		/// DateTime型の値を取得
		/// </summary>
		/// <param name="section"></param>
		/// <param name="key"></param>
		/// <param name="def"></param>
		/// <returns></returns>
		public DateTime GetDateTime(string section, string key, DateTime def)
		{
			string val = GetValueInternal(section, key);
			if (val != null)
			{
				TypeConverter con = TypeDescriptor.GetConverter(typeof(DateTime));

				if (con != null && con.IsValid(val))
					return (DateTime)con.ConvertFromString(val);
			}

			return def;
		}

		/// <summary>
		/// Point型の値を取得
		/// </summary>
		/// <param name="section"></param>
		/// <param name="key"></param>
		/// <returns>値が存在しない場合は Point.Empty を返す</returns>
		public Point GetPoint(string section, string key)
		{
			return GetPoint(section, key, Point.Empty);
		}

		/// <summary>
		/// Point型の値を取得
		/// </summary>
		/// <param name="section"></param>
		/// <param name="key"></param>
		/// <param name="def"></param>
		/// <returns></returns>
		public Point GetPoint(string section, string key, Point def)
		{
			string val = GetValueInternal(section, key);
			if (val != null)
			{
				TypeConverter con = TypeDescriptor.GetConverter(typeof(Point));

				if (con != null && con.IsValid(val))
					return (Point)con.ConvertFromString(val);
			}
			return def;
		}

		/// <summary>
		/// Size型の値を取得
		/// </summary>
		/// <param name="section"></param>
		/// <param name="key"></param>
		/// <returns>値が存在しない場合は Size.Empty を返す</returns>
		public Size GetSize(string section, string key)
		{
			return GetSize(section, key, Size.Empty);
		}

		/// <summary>
		/// Size型の値を取得
		/// </summary>
		/// <param name="section"></param>
		/// <param name="key"></param>
		/// <param name="def"></param>
		/// <returns></returns>
		public Size GetSize(string section, string key, Size def)
		{
			string val = GetValueInternal(section, key);
			if (val != null)
			{
				TypeConverter con = TypeDescriptor.GetConverter(typeof(Size));

				if (con != null && con.IsValid(val))
					return (Size)con.ConvertFromString(val);
			}
			return def;
		}

		/// <summary>
		/// Rectangle型の値を取得
		/// </summary>
		/// <param name="section"></param>
		/// <param name="key"></param>
		/// <returns>値が存在しない場合は Rectangle.Empty を返す</returns>
		public Rectangle GetRect(string section, string key)
		{
			return GetRect(section, key, Rectangle.Empty);
		}

		/// <summary>
		/// Rectangle型の値を取得
		/// </summary>
		/// <param name="section"></param>
		/// <param name="key"></param>
		/// <param name="def"></param>
		/// <returns></returns>
		public Rectangle GetRect(string section, string key, Rectangle def)
		{
			string val = GetValueInternal(section, key);
			if (val != null)
			{
				TypeConverter con = TypeDescriptor.GetConverter(typeof(Rectangle));

				if (con != null && con.IsValid(val))
					return (Rectangle)con.ConvertFromString(val);
			}
			return def;
		}

		/// <summary>
		/// Color型の値を取得
		/// </summary>
		/// <param name="section"></param>
		/// <param name="key"></param>
		/// <returns>値が存在しない場合は Color.Empty を返す</returns>
		public Color GetColor(string section, string key)
		{
			return GetColor(section, key, Color.Empty);
		}

		/// <summary>
		/// Color型の値を取得
		/// </summary>
		/// <param name="section"></param>
		/// <param name="key"></param>
		/// <param name="def"></param>
		/// <returns></returns>
		public Color GetColor(string section, string key, Color def)
		{
			string val = GetValueInternal(section, key);
			if (val != null)
			{
				TypeConverter con = TypeDescriptor.GetConverter(typeof(Color));

				if (con != null && con.IsValid(val))
					return (Color)con.ConvertFromString(val);
			}
			return def;
		}

		/// <summary>
		/// 列挙型の値を取得
		/// </summary>
		/// <param name="section"></param>
		/// <param name="key"></param>
		/// <param name="def"></param>
		/// <returns>値が存在しない場合は def を返す</returns>
		public Enum GetEnum(string section, string key, Enum def)
		{
			string val = GetValueInternal(section, key);
			if (val != null)
			{
				TypeConverter con = TypeDescriptor.GetConverter(def.GetType());

				if (con != null && con.IsValid(val))
					return (Enum)con.ConvertFrom(val);
			}
			return def;
		}

		/// <summary>
		/// base64の数字で構成された値をbyte[]型に変換して取得。
		/// セクションまたはキーが存在しない場合はnullを返す。
		/// </summary>
		/// <param name="section"></param>
		/// <param name="key"></param>
		/// <returns></returns>
		public byte[] GetBase64(string section, string key)
		{
			string r = GetValueInternal(section, key);
			return (r != null) ? Convert.FromBase64String(r) : null;
		}

		/// <summary>
		/// 指定したセクションのキーに値を設定
		/// </summary>
		/// <param name="section"></param>
		/// <param name="key"></param>
		/// <param name="value"></param>
		/// <param name="filePath"></param>
		public static void SetValue(string section, string key, object value, string filePath)
		{
			PrivateProfile.Write(section, key, value.ToString(), filePath);
		}

		/// <summary>
		/// 指定したセクションのキーに設定されているstring値を取得
		/// </summary>
		/// <param name="section"></param>
		/// <param name="key"></param>
		/// <param name="def"></param>
		/// <param name="filePath"></param>
		/// <returns></returns>
		public static string GetString(string section, string key, string def, string filePath)
		{
			StringBuilder buffer = new StringBuilder(2048);
			PrivateProfile.ReadString(section, key, def, buffer, 2048, filePath);

			return buffer.ToString();
		}

		/// <summary>
		/// 指定したセクションのキーに設定されているint値を取得
		/// </summary>
		/// <param name="section"></param>
		/// <param name="key"></param>
		/// <param name="def"></param>
		/// <param name="filePath"></param>
		/// <returns></returns>
		public static int GetInt(string section, string key, int def, string filePath)
		{
			return (int)PrivateProfile.ReadInt(section, key, def, filePath);
		}
	}

	/// <summary>
	/// CSPrivateProfileのセクションを表す
	/// </summary>
	public class CSPrivateProfileSection : IEnumerable
	{
		private string name;
		private List<CSPrivateProfileKeyValue> values;
		private List<string> comments;

		/// <summary>
		/// 登録されているキー数を取得
		/// </summary>
		public int Count {
			get {
				return values.Count;
			}
		}

		public List<string> Comments {
			get {
				return comments;
			}
		}

		/// <summary>
		/// 指定したキーの値を取得または設定
		/// </summary>
		public string this[string key] {
			set {
				CSPrivateProfileKeyValue kv = GetKeyValue(key);
				
				if (kv == null)
				{
					kv = new CSPrivateProfileKeyValue(key, value);
					values.Add(kv);
				}
				else {
					kv.Value = value;
				}
			}
			get {
				CSPrivateProfileKeyValue kv = GetKeyValue(key);
				
				if (kv == null)
				{
					return null;
				}
				else {
					return kv.Value;
				}
			}
		}

		/// <summary>
		/// セクション名を取得または設定
		/// </summary>
		public string Name {
			set {
				if (value == null)
					throw new ArgumentNullException("Name");

				name = value;
			}
			get { return name; }
		}

		/// <summary>
		/// すべてのキーを取得
		/// </summary>
		public List<string> Keys {
			get {
				return values.ConvertAll<string>(delegate(CSPrivateProfileKeyValue v)
				{
					return v.Key;
				});
			}
		}

		/// <summary>
		/// すべての値を取得
		/// </summary>
		public List<string> Values {
			get {
				return values.ConvertAll<string>(delegate(CSPrivateProfileKeyValue v)
				{
					return v.Value;
				});
			}
		}

		/// <summary>
		/// CSPrivateProfileSectionクラスのインスタンスを初期化
		/// </summary>
		public CSPrivateProfileSection()
		{
			name = String.Empty;
			values = new List<CSPrivateProfileKeyValue>();
			comments = new List<string>();
		}

		/// <summary>
		/// CSPrivateProfileSectionクラスのインスタンスを初期化
		/// </summary>
		/// <param name="name">セクション名</param>
		public CSPrivateProfileSection(string name) : this()
		{
			this.name = name;
		}

		public CSPrivateProfileSection(string name, List<string> comments) : this(name)
		{
			this.comments.AddRange(comments);
		}
 
		public void Add(CSPrivateProfileKeyValue kv)
		{
			values.Add(kv);
		}

		public void SetKeyValue(CSPrivateProfileKeyValue kv)
		{
			CSPrivateProfileKeyValue temp = GetKeyValue(kv.Key);

			if (temp == null)
			{
				values.Add(kv);
			}
			else {
				temp.Value = kv.Value;
				temp.Comments.Clear();
				temp.Comments.AddRange(kv.Comments);
			}
		}

		public CSPrivateProfileKeyValue GetKeyValue(string key)
		{
			return values.Find(delegate(CSPrivateProfileKeyValue v)
			{
				return (v.Key == key);
			});
		}

		/// <summary>
		/// 指定したキーが含まれているかどうかを判断
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		public bool ContainsKey(string key)
		{
			return values.Exists(delegate (CSPrivateProfileKeyValue v) {
				return (v.Key == key);
			});
		}

		public IEnumerator GetEnumerator()
		{
			return values.GetEnumerator();
		}
	}

	/// <summary>
	/// CSPrivateProfileSectionをコレクション管理
	/// </summary>
	public class CSPrivateProfileSectionCollection : System.Collections.IEnumerable
	{
		private List<CSPrivateProfileSection> sections;

		/// <summary>
		/// 登録されているセクション数を取得
		/// </summary>
		public int Count {
			get {
				return sections.Count;
			}
		}

		/// <summary>
		/// 指定したキーを持つセクションを取得
		/// </summary>
		public CSPrivateProfileSection this[string key] {
			get {
				CSPrivateProfileSection sec = sections.Find(delegate (CSPrivateProfileSection s) {
					return (s.Name == key);
				});

				if (sec == null)
				{
					sec = new CSPrivateProfileSection(key);
					sections.Add(sec);
				}
				return sec;
			}
		}

		/// <summary>
		/// CSPrivateProfileSectionCollectionクラスのインスタンスを初期化
		/// </summary>
		public CSPrivateProfileSectionCollection()
		{
			sections = new List<CSPrivateProfileSection>();
		}

		/// <summary>
		/// セクションを追加
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		public void Add(CSPrivateProfileSection obj)
		{
			if (obj == null)
				throw new ArgumentNullException("obj");

			sections.Add(obj);
		}

		/// <summary>
		/// セクションの配列を追加
		/// </summary>
		/// <param name="array"></param>
		public void AddRange(CSPrivateProfileSection[] array)
		{
			if (array == null)
				throw new ArgumentNullException("array");

			sections.AddRange(array);
		}

		/// <summary>
		/// 指定したキーを持つセクションを削除
		/// </summary>
		/// <param name="key"></param>
		public void Remove(string key)
		{
			sections.RemoveAll(delegate (CSPrivateProfileSection s) {
				return (s.Name == key);
			});
		}

		/// <summary>
		/// 指定したセクションが存在するかどうかを判断
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		public bool ContainsSection(string key)
		{
			return sections.Exists(delegate (CSPrivateProfileSection s) {
				return (s.Name == key);
			});
		}

		/// <summary>
		/// すべてのセクションを削除
		/// </summary>
		public void Clear()
		{
			sections.Clear();
		}

		/// <summary>
		/// コレクションを反復処理できる列挙子を取得
		/// </summary>
		public System.Collections.IEnumerator GetEnumerator()
		{
			return sections.GetEnumerator();
		}
	}

	/// <summary>
	/// キーと値、コメントを管理します。
	/// </summary>
	public class CSPrivateProfileKeyValue
	{
		private List<string> comments = new List<string>();
		private string key, value;

		public string Key {
			set {
				key = value;
			}
			get {
				return key;
			}
		}

		public string Value {
			set {
				this.value = value;
			}
			get {
				return value;
			}
		}

		public List<string> Comments
		{
			get
			{
				return comments;
			}
		}

		public CSPrivateProfileKeyValue()
		{
			key = value = String.Empty;
		}

		public CSPrivateProfileKeyValue(string key, string value)
		{
			this.key = key;
			this.value = value;
		}

		public CSPrivateProfileKeyValue(string key, string value, List<string> comments)
			: this(key, value)
		{
			this.comments.AddRange(comments);
		} 
	}
}
