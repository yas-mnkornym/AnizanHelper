// TabButton.cs

using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Windows.Forms;

namespace CSharpSamples
{
	/// <summary>
	/// TabButtonControl のタブボタンを表すクラスです。
	/// </summary>
	[DesignTimeVisible(false)]
	public class TabButton : Component, ICloneable
	{
		#region inner class
		/// <summary>
		/// TabButton を格納するコレクションです。
		/// </summary>
		public class TabButtonCollection : ICollection, IList, IEnumerable
		{
			private TabButtonControl parent;
			private ArrayList innerList;

			/// <summary>
			/// コレクションに格納されているタブ数を取得します。
			/// </summary>
			public int Count
			{
				get
				{
					return this.innerList.Count;
				}
			}

			/// <summary>
			/// 指定したインデックスのタブを取得または設定します。
			/// </summary>
			public TabButton this[int index]
			{
				get
				{
					return (TabButton)this.innerList[index];
				}
			}

			/// <summary>
			/// TabButtonCollectionクラスのインスタンスを初期化。
			/// </summary>
			internal TabButtonCollection(TabButtonControl parent)
			{
				this.parent = parent;
				this.innerList = new ArrayList();
			}

			/// <summary>
			/// コレクションの末尾に button を追加します。
			/// </summary>
			/// <param name="button"></param>
			/// <returns></returns>
			public int Add(TabButton button)
			{
				if (button == null)
				{
					throw new ArgumentNullException("button");
				}

				if (button.parent != null)
				{
					throw new ArgumentException("このボタンは既に他のタブコントロールに登録されています。");
				}

				int index = this.innerList.Add(button);

				button.parent = this.parent;
				button.imageList = this.parent.ImageList;
				this.parent.UpdateButtons();

				return index;
			}

			/// <summary>
			/// コレクションの末尾に array を追加します。
			/// </summary>
			/// <param name="array"></param>
			public void AddRange(TabButton[] array)
			{
				foreach (TabButton button in array)
				{
					this.Add(button);
				}
			}

			/// <summary>
			/// コレクション内の index 番目に button を挿入します。
			/// </summary>
			/// <param name="index">0 から始まるコレクション内インデックス。</param>
			/// <param name="button">index 番目に挿入されるボタン。</param>
			public void Insert(int index, TabButton button)
			{
				if (index < 0 || index > this.Count)
				{
					throw new ArgumentOutOfRangeException("index");
				}

				if (button.parent != null)
				{
					throw new ArgumentException("このボタンは既に他のタブコントロールに登録されています。");
				}

				this.innerList.Insert(index, button);

				button.parent = this.parent;
				button.imageList = this.parent.ImageList;

				this.parent.UpdateButtons();
			}

			/// <summary>
			/// button をコレクションから削除します。
			/// </summary>
			/// <param name="button">コレクションから削除するボタン。</param>
			public void Remove(TabButton button)
			{
				if (this.innerList.Contains(button))
				{
					button.parent = null;
					button.imageList = null;

					this.innerList.Remove(button);
					this.parent.UpdateButtons();
				}
			}

			/// <summary>
			/// コレクションから index 番目のボタンを削除します。
			/// </summary>
			/// <param name="index">削除するボタンのインデックス。</param>
			public void RemoveAt(int index)
			{
				if (index < 0 || index >= this.Count)
				{
					throw new ArgumentOutOfRangeException("index");
				}

				TabButton button = this[index];
				button.parent = null;
				button.imageList = null;

				this.innerList.RemoveAt(index);
				this.parent.UpdateButtons();
			}

			public bool Contains(TabButton button)
			{
				return this.innerList.Contains(button);
			}

			public int IndexOf(TabButton button)
			{
				return this.innerList.IndexOf(button);
			}

			/// <summary>
			/// 登録されているボタンをすべて削除します。
			/// </summary>
			public void Clear()
			{
				foreach (TabButton button in this.innerList)
				{
					button.parent = null;
					button.imageList = null;
				}
				this.innerList.Clear();
				this.parent.UpdateButtons();
			}

			/// <summary>
			/// button の位置を target の前に移動します。
			/// </summary>
			/// <param name="target"></param>
			/// <param name="button"></param>
			public void InsertBefore(TabButton target, TabButton button)
			{
				if (target == button)
				{
					return;
				}

				if (target.parent == null)
				{
					throw new ArgumentException("target に親が存在しません。");
				}

				if (button.parent == null)
				{
					throw new ArgumentException("button に親が存在しません");
				}

				if (target.parent != button.parent)
				{
					throw new ArgumentException("target と button の親が違います。");
				}

				int newIndex;

				if (target.Index < button.Index)
				{
					newIndex = target.Index;
				}
				else
				{
					newIndex = target.Index - 1;
				}

				this.innerList.Remove(button);
				this.innerList.Insert(newIndex, button);

				this.parent.UpdateButtons();
			}

