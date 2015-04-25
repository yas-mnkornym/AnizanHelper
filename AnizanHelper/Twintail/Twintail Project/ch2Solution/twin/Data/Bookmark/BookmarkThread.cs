// BookmarkThread.cs

namespace Twin
{
	using System;

	/// <summary>
	/// スレッドのお気に入りを表す
	/// </summary>
	public class BookmarkThread : BookmarkEntry
	{
		private ThreadHeader header;
		private BookmarkEntry parent;
		private string name;

		/// <summary>
		/// このインスタンスの親を取得または設定
		/// </summary>
		public override BookmarkEntry Parent {
			set { parent = value; }
			get { return parent; }
		}

		/// <summary>
		/// このプロパティは使用できません
		/// </summary>
		public override BookmarkEntryCollection Children {
			get { throw new NotSupportedException("Childrenプロパティはサポートしていません"); }
		}

		/// <summary>
		/// このプロパティは常にtrueを返す
		/// </summary>
		public override bool IsLeaf {
			get { return true; }
		}

		/// <summary>
		/// この気に入りスレッドの名前を取得または設定
		/// </summary>
		public override string Name {
			set {
				if (value == null)
					throw new ArgumentNullException("Name");
				name = value;
			}
			get { return name; }
		}

		/// <summary>
		/// お気に入りのスレッド情報を取得
		/// </summary>
		public ThreadHeader HeaderInfo {
			get { return header; }
		}

		/// <summary>
		/// BookmarkThreadクラスのインスタンスを初期化
		/// </summary>
		/// <param name="header"></param>
		/// <param name="id"></param>
		public BookmarkThread(ThreadHeader header, int id) : base(id)
		{
			// 
			// TODO: コンストラクタ ロジックをここに追加してください。
			//
			this.header = header;
			this.name = header.Subject;
			this.parent = null;
		}

		/// <summary>
		/// BookmarkThreadクラスのインスタンスを初期化
		/// </summary>
		/// <param name="header"></param>
		public BookmarkThread(ThreadHeader header) : this(header, -1)
		{
		}

		/// <summary>
		/// このインスタンスを親から削除
		/// </summary>
		public override void Remove()
		{
			if (parent != null)
				parent.Children.Remove(this);
		}

		/// <summary>
		/// このお気に入りエントリを複製
		/// </summary>
		/// <returns></returns>
		public override BookmarkEntry Clone()
		{
			BookmarkThread clone = new BookmarkThread(header);
			clone.name = this.name;

			return clone;
		}
	}
}
