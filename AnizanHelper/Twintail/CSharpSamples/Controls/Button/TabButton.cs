// TabButton.cs

using System;
using System.Drawing;
using System.Drawing.Design;
using System.Collections;
using System.ComponentModel;
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
			public int Count {
				get {
					return innerList.Count;
				}
			}

			/// <summary>
			/// 指定したインデックスのタブを取得または設定します。
			/// </summary>
			public TabButton this[int index]
			{
				get {
					return (TabButton)innerList[index];
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
					throw new ArgumentNullException("button");
				
				if (button.parent != null)
					throw new ArgumentException("このボタンは既に他のタブコントロールに登録されています。");

				int index = innerList.Add(button);

				button.parent = parent;
				button.imageList = parent.ImageList;
				parent.UpdateButtons();

				return index;
			}

			/// <summary>
			/// コレクションの末尾に array を追加します。
			/// </summary>
			/// <param name="array"></param>
			public void AddRange(TabButton[] array)
			{
				foreach (TabButton button in array)
					Add(button);
			}

			/// <summary>
			/// コレクション内の index 番目に button を挿入します。
			/// </summary>
			/// <param name="index">0 から始まるコレクション内インデックス。</param>
			/// <param name="button">index 番目に挿入されるボタン。</param>
			public void Insert(int index, TabButton button)
			{
				if (index < 0 || index > Count)
					throw new ArgumentOutOfRangeException("index");

				if (button.parent != null)
					throw new ArgumentException("このボタンは既に他のタブコントロールに登録されています。");

				innerList.Insert(index, button);
				
				button.parent = parent;
				button.imageList = parent.ImageList;

				parent.UpdateButtons();
			}

			/// <summary>
			/// button をコレクションから削除します。
			/// </summary>
			/// <param name="button">コレクションから削除するボタン。</param>
			public void Remove(TabButton button)
			{
				if (innerList.Contains(button))
				{
					button.parent = null;
					button.imageList = null;
				
					innerList.Remove(button);
					parent.UpdateButtons();
				}
			}

			/// <summary>
			/// コレクションから index 番目のボタンを削除します。
			/// </summary>
			/// <param name="index">削除するボタンのインデックス。</param>
			public void RemoveAt(int index)
			{
				if (index < 0 || index >= Count)
					throw new ArgumentOutOfRangeException("index");

				TabButton button = this[index];
				button.parent = null;
				button.imageList = null;

				innerList.RemoveAt(index);
				parent.UpdateButtons();
			}

			public bool Contains(TabButton button)
			{
				return innerList.Contains(button);
			}

			public int IndexOf(TabButton button)
			{
				return innerList.IndexOf(button);
			}

			/// <summary>
			/// 登録されているボタンをすべて削除します。
			/// </summary>
			public void Clear()
			{
				foreach (TabButton button in innerList)
				{
					button.parent = null;
					button.imageList = null;
				}
				innerList.Clear();
				parent.UpdateButtons();
			}

			/// <summary>
			/// button の位置を target の前に移動します。
			/// </summary>
			/// <param name="target"></param>
			/// <param name="button"></param>
			public void InsertBefore(TabButton target, TabButton button)
			{
				if (target == button)
					return;

				if (target.parent == null)
					throw new ArgumentException("target に親が存在しません。");

				if (button.parent == null)
					throw new ArgumentException("button に親が存在しません");

				if (target.parent != button.parent)
					throw new ArgumentException("target と button の親が違います。");

				int newIndex;

				if (target.Index < button.Index)
				{
					newIndex = target.Index;
				}
				else {
					newIndex = target.Index-1;
				}

				innerList.Remove(button);
				innerList.Insert(newIndex, button);

				parent.UpdateButtons();
			}

			/// <summary>
			/// TabButtonCollection のセクションの列挙子を返します。
			/// </summary>
			/// <returns>IEnumerator</returns>
			public IEnumerator GetEnumerator()
			{
				return innerList.GetEnumerator();
			}

			#region ICollection
			/// <summary>
			/// 子のコレクションへのアクセスが同期されているかどうかを判断します。
			/// </summary>
			bool ICollection.IsSynchronized {
				get {
					return innerList.IsSynchronized;
				}
			}

			/// <summary>
			/// 子のコレクションへのアクセスを同期するために使用するオブジェクトを取得します。
			/// </summary>
			object ICollection.SyncRoot {
				get {
					return innerList.SyncRoot;
				}
			}

			/// <summary>
			/// このインスタンスを array にコピーします。
			/// </summary>
			/// <param name="array"></param>
			/// <param name="index"></param>
			void ICollection.CopyTo(Array array, int index)
			{
				innerList.CopyTo(array, index);
			}
			#endregion

			#region IList
			bool IList.IsReadOnly {
				get {
					return innerList.IsReadOnly;
				}
			}
			bool IList.IsFixedSize {
				get {
					return innerList.IsFixedSize;
				}
			}
			object IList.this[int index] {
				set {
					throw new NotSupportedException();
				}
				get {
					return this[index];
				}
			}
			int IList.Add(object obj)
			{
				return Add((TabButton)obj);
			}
			bool IList.Contains(object obj)
			{
				return innerList.Contains(obj);
			}
			int IList.IndexOf(object obj)
			{
				return innerList.IndexOf(obj);
			}
			void IList.Insert(int index, object obj)
			{
				Insert(index, (TabButton)obj);
			}
			void IList.Remove(object obj)
			{
				Remove((TabButton)obj);
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
		private string text = String.Empty;
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
		public string Text {
			set {
				text = value;
				Update(true);
			}
			get {
				return text;
			}
		}

		/// <summary>
		/// このボタンのインデックスを取得します。
		/// </summary>
		[Browsable(false)]
		public int Index {
			get {
				if (parent != null)
					return parent.Buttons.IndexOf(this);

				return -1;
			}
		}

		[Browsable(false)]
		public ImageList ImageList {
			get {
				return imageList;
			}
		}

		/// <summary>
		/// ImageList のインデックスを取得または設定します。
		/// </summary>
		[DefaultValue(-1)]
		[TypeConverter(typeof(ImageIndexConverter))]
		[Editor("System.Windows.Forms.Design.ImageIndexEditor", typeof(UITypeEditor))]   
		public int ImageIndex {
			set {
				if (imageIndex != value)
				{
					imageIndex = value;
					Update(false);
				}
			}
			get {
				return imageIndex;
			}
		}

		/// <summary>
		/// このボタンが選択されていれば true、そうでなければ false を返します。
		/// </summary>
		[Browsable(false)]
		public bool IsSelected {
			get {
				if (parent != null)
					return parent.Selected.Equals(this);

				return false;
			}
		}

		/// <summary>
		/// このボタンの Rectangle 座標を取得します。
		/// </summary>
		[Browsable(false)]
		public Rectangle Bounds {
			get {
				return bounds;
			}
		}

		/// <summary>
		/// アクティブなタブの文字色を取得または設定します。
		/// </summary>
		[DefaultValue(typeof(Color), "ControlText")]
		public Color ActiveForeColor {
			set {
				activeForeColor = value;
				Update(false);
			}
			get {
				return activeForeColor;
			}
		}

		/// <summary>
		/// アクティブなタブの背景色を取得または設定します。
		/// </summary>
		[DefaultValue(typeof(Color), "ControlLightLight")]
		public Color ActiveBackColor {
			set {
				activeBackColor = value;
				Update(false);
			}
			get {
				return activeBackColor;
			}
		}

		/// <summary>
		/// 非アクティブなタブの文字色を取得または設定します。
		/// </summary>
		[DefaultValue(typeof(Color), "ControlText")]
		public Color InactiveForeColor {
			set {
				inactiveForeColor = value;
				Update(false);
			}
			get {
				return inactiveForeColor;
			}
		}

		/// <summary>
		/// 非アクティブなタブの背景色を取得または設定します。
		/// </summary>
		[DefaultValue(typeof(Color), "Control")]
		public Color InactiveBackColor {
			set {
				inactiveBackColor = value;
				Update(false);
			}
			get {
				return inactiveBackColor;
			}
		}

		/// <summary>
		/// アクティブな表示テキストのフォントファミリーを取得または設定します。
		/// </summary>
		public FontFamily ActiveFontFamily {
			set {
				activeFontFamily = value;
				Update(true);
			}
			get {
				return activeFontFamily;
			}
		}

		/// <summary>
		/// アクティブな表示テキストのフォントスタイルを取得または設定します。
		/// </summary>
		[DefaultValue(typeof(FontStyle), "Regular")]
		public FontStyle ActiveFontStyle {
			set {
				activeFontStyle = value;
				Update(true);
			}
			get {
				return activeFontStyle;
			}
		}

		/// <summary>
		/// 非アクティブな表示テキストのフォントファミリーを取得または設定します。
		/// </summary>
		public FontFamily InactiveFontFamily {
			set {
				inactiveFontFamily = value;
				Update(true);
			}
			get {
				return inactiveFontFamily;
			}
		}

		/// <summary>
		/// 非アクティブな表示テキストのフォントスタイルを取得または設定します。
		/// </summary>
		[DefaultValue(typeof(FontStyle), "Regular")]
		public FontStyle InactiveFontStyle {
			set {
				inactiveFontStyle = value;
				Update(true);
			}
			get {
				return inactiveFontStyle;
			}
		}

		/// <summary>
		/// タグを取得または設定します。
		/// </summary>
		public object Tag {
			set {
				tag = value;
			}
			get {
				return tag;
			}
		}

		public TabButton()
		{
		}

		public TabButton(string text)
		{
			Text = text;
		}

		public TabButton(string text, int imageIndex)
		{
			Text = text;
			ImageIndex = imageIndex;
		}

		public TabButton(TabButton button)
		{
			Text = button.Text;
			ImageIndex = button.ImageIndex;

			ActiveForeColor = button.ActiveForeColor;
			ActiveBackColor = button.ActiveBackColor;
			ActiveFontFamily = button.ActiveFontFamily;
			ActiveFontStyle = button.ActiveFontStyle;

			InactiveForeColor = button.InactiveForeColor;
			InactiveBackColor = button.InactiveBackColor;
			InactiveFontFamily = button.InactiveFontFamily;
			InactiveFontStyle = button.InactiveFontStyle;
		}

		/// <summary>
		/// ボタンの状態が更新されたことを親コントロールに通知します。
		/// </summary>
		/// <param name="all"></param>
		private void Update(bool all)
		{
			if (parent == null)
				return;

			if (all) parent.UpdateButtons();
			else     parent.UpdateButton(this);
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