			/// <summary>
			/// TabButtonCollection のセクションの列挙子を返します。
			/// </summary>
			/// <returns>IEnumerator</returns>
			public IEnumerator GetEnumerator()
			{
				return this.innerList.GetEnumerator();
			}

			#region ICollection
			/// <summary>
			/// 子のコレクションへのアクセスが同期されているかどうかを判断します。
			/// </summary>
			bool ICollection.IsSynchronized
			{
				get
				{
					return this.innerList.IsSynchronized;
				}
			}

			/// <summary>
			/// 子のコレクションへのアクセスを同期するために使用するオブジェクトを取得します。
			/// </summary>
			object ICollection.SyncRoot
			{
				get
				{
					return this.innerList.SyncRoot;
				}
			}

			/// <summary>
			/// このインスタンスを array にコピーします。
			/// </summary>
			/// <param name="array"></param>
			/// <param name="index"></param>
			void ICollection.CopyTo(Array array, int index)
			{
				this.innerList.CopyTo(array, index);
			}
			#endregion

			#region IList
			bool IList.IsReadOnly
			{
				get
				{
					return this.innerList.IsReadOnly;
				}
			}
			bool IList.IsFixedSize
			{
				get
				{
					return this.innerList.IsFixedSize;
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
				return this.Add((TabButton)obj);
			}
			bool IList.Contains(object obj)
			{
				return this.innerList.Contains(obj);
			}
			int IList.IndexOf(object obj)
			{
				return this.innerList.IndexOf(obj);
			}
			void IList.Insert(int index, object obj)
			{
				this.Insert(index, (TabButton)obj);
			}
			void IList.Remove(object obj)
			{
				this.Remove((TabButton)obj);
			}
			void IList.RemoveAt(int index)
			{
				this.RemoveAt(index);
			}
			#endregion
		}
		#endregion

		internal ImageList imageList;
		private TabButtonControl parent = null;
		private string text = string.Empty;
		private int imageIndex = -1;
		private object tag;

		private Color activeForeColor = SystemColors.ControlText;
		private Color activeBackColor = SystemColors.ControlLightLight;
		private FontStyle activeFontStyle = FontStyle.Regular;
		private FontFamily activeFontFamily = Control.DefaultFont.FontFamily;

		private Color inactiveForeColor = SystemColors.ControlText;
		private Color inactiveBackColor = SystemColors.Control;
		private FontStyle inactiveFontStyle = FontStyle.Regular;
		private FontFamily inactiveFontFamily = Control.DefaultFont.FontFamily;

		internal Rectangle bounds = Rectangle.Empty;

		/// <summary>
		/// タブの表示テキストを取得または設定します。
		/// </summary>
		[DefaultValue("")]
		public string Text
		{
			set
			{
				this.text = value;
				this.Update(true);
			}
			get
			{
				return this.text;
			}
		}

		/// <summary>
		/// このボタンのインデックスを取得します。
		/// </summary>
		[Browsable(false)]
		public int Index
		{
			get
			{
				if (this.parent != null)
				{
					return this.parent.Buttons.IndexOf(this);
				}

				return -1;
			}
		}

		[Browsable(false)]
		public ImageList ImageList
		{
			get
			{
				return this.imageList;
			}
		}

		/// <summary>
		/// ImageList のインデックスを取得または設定します。
		/// </summary>
		[DefaultValue(-1)]
		[TypeConverter(typeof(ImageIndexConverter))]
		[Editor("System.Windows.Forms.Design.ImageIndexEditor", typeof(UITypeEditor))]
		public int ImageIndex
		{
			set
			{
				if (this.imageIndex != value)
				{
					this.imageIndex = value;
					this.Update(false);
				}
			}
			get
			{
				return this.imageIndex;
			}
		}

		/// <summary>
		/// このボタンが選択されていれば true、そうでなければ false を返します。
		/// </summary>
		[Browsable(false)]
		public bool IsSelected
		{
			get
			{
				if (this.parent != null)
				{
					return this.parent.Selected.Equals(this);
				}

				return false;
			}
		}

