// ThreadHeader.cs

namespace Twin
{
	using System;
	using System.Runtime.Serialization;

	/// <summary>
	/// スレッドの基本ヘッダ情報を表す
	/// </summary>
	public abstract class ThreadHeader : IComparable
	{
		private BoardInfo board;
		private string subject;
		private string key;
		private string etag;
		private DateTime lastModified;
		private DateTime lastWritten;
		private int resCount;
		private int gotResCount;
		private int gotByteCount;
		private int newResCount;
		private int no;

		private SortedValueCollection<int> sirusi;
		private int refcount;
		private int shiori;
		private bool pastlog;
		private bool newthread;
		private float position;
		private bool useGzip;
		private object tag;

		private int hashcode;

		/// <summary>
		/// 板情報を取得または設定
		/// </summary>
		public BoardInfo BoardInfo {
			set {
				if (value == null) {
					throw new ArgumentNullException("Board");
				}
				board = value;
				CalcHash();
			}
			get { return board; }
		}
		
		/// <summary>
		/// スレッドの番号を取得または設定
		/// </summary>
		public int No {
			set { no = value; }
			get { return no; }
		}

		/// <summary>
		/// スレッドタイトルを取得または設定
		/// </summary>
		public string Subject {
			set {
				if (value == null) {
					throw new ArgumentNullException("Subject");
				}
				subject = value;
			}
			get { return subject; }
		}

		/// <summary>
		/// DAT番号を取得または設定
		/// </summary>
		public string Key {
			set {
				if (value == null) {
					throw new ArgumentNullException("Key");
				}
				key = value;
				CalcHash();
			}
			get { return key; }
		}

		/// <summary>
		/// ETagを取得または設定
		/// </summary>
		public string ETag {
			set {
				etag = value;
				if (etag == null)
					etag = String.Empty;
			}
			get { return etag; }
		}

		/// <summary>
		/// DATの存在するURLを取得
		/// </summary>
		public abstract string DatUrl {
			get;
		}

		/// <summary>
		/// URLを取得
		/// </summary>
		public abstract string Url {
			get;
		}

		/// <summary>
		/// スレッドが立てられた日付を取得
		/// </summary>
		public DateTime Date {
			get { 
				DateTime result = new DateTime(1970, 1, 1);

				try {
					// スレッドが立てられた日付を計算
					int seconds;
					if (Int32.TryParse(Key, out seconds))
						result = result.AddSeconds(seconds);
				}
				catch (Exception) {}

				return result;
			}
		}

		/// <summary>
		/// スレッドの最終更新日を取得または設定
		/// </summary>
		public DateTime LastModified {
			set { lastModified = value; }
			get { return lastModified; }
		}

		/// <summary>
		/// スレッドの最終書込日を取得または設定
		/// </summary>
		public DateTime LastWritten {
			set { lastWritten = value; }
			get { return lastWritten; }
		}

		/// <summary>
		/// レス数を取得または設定
		/// </summary>
		public int ResCount {
			set { resCount = value; }
			get { return resCount; }
		}

		/// <summary>
		/// 取得済みレス数を取得または設定
		/// </summary>
		public int GotResCount {
			set {
				gotResCount = value; 

				if (gotResCount > resCount)
					resCount = gotResCount;
			}
			get { return gotResCount; }
		}

		/// <summary>
		/// 既得済みバイト数を取得または設定
		/// </summary>
		public int GotByteCount {
			set { gotByteCount = value; }
			get { return gotByteCount; }
		}

		/// <summary>
		/// 新着レス数を取得または設定
		/// </summary>
		public int NewResCount {
			set { newResCount = value; }
			get { return newResCount; }
		}

		/// <summary>
		/// レス数から既得を引いた数を取得
		/// </summary>
		public int SubNewResCount {
			get {
				if (GotResCount > 0)
					return (ResCount - GotResCount);
				return 0;
			}
		}

		/// <summary>
		/// レス数の上限を取得
		/// </summary>
		public abstract int UpperLimitResCount {
			get;
		}

		/// <summary>
		/// 最大レス数を越えているかどうかを判断
		/// </summary>
		public bool IsLimitOverThread {
			get { return (ResCount >= UpperLimitResCount); }
		}

		/// <summary>
		/// このスレッドを参照した回数
		/// </summary>
		public int RefCount {
			set { refcount = value; }
			get { return refcount; }
		}

		/// <summary>
		/// しおり番号を取得または設定
		/// </summary>
		public int Shiori {
			set { shiori = Math.Max(0, value); }
			get { return shiori; }
		}

