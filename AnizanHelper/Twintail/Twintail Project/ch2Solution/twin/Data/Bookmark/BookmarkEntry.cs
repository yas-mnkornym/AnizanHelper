// BookmarkEntry.cs

namespace Twin
{
	using System;
	using System.Collections;

	/// <summary>
	/// お気に入りの基本抽象クラス
	/// </summary>
	public abstract class BookmarkEntry
	{
		/// <summary>
		/// すべてのお気に入りが登録されているテーブル
		/// </summary>
		private static readonly Hashtable hash;
		private static readonly Random random;

		private int id;
		private object tag;

		/// <summary>
		/// このエントリが格納されている親フォルダを取得または設定
		/// </summary>
		public abstract BookmarkEntry Parent {
			set;
			get;
		}

		/// <summary>
		/// このエントリの子コレクションを取得
		/// </summary>
		public abstract BookmarkEntryCollection Children {
			get;
		}

		/// <summary>
		/// このエントリの名前を取得または設定
		/// </summary>
		public abstract string Name {
			set;
			get;
		}

		/// <summary>
		/// このインスタンスが葉かどうかを判断
		/// </summary>
		public abstract bool IsLeaf {
			get;
		}

		/// <summary>
		/// このお気に入りを識別するIDを取得
		/// </summary>
		public int Id {
			get {
				return id;
			}
		}

		/// <summary>
		/// タグを取得または設定
		/// </summary>
		public object Tag {
			set { tag = value; }
			get { return tag; }
		}

		static BookmarkEntry()
		{
			hash = new Hashtable();
			random = new Random();
		}

		/// <summary>
		/// BookmarkEntryクラスのインスタンスを初期化
		/// </summary>
		protected BookmarkEntry()
		{
			// 
			// TODO: コンストラクタ ロジックをここに追加してください。
			//
			id = GetRandomId();
			hash[id] = this;
		}

		/// <summary>
		/// BookmarkEntryクラスのインスタンスを初期化
		/// </summary>
		/// <param name="entryid">他のお気に入りとかぶらないIdを指定。-1にするとランダムに設定される。</param>
		protected BookmarkEntry(int entryid)
		{
			if (entryid == -1)
				entryid = GetRandomId();

			if (hash.Contains(entryid))
				throw new ArgumentException("Id:" + entryid + "は他のお気に入りのIdと重複しています");

			id = entryid;
			hash[id] = this;
		}

		/// <summary>
		/// このエントリを親から削除
		/// </summary>
		public abstract void Remove();

		/// <summary>
		/// このエントリを複製
		/// </summary>
		/// <returns></returns>
		public abstract BookmarkEntry Clone();

		/// <summary>
		/// 既に登録されているお気に入りに重複しないランダムなIDを取得
		/// </summary>
		/// <returns></returns>
		protected static int GetRandomId()
		{
			int id;
			do {
				id = random.Next();
			}
			while (hash.ContainsKey(id) || id == -1);

			return id;
		}

		/// <summary>
		/// 指定したIDを持つお気に入りが登録されているかどうかを判断
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		public static bool Contains(int id)
		{
			return hash.ContainsKey(id);
		}

		/// <summary>
		/// 指定したIDを持つエントリを取得
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		public static BookmarkEntry GetEntryOf(int id)
		{
			return hash[id] as BookmarkEntry;
		}

		/// <summary>
		/// 指定したお気に入りに新しいIDを設定する
		/// </summary>
		/// <param name="entry"></param>
		/// <param name="newid"></param>
		public static void SetEntryId(BookmarkEntry entry, int newid)
		{
			if (entry.id == newid)
				return;

			if (hash.ContainsKey(newid))
				throw new ArgumentException("Id:" + newid + "は他のお気に入りと重複しています");

			entry.id = newid;

			hash.Remove(entry.id);
			hash[newid] = entry;
		}

		/// <summary>
		/// このインスタンスのハッシュ値を返す
		/// </summary>
		/// <returns></returns>
		public override int GetHashCode()
		{
			return id;
		}

		/// <summary>
		/// このインスタンスとobjが等しいかどうかを判断
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		public override bool Equals(object obj)
		{
			BookmarkEntry entry = obj as BookmarkEntry;
			return (entry != null) ? (this.id == entry.id) : false;
		}

		/// <summary>
		/// このインスタンスを文字列形式に変換
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			return Name;
		}
	}
}