		/// <summary>
		/// このボタンの Rectangle 座標を取得します。
		/// </summary>
		[Browsable(false)]
		public Rectangle Bounds
		{
			get
			{
				return this.bounds;
			}
		}

		/// <summary>
		/// アクティブなタブの文字色を取得または設定します。
		/// </summary>
		[DefaultValue(typeof(Color), "ControlText")]
		public Color ActiveForeColor
		{
			set
			{
				this.activeForeColor = value;
				this.Update(false);
			}
			get
			{
				return this.activeForeColor;
			}
		}

		/// <summary>
		/// アクティブなタブの背景色を取得または設定します。
		/// </summary>
		[DefaultValue(typeof(Color), "ControlLightLight")]
		public Color ActiveBackColor
		{
			set
			{
				this.activeBackColor = value;
				this.Update(false);
			}
			get
			{
				return this.activeBackColor;
			}
		}

		/// <summary>
		/// 非アクティブなタブの文字色を取得または設定します。
		/// </summary>
		[DefaultValue(typeof(Color), "ControlText")]
		public Color InactiveForeColor
		{
			set
			{
				this.inactiveForeColor = value;
				this.Update(false);
			}
			get
			{
				return this.inactiveForeColor;
			}
		}

		/// <summary>
		/// 非アクティブなタブの背景色を取得または設定します。
		/// </summary>
		[DefaultValue(typeof(Color), "Control")]
		public Color InactiveBackColor
		{
			set
			{
				this.inactiveBackColor = value;
				this.Update(false);
			}
			get
			{
				return this.inactiveBackColor;
			}
		}

		/// <summary>
		/// アクティブな表示テキストのフォントファミリーを取得または設定します。
		/// </summary>
		public FontFamily ActiveFontFamily
		{
			set
			{
				this.activeFontFamily = value;
				this.Update(true);
			}
			get
			{
				return this.activeFontFamily;
			}
		}

		/// <summary>
		/// アクティブな表示テキストのフォントスタイルを取得または設定します。
		/// </summary>
		[DefaultValue(typeof(FontStyle), "Regular")]
		public FontStyle ActiveFontStyle
		{
			set
			{
				this.activeFontStyle = value;
				this.Update(true);
			}
			get
			{
				return this.activeFontStyle;
			}
		}

		/// <summary>
		/// 非アクティブな表示テキストのフォントファミリーを取得または設定します。
		/// </summary>
		public FontFamily InactiveFontFamily
		{
			set
			{
				this.inactiveFontFamily = value;
				this.Update(true);
			}
			get
			{
				return this.inactiveFontFamily;
			}
		}

		/// <summary>
		/// 非アクティブな表示テキストのフォントスタイルを取得または設定します。
		/// </summary>
		[DefaultValue(typeof(FontStyle), "Regular")]
		public FontStyle InactiveFontStyle
		{
			set
			{
				this.inactiveFontStyle = value;
				this.Update(true);
			}
			get
			{
				return this.inactiveFontStyle;
			}
		}

		/// <summary>
		/// タグを取得または設定します。
		/// </summary>
		public object Tag
		{
			set
			{
				this.tag = value;
			}
			get
			{
				return this.tag;
			}
		}

		public TabButton()
		{
		}

		public TabButton(string text)
		{
			this.Text = text;
		}

		public TabButton(string text, int imageIndex)
		{
			this.Text = text;
			this.ImageIndex = imageIndex;
		}

		public TabButton(TabButton button)
		{
			this.Text = button.Text;
			this.ImageIndex = button.ImageIndex;

			this.ActiveForeColor = button.ActiveForeColor;
			this.ActiveBackColor = button.ActiveBackColor;
			this.ActiveFontFamily = button.ActiveFontFamily;
			this.ActiveFontStyle = button.ActiveFontStyle;

			this.InactiveForeColor = button.InactiveForeColor;
			this.InactiveBackColor = button.InactiveBackColor;
			this.InactiveFontFamily = button.InactiveFontFamily;
			this.InactiveFontStyle = button.InactiveFontStyle;
		}

		/// <summary>
		/// ボタンの状態が更新されたことを親コントロールに通知します。
		/// </summary>
		/// <param name="all"></param>
		private void Update(bool all)
		{
			if (this.parent == null)
			{
				return;
			}

			if (all)
			{
				this.parent.UpdateButtons();
			}
			else
			{
				this.parent.UpdateButton(this);
			}
		}

		/// <summary>
		/// インスタンスの簡易コピーを作成します。
		/// </summary>
		/// <returns></returns>
		public object Clone()
		{
			return new TabButton(this);
		}
	}
}