		/// <summary>
		/// スクロール位置を取得または設定
		/// </summary>
		public float Position {
			set { position = value; }
			get { return position; }
		}

		/// <summary>
		/// 過去ログかどうかを表す値を取得または設定
		/// </summary>
		public bool Pastlog {
			set { pastlog = value; }
			get { return pastlog; }
		}

		/// <summary>
		/// 新着スレッドかどうかを示す値を取得または設定
		/// </summary>
		public bool IsNewThread {
			set { newthread = value; }
			get { return newthread; }
		}

		/// <summary>
		/// Gzip圧縮を利用するかどうかを表す値を取得
		/// </summary>
		public bool UseGzip {
			set { useGzip = value; }
			get { return useGzip; }
		}

		/// <summary>
		/// Tagを取得または設定
		/// </summary>
		public object Tag {
			set { tag = value; }
			get { return tag; }
		}

		/// <summary>
		/// 印されたレス番号のコレクションを取得
		/// </summary>
		public SortedValueCollection<int> Sirusi {
			get { return sirusi; }
		}

		/// <summary>
		/// ThreadHeaderクラスのインスタンスを初期化
		/// </summary>
		public ThreadHeader()
		{
			// 
			// TODO: コンストラクタ ロジックをここに追加してください。
			//
			board = new BoardInfo();
			subject = String.Empty;
			key = String.Empty;
			lastModified = new DateTime(0);
			lastWritten = new DateTime(0);
			no = 0;
			resCount = 0;
			gotResCount = 0;
			gotByteCount = 0;
			newResCount = 0;
			position = 0;
			shiori = 0;
			refcount = 0;
			useGzip = false;
			pastlog = false;
			newthread = false;
			etag = null;
			tag = null;
			sirusi = new SortedValueCollection<int>();

			hashcode = -1;
		}

		/// <summary>
		/// ThreadHeaderクラスのインスタンスを初期化
		/// </summary>
		/// <param name="board"></param>
		/// <param name="key"></param>
		public ThreadHeader(BoardInfo board, string key) : this()
		{
			this.board = board;
			this.key = key;
		}

		/// <summary>
		/// ThreadHeaderクラスのインスタンスを初期化
		/// </summary>
		/// <param name="board"></param>
		/// <param name="key"></param>
		/// <param name="subject"></param>
		public ThreadHeader(BoardInfo board, string key, string subject) : this(board, key)
		{
			this.subject = subject;
		}

		/// <summary>
		/// 現在のインスタンスの値をheaderにコピー
		/// </summary>
		/// <param name="header"></param>
		public void CopyTo(ThreadHeader header)
		{
			if (header == null) {
				throw new ArgumentNullException("header");
			}

			header.No = no;
			header.Key = key;
			header.BoardInfo = board;
			header.ETag = etag;
			header.ResCount = resCount;
			header.GotByteCount = gotByteCount;
			header.GotResCount = gotResCount;
			header.LastModified = lastModified;
			header.LastWritten = lastWritten;
			header.NewResCount = newResCount;
			header.Position = position;
			header.Subject = subject;
			header.UseGzip = useGzip;
			header.Shiori = shiori;
			header.Pastlog = pastlog;
			header.newthread = newthread;
			header.refcount = refcount;
			header.Tag = tag;

			sirusi.Copy(header.sirusi);
		}

		/// <summary>
		/// ハッシュコードを計算 (URLの値を変更したときに呼ぶ必要がある)
		/// </summary>
		protected void CalcHash()
		{
			hashcode = Url.GetHashCode();
		}

		/// <summary>
		/// このインスタンスのハッシュ値を取得
		/// </summary>
		/// <returns></returns>
		public override int GetHashCode()
		{
			return hashcode;
		}

		/// <summary>
		/// 現在のインスタンスとobjを比較
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		public override bool Equals(object obj)
		{
			return Equals(obj as ThreadHeader);
		}

		/// <summary>
		/// 現在のインスタンスとheaderを比較
		/// </summary>
		/// <param name="header"></param>
		/// <returns></returns>
		public bool Equals(ThreadHeader header)
		{
			if (header == null)
				return false;

			if (board.Path == header.board.Path && key == header.key)
				return true;

			else
				return false;
		}

		/// <summary>
		/// 現在のインスタンスを文字列形式に変換
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			return String.Format("No{0} {1} ({2})",
				no, subject, resCount);
		}
		
		/// <summary>
		/// 現在のインスタンスとobjを比較
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		public int CompareTo(object obj)
		{
			ThreadHeader h = obj as ThreadHeader;
			if (h == null) return 1;

			return String.Compare(Url, h.Url, true);
		}
	}
}
