// BookmarkFolder.cs

namespace Twin
{
	using System;
	using System.Collections.Generic;
	using System.Windows.Forms;

	/// <summary>
	/// お気に入りフォルダを管理
	/// </summary>
	public class BookmarkFolder : BookmarkEntry
	{
		private BookmarkEntryCollection children;
		private BookmarkEntry parent;
		private SortOrder sortorder;
		private string name;
		private bool expanded;

		/// <summary>
		/// このインスタンスの親フォルダを取得または設定
		/// </summary>
		public override BookmarkEntry Parent
		{
			set
			{
				parent = value;
			}
			get
			{
				return parent;
			}
		}

		/// <summary>
		/// このエントリに格納されているコレクションを取得
		/// </summary>
		public override BookmarkEntryCollection Children
		{
			get
			{
				return children;
			}
		}

		/// <summary>
		/// このプロパティは常にfalseを返す
		/// </summary>
		public override bool IsLeaf
		{
			get
			{
				return false;
			}
		}

		/// <summary>
		/// この気に入りフォルダの名前を取得または設定
		/// </summary>
		public override string Name
		{
			set
			{
				if (value == null)
				{
					throw new ArgumentNullException("Name");
				}
				name = value;
			}
			get
			{
				return name;
			}
		}

		/// <summary>
		/// このフォルダが展開状態かどうかを表す値を取得または設定
		/// </summary>
		public bool Expanded
		{
			set
			{
				if (expanded != value)
					expanded = value;
			}
			get
			{
				return expanded;
			}
		}

		/// <summary>
		/// BookmarkFolderクラスのインスタンスを初期化
		/// </summary>
		public BookmarkFolder()
			: this(String.Empty, -1)
		{
		}

		/// <summary>
		/// BookmarkFolderクラスのインスタンスを初期化
		/// </summary>
		public BookmarkFolder(string name, int id)
			: base(id)
		{
			this.children = new BookmarkEntryCollection(this);
			this.sortorder = SortOrder.Ascending;
			this.expanded = false;
			this.parent = null;
			this.name = name;
		}

		/// <summary>
		/// BookmarkFolderクラスのインスタンスを初期化
		/// </summary>
		/// <param name="id"></param>
		public BookmarkFolder(int id)
			: this(String.Empty, id)
		{
		}

		/// <summary>
		/// BookmarkFolderクラスのインスタンスを初期化
		/// </summary>
		public BookmarkFolder(string name)
			: this(name, -1)
		{
		}

		/// <summary>
		/// このインスタンスを親から削除
		/// </summary>
		public override void Remove()
		{
			if (parent == null)
			{
				throw new InvalidOperationException("このインスタンスに親が設定されていません");
			}
			parent.Children.Remove(this);
		}

		/// <summary>
		/// このフォルダを複製
		/// </summary>
		/// <returns></returns>
		public override BookmarkEntry Clone()
		{
			BookmarkFolder clone = new BookmarkFolder(name);
			clone.Children.AddRange(children);

			return clone;
		}

		/// <summary>
		/// 指定したソート方法で情報をソート
		/// </summary>
		/// <param name="obj"></param>
		/// <param name="order"></param>
		public void Sort(BookmarkSortObject obj)
		{
			children.Sort(new BookmarkSorter(obj, sortorder));
			sortorder = (sortorder != SortOrder.Ascending) ? SortOrder.Ascending : SortOrder.Descending;
		}

		/// <summary>
		/// 指定したソート方法で情報をソート
		/// </summary>
		/// <param name="obj"></param>
		/// <param name="order"></param>
		public void Sort(BookmarkSortObject obj, SortOrder order)
		{
			sortorder = order;
			Sort(obj);
		}

		/// <summary>
		/// このフォルダに含まれている子の数を取得
		/// </summary>
		/// <param name="includeSubChilren">サブフォルダも含める場合はtrue</param>
		/// <returns></returns>
		public int GetChildCount(bool includeSubChildren)
		{
			int result = children.Count;

			if (includeSubChildren)
				foreach (BookmarkEntry entry in children)
					if (!entry.IsLeaf)
						result += ((BookmarkFolder)entry).GetChildCount(true);

			return result;
		}

		/// <summary>
		/// このフォルダのお気に入りを取得
		/// </summary>
		/// <param name="includeSubChildren">サブフォルダも含める場合はtrue</param>
		/// <returns></returns>
		public List<ThreadHeader> GetBookmarks(bool includeSubChildren)
		{
			List<ThreadHeader> items =
				new List<ThreadHeader>();

			foreach (BookmarkEntry entry in children)
			{
				if (entry.IsLeaf)
				{
					BookmarkThread thread = (BookmarkThread)entry;
					items.Add(thread.HeaderInfo);
				}
				else if (includeSubChildren)
				{
					BookmarkFolder folder = (BookmarkFolder)entry;
					items.AddRange(folder.GetBookmarks(includeSubChildren));
				}
			}
			return items;
		}
	}
}
