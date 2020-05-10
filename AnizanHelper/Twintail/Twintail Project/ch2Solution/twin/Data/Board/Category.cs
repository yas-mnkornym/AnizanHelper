// Category.cs

namespace Twin
{
	using System;
	using System.Collections;
	using System.Runtime.Serialization;

	/// <summary>
	/// 掲示板の板をまとめるカテゴリを表す
	/// </summary>
	public class Category
	{
		private BoardInfoCollection children;
		private string name;
		private bool isExpanded;

		/// <summary>
		/// 格納されている子エントリ数を取得
		/// </summary>
		public int Count {
			get {
				return children.Count;
			}
		}

		/// <summary>
		/// カテゴリ名を取得または設定
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
		/// このカテゴリの子エントリを取得
		/// </summary>
		public BoardInfoCollection Children {
			get {
				return children;
			}
		}

		/// <summary>
		/// フォルダが展開されているかどうかを取得または設定
		/// </summary>
		public bool IsExpanded {
			set {
				if (value != isExpanded)
					isExpanded = value;
			}
			get { return isExpanded; }
		}

		/// <summary>
		/// Categoryクラスのインスタンスを初期化
		/// </summary>
		public Category()
		{
			this.children = new BoardInfoCollection();
			this.isExpanded = false;
			this.name = String.Empty;
		}

		/// <summary>
		/// Categoryクラスのインスタンスを初期化
		/// </summary>
		/// <param name="name">カテゴリ名</param>
		public Category(string name) : this()
		{
			if (name == null) {
				throw new ArgumentNullException("name");
			}

			this.name = name;
		}

		/// <summary>
		/// ハッシュ関数
		/// </summary>
		/// <returns></returns>
		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		/// <summary>
		/// このインスタンスとobjを比較
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		public override bool Equals(object obj)
		{
			return Equals(obj as Category);
		}

		/// <summary>
		/// このインスタンスとcategoryを比較
		/// </summary>
		/// <param name="category"></param>
		/// <returns></returns>
		public bool Equals(Category category)
		{
			if (category != null &&
				category.Count == Count)
			{
				for (int i = 0; i < Count; i++)
				{
					BoardInfo board1 = category.Children[i];
					BoardInfo board2 = Children[i];
					if (!board1.Equals(board2))
						return false;
				}
				return true;
			}
			return false;
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
