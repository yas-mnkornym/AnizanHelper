// CSharpToolBarButton.cs

namespace CSharpSamples
{
	using System;
	using System.Collections;
	using System.ComponentModel;
	using System.Drawing.Design;
	using System.Drawing;
	using System.Windows.Forms;

	/// <summary>
	/// CSharpToolBar のボタンを表す
	/// </summary>
	[DesignTimeVisible(false)]
	public class CSharpToolBarButton : Component, ICloneable
	{
		#region InnerClass
		/// <summary>
		/// CSharpToolBarButtonを格納するコレクション
		/// </summary>
		public class CSharpToolBarButtonCollection : ICollection, IList, IEnumerable
		{
			private CSharpToolBar toolBar;
			private ArrayList innerList;

			/// <summary>
			/// コレクションに格納されているボタン数を取得
			/// </summary>
			public int Count
			{
				get
				{
					return innerList.Count;
				}
			}

			/// <summary>
			/// 指定したインデックスのボタンを取得または設定
			/// </summary>
			public CSharpToolBarButton this[int index]
			{
				get
				{
					return innerList[index] as CSharpToolBarButton;
				}
			}

			/// <summary>
			/// CSharpToolBarButtonCollectionクラスのインスタンスを初期化
			/// </summary>
			/// <param name="toolbar">このコレクションの親になるCSharpToolBarクラスのインスタンス</param>
			internal CSharpToolBarButtonCollection(CSharpToolBar toolbar)
			{
				if (toolbar == null)
					throw new ArgumentNullException("toolbar");

				toolBar = toolbar;
				innerList = new ArrayList();
			}

			/// <summary>
			/// ツールバーの末尾にボタンを追加
			/// </summary>
			/// <param name="button">ツールバーに追加するボタン</param>
			/// <returns>ボタンが追加されたコレクション内のインデックス</returns>
			public int Add(CSharpToolBarButton button)
			{
				if (button == null)
				{
					throw new ArgumentNullException("button");
				}
				if (button.toolBar != null)
				{
					throw new ArgumentException("このボタンは既に他のツールバーに登録されています");
				}

				int index = innerList.Add(button);

				button.toolBar = toolBar;
				button.imageList = toolBar.ImageList;

				toolBar.UpdateButtons();

				return index;
			}

			/// <summary>
			/// ツールバーにCSharpToolBarButtonの配列を追加
			/// </summary>
			/// <param name="buttons"></param>
			public void AddRange(CSharpToolBarButton[] buttons)
			{
				foreach (CSharpToolBarButton but in buttons)
					Add(but);
			}

			/// <summary>
			/// ツールバーの指定したインデックスの位置にボタンを挿入
			/// </summary>
			/// <param name="index">buttonを挿入する0から始まるインデックス番号</param>
			/// <param name="button">挿入するCSharpToolBarButton</param>
			public void Insert(int index, CSharpToolBarButton button)
			{
				if (index < 0 || index > Count)
					throw new ArgumentOutOfRangeException("index");

				button.imageList = toolBar.ImageList;
				button.toolBar = toolBar;

				innerList.Insert(index, button);
				toolBar.UpdateButtons();
			}

			/// <summary>
			/// ボタンをツールバーから削除
			/// </summary>
			/// <param name="button">ツールバーから削除するCSharpToolBarButton</param>
			public void Remove(CSharpToolBarButton button)
			{
				int index = innerList.IndexOf(button);
				if (index >= 0)
				{
					button.toolBar = null;
					button.imageList = null;

					innerList.Remove(button);
					toolBar.UpdateButtons();
				}
			}

			/// <summary>
			/// ツールバーから指定したインデックスにあるボタンを削除
			/// </summary>
			/// <param name="index">削除するCSharpToolBarButtonのインデックス</param>
			public void RemoveAt(int index)
			{
				if (index < 0 || index >= Count)
					throw new ArgumentOutOfRangeException("index");

				CSharpToolBarButton button = this[index];
				button.toolBar = null;
				button.imageList = null;

				innerList.RemoveAt(index);
				toolBar.UpdateButtons();
			}

			/// <summary>
			/// ツールバーに格納されているボタンをすべて削除
			/// </summary>
			public void Clear()
			{
				foreach (CSharpToolBarButton button in innerList)
				{
					button.toolBar = null;
					button.imageList = null;
				}
				innerList.Clear();
				toolBar.UpdateButtons();
			}

			/// <summary>
			/// button の 順番を newIndex に変更します。
			/// </summary>
			/// <param name="button"></param>
			/// <param name="newIndex"></param>
			public void ChangeIndex(CSharpToolBarButton button, int newIndex)
			{
				if (button.toolBar == null)
					throw new ArgumentException("button に親が存在しません");

				if (newIndex < 0 || newIndex > Count)
					throw new ArgumentOutOfRangeException();

				if (button.Index == newIndex)
					return;

				if (button.Index < newIndex)
					newIndex -= 1;

				innerList.Remove(button);
				innerList.Insert(newIndex, button);

				toolBar.UpdateButtons();
			}

			public int IndexOf(CSharpToolBarButton button)
			{
				return innerList.IndexOf(button);
			}

			/// <summary>
			/// CSharpToolBarButtonCollectionのセクションの列挙子を返す
			/// </summary>
			/// <returns>IEnumerator</returns>
			public IEnumerator GetEnumerator()
			{
				return innerList.GetEnumerator();
			}

			#region ICollection
			/// <summary>
			/// 子のコレクションへのアクセスが同期されているかどうかを判断
			/// </summary>
			bool ICollection.IsSynchronized
			{
				get
				{
					return innerList.IsSynchronized;
				}
			}

			/// <summary>
			/// 子のコレクションへのアクセスを同期するために使用するオブジェクトを取得
			/// </summary>
			object ICollection.SyncRoot
			{
				get
				{
					return innerList.SyncRoot;
				}
			}

			/// <summary>
			/// このインスタンスをarrayにコピー
			/// </summary>
			/// <param name="array"></param>
			/// <param name="index"></param>
			void ICollection.CopyTo(Array array, int index)
			{
				innerList.CopyTo(array, index);
			}
			#endregion

			#region IList
			bool IList.IsReadOnly
			{
				get
				{
					return innerList.IsReadOnly;
				}
			}
			bool IList.IsFixedSize
			{
				get
				{
					return innerList.IsFixedSize;
				}
			}
			object IList.this[int index]
			{
				set
				{
					throw new NotSupportedException();
				}
				get
				{
					return this[index];
				}
			}
			int IList.Add(object obj)
			{
				return this.Add((CSharpToolBarButton)obj);
			}
			bool IList.Contains(object obj)
			{
				return innerList.Contains((CSharpToolBarButton)obj);
			}
			int IList.IndexOf(object obj)
			{
				return this.IndexOf((CSharpToolBarButton)obj);
			}
			void IList.Insert(int index, object obj)
			{
				this.Insert(index, (CSharpToolBarButton)obj);
			}
			void IList.Remove(object obj)
			{
				this.Remove((CSharpToolBarButton)obj);
			}
			void IList.RemoveAt(int index)
			{
				this.RemoveAt(index);
			}
			#endregion
		}
		#endregion

		internal Rectangle bounds;
		private CSharpToolBar toolBar;

		private CSharpToolBarButtonStyle style;
		internal ImageList imageList;
		private string text;
		private int imageIndex;
		private object tag;

		/// <summary>
		/// このボタンが格納されているツールバーを取得
		/// </summary>
		public CSharpToolBar ToolBar
		{
			get
			{
				return toolBar;
			}
		}

		/// <summary>
		/// このボタンのRectangle座標を取得
		/// </summary>
		public Rectangle Bounds
		{
			get
			{
				return bounds;
			}
		}

		/// <summary>
		/// このボタンのインデックス番号を取得
		/// </summary>
		public int Index
		{
			get
			{
				if (toolBar != null)
				{
					return toolBar.Buttons.IndexOf(this);
				}
				return -1;
			}
		}

		/// <summary>
		/// ツールバーボタンのスタイル形式を取得または設定
		/// </summary>
		public CSharpToolBarButtonStyle Style
		{
			set
			{
				if (style != value)
				{
					style = value;
					Update();
				}
			}
			get
			{
				return style;
			}
		}

		/// <summary>
		/// ボタンに表示されるテキストを取得または設定
		/// </summary>
		public string Text
		{
			set
			{
				if (text == null)
					throw new ArgumentNullException("Text");

				text = value;
				Update();
			}
			get
			{
				return text;
			}
		}

		[Browsable(false)]
		public ImageList ImageList
		{
			get
			{
				return imageList;
			}
		}

		/// <summary>
		/// イメージリストのアイコン番号を取得または設定
		/// </summary>
		[DefaultValue(-1)]
		[TypeConverter(typeof(ImageIndexConverter))]
		[Editor("System.Windows.Forms.Design.ImageIndexEditor", typeof(UITypeEditor))]
		public int ImageIndex
		{
			set
			{
				if (imageIndex != value)
				{
					imageIndex = value;
					Update();
				}
			}
			get
			{
				return imageIndex;
			}
		}

		/// <summary>
		/// タグを取得または設定
		/// </summary>
		public object Tag
		{
			set
			{
				tag = value;
			}
			get
			{
				return tag;
			}
		}

		/// <summary>
		/// CSharpToolBarButtonクラスのインスタンスを初期化
		/// </summary>
		public CSharpToolBarButton()
		{
			// 
			// TODO: コンストラクタ ロジックをここに追加してください。
			//
			imageIndex = -1;
			bounds = Rectangle.Empty;
			text = String.Empty;
			style = CSharpToolBarButtonStyle.Button;
			toolBar = null;
			tag = null;
		}

		/// <summary>
		/// CSharpToolBarButtonクラスのインスタンスを初期化
		/// </summary>
		/// <param name="text">表示されるボタンテキスト</param>
		public CSharpToolBarButton(string text)
			: this()
		{
			if (text == null)
				throw new ArgumentNullException("text");

			this.text = text;
		}

		/// <summary>
		/// CSharpToolBarButtonクラスのインスタンスを初期化
		/// </summary>
		/// <param name="text">表示されるボタンテキスト</param>
		/// <param name="imageIndex">イメージリストのアイコン番号</param>
		public CSharpToolBarButton(string text, int imageIndex)
			: this(text)
		{
			this.imageIndex = imageIndex;
		}

		/// <summary>
		/// CSharpToolBarButtonクラスのインスタンスを初期化
		/// </summary>
		/// <param name="button">コピー元のボタン</param>
		private CSharpToolBarButton(CSharpToolBarButton button)
			: this()
		{
			if (button == null)
				throw new ArgumentNullException("button");

			this.text = button.text;
			this.imageIndex = button.imageIndex;
			this.style = button.style;
			this.tag = button.tag;
		}

		/// <summary>
		/// このインスタンスのコピーを作成
		/// </summary>
		/// <returns></returns>
		public object Clone()
		{
			CSharpToolBarButton clone = new CSharpToolBarButton(this);
			return clone;
		}

		/// <summary>
		/// 親のツールバーに更新されたことを通知します。
		/// </summary>
		protected void Update()
		{
			if (toolBar != null)
				toolBar.UpdateButtons();
		}
	}
}
