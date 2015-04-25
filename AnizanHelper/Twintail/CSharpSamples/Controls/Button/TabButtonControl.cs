// TabButtonControl.cs

using System;
using System.Drawing;
using System.Drawing.Design;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Windows.Forms;

namespace CSharpSamples
{
	using TabButtonCollection = TabButton.TabButtonCollection;

	/// <summary>
	/// TabControl のタブ部分だけの機能を持つクラスです。
	/// </summary>
	[DefaultEvent("SelectedChanged")]
	public class TabButtonControl : Control
	{
		private TabButtonCollection buttons;

		private TabButtonStyle buttonStyle;
		private Border3DStyle borderStyle;
		private ImageList imageList;
		private Size buttonSize;
		private Color hotTrackColor;
		private bool hotTrack;
		private bool autoAdjustControlHeight;
		private bool autoAdjustButtonSize;
		private bool wrappable;

		private TabButton selected = null;		// 現在選択されている TabButton
		private TabButton focused = null;		// 直前にクリックされた TabButton
		private TabButton hot = null;			// マウス直下にある TabButton

		private bool disposed = false;

		protected override Size DefaultSize {
			get {
				return new Size(200, 50);
			}
		}

		/// <summary>
		/// 登録されているボタンのコレクションを返します。
		/// </summary>
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[Editor(typeof(CollectionEditor), typeof(UITypeEditor))]
		public TabButtonCollection Buttons {
			get {
				return buttons;
			}
		}

		/// <summary>
		/// ボタンに表示する ImageList を取得または設定します。
		/// </summary>
		public ImageList ImageList {
			set {
				imageList = value;

				foreach (TabButton b in buttons)
					b.imageList = value;

				UpdateButtons();
			}
			get { return imageList; }
		}

		/// <summary>
		/// ボタンのスタイルを表す TabButtonStyle 列挙体を取得または設定します。
		/// </summary>
		[DefaultValue(typeof(TabButtonStyle), "Button")]
		public TabButtonStyle Style {
			set {
				buttonStyle = value;
				Refresh();
			}
			get { return buttonStyle; }
		}

		/// <summary>
		/// タブコントロールの境界線を取得または設定します。
		/// </summary>
		[DefaultValue(typeof(Border3DStyle), "Adjust")]
		public Border3DStyle BorderStyle {
			set {
				if (borderStyle != value) {
					borderStyle = value;
					Refresh();
				}
			}
			get { return borderStyle; }
		}

		/// <summary>
		/// タブが一行に収まらないときに、
		/// 次の行に折り返すかどうかを示す値を取得または設定します。
		/// </summary>
		[DefaultValue(true)]
		public bool Wrappable {
			set {
				if (wrappable != value) 
				{
					wrappable = value;
					throw new NotImplementedException();
				}
			}
			get { return wrappable; }
		}

		/// <summary>
		/// ツールバーボタンの固定サイズを取得または設定します。
		/// このプロパティを有効にするには AutoAdjustButtonSize プロパティが false に設定されている必要があります。
		/// </summary>
		[DefaultValue(typeof(Size), "80, 20")]
		public Size ButtonSize {
			set {
				buttonSize = value;
				UpdateButtons();
			}
			get { return buttonSize; }
		}

		/// <summary>
		/// HotTrack プロパティが有効になっている場合に、変化する色を設定します。
		/// </summary>
		[DefaultValue(typeof(Color), "ControlDark")]
		public Color HotTrackColor {
			set {
				hotTrackColor = value;
			}
			get {
				return hotTrackColor;
			}
		}

		/// <summary>
		/// マウスをタブの上に置いたとき、
		/// 外観が変化するかどうかを示す値を取得または設定します。
		/// </summary>
		[DefaultValue(true)]
		public bool HotTrack {
			set {
				hotTrack = value;
			}
			get { return hotTrack; }
		}
	
		/// <summary>
		/// コントロールの高さを自動で調整するかどうかを取得または設定します。
		/// </summary>
		[DefaultValue(true)]
		public bool AutoAdjustControlHeight {
			set {
				if (autoAdjustControlHeight != value)
				{
					autoAdjustControlHeight = value;
					UpdateButtons();
				}
			}
			get { return autoAdjustControlHeight; }

		}
		/// <summary>
		/// タブボタンのサイズを自動で調整するかどうかを取得または設定します。
		/// </summary>
		[DefaultValue(true)]
		public bool AutoAdjustButtonSize {
			set {
				if (autoAdjustButtonSize != value)
				{
					autoAdjustButtonSize = value;
					UpdateButtons();
				}
			}
			get { return autoAdjustButtonSize; }
		}

		/// <summary>
		/// 選択されている TabButton を取得または設定します。
		/// </summary>
		[Browsable(false)]
		public TabButton Selected {
			set {
				if (value == null)
					throw new ArgumentNullException();

				SetSelected(value);
			}
			get {
				return selected;
			}
		}

		/// <summary>
		/// 選択されているタブが変更されたときに発生します。
		/// </summary>
		public event TabButtonEventHandler SelectedChanged;

		/// <summary>
		/// TabButtonControl クラスのインスタンスを初期化。
		/// </summary>	
		public TabButtonControl()
		{
			// 
			// TODO: コンストラクタ ロジックをここに追加してください。
			//
			buttons = new TabButtonCollection(this);
			borderStyle = Border3DStyle.Adjust;
			buttonStyle = TabButtonStyle.Flat;
			autoAdjustButtonSize = true;
			autoAdjustControlHeight = true;
			wrappable = true;
			imageList = null;
			hotTrack = true;
			hotTrackColor = SystemColors.ControlDark;
			buttonSize = new Size(80, 20);
			
			// ちらつきを押さえるために各スタイルを設定
			SetStyle(ControlStyles.AllPaintingInWmPaint |
				ControlStyles.DoubleBuffer |
				ControlStyles.UserPaint, true);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (!disposed)
				{
					disposed = true;
				}
			}
			base.Dispose(disposing);
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			base.OnPaint(e);

			foreach (TabButton button in buttons)
			{
				if (button.Bounds.IntersectsWith(e.ClipRectangle))
					DrawButton(e.Graphics, button);
			}

			ControlPaint.DrawBorder3D(e.Graphics, ClientRectangle, borderStyle);
		}

		protected override void OnSizeChanged(EventArgs e)
		{
			base.OnSizeChanged(e);
			UpdateButtons();
		}

		protected override void OnMouseDown(MouseEventArgs e)
		{
			base.OnMouseDown(e);

			if (e.Button == MouseButtons.Left)
			{
				TabButton button = GetButtonAt(new Point(e.X, e.Y));

				if (button != null)
				{
					focused = button;
					SetSelected(focused);
				}
			}
		}

		protected override void OnMouseMove(MouseEventArgs e)
		{
			base.OnMouseMove(e);

			TabButton button = GetButtonAt(new Point(e.X, e.Y));

			if (button != null)
			{
				Region region = new Region(button.Bounds);
				if (hot != null) region.Union(hot.Bounds);

				hot = button;

				Invalidate(region);
				Update();
			}
			else if (hot != null)
			{
				Rectangle rc = hot.Bounds;
				hot = null;

				Invalidate(rc);
				Update();
			}

		}

		protected override void OnMouseUp(MouseEventArgs e)
		{
			base.OnMouseUp(e);

			if (e.Button == MouseButtons.Left)
			{
				TabButton button = GetButtonAt(new Point(e.X, e.Y));

				if (button != null && focused != null && focused != button)
					buttons.InsertBefore(focused, button);
			}
			focused = null;
		}

		protected override void OnMouseLeave(EventArgs e)
		{
			base.OnMouseLeave(e);

			if (hot != null)
			{
				Rectangle rc = hot.Bounds;
				hot = null;

				Invalidate(rc);
				Update();
			}
		}

		protected override void OnMouseHover(EventArgs e)
		{
			base.OnMouseHover(e);
		}

		/// <summary>
		/// 指定したボタンの状態を更新します。
		/// </summary>
		/// <param name="button"></param>
		internal void UpdateButton(TabButton button)
		{
			if (button != null)
			{
				Invalidate(button.Bounds);
				Update();
			}
		}

		/// <summary>
		/// すべてのボタンの状態を更新します。
		/// </summary>
		internal void UpdateButtons()
		{
			if (buttons.Count > 0 && selected == null)
				selected = buttons[0];

			else if (buttons.Count == 0)
				selected = null;

			using (Graphics g = CreateGraphics())
			{
				// すべてのボタンの座標を更新
				UpdateButtonRect(g);
			}

			int height = 0;

			foreach (TabButton button in buttons)
				height = Math.Max(height, button.Bounds.Bottom);	

			if (AutoAdjustControlHeight)
			{
				this.Height = height + SystemInformation.Border3DSize.Height;
			}

			Refresh();
		}

		/// <summary>
		/// SelectedChanged イベントを発生させます。
		/// </summary>
		/// <param name="button"></param>
		private void OnSelectedChanged(TabButton button)
		{
			if (SelectedChanged != null)
				SelectedChanged(this, new TabButtonEventArgs(button));
		}

		private void SetSelected(TabButton button)
		{
			if (selected != null)
			{
				Region region = new Region(selected.Bounds);
				region.Union(button.Bounds);

				selected = button;
				Invalidate(region);
			}
			else {
				selected = button;
				Invalidate(selected.Bounds);
			}

			Update();

			OnSelectedChanged(selected);
		}

		private void DrawButton(Graphics g, TabButton button)
		{
			StringFormat format = StringFormat.GenericDefault;
			format.Alignment = StringAlignment.Center;
			format.LineAlignment = StringAlignment.Center;
			format.FormatFlags = StringFormatFlags.NoWrap;

			Rectangle bounds = button.Bounds;
			Size imgSize = (imageList != null) ? imageList.ImageSize : new Size(0, 0);

			int margin = (button.ImageIndex != -1) ? 5 : 0;
			Rectangle imageRect = new Rectangle(bounds.X + margin, bounds.Y + bounds.Height / 2 - imgSize.Height / 2, imgSize.Width, imgSize.Height);
			Rectangle textRect = new Rectangle(imageRect.Right, bounds.Y, bounds.Width - imageRect.Width - margin, bounds.Height);

			// 境界線を描画
			DrawButtonBorder(g, button);
			
			// ホットな背景色を描画
			if (hotTrack && button.Equals(hot))
			{
				Rectangle rc = button.Bounds;
				Size size = SystemInformation.Border3DSize;

				rc.Width -= 2;
				rc.Height -= 2;

				using (Brush b = new SolidBrush(Color.FromArgb(50, HotTrackColor)))
					g.FillRectangle(b, rc);

				using (Pen pen = new Pen(HotTrackColor))
					g.DrawRectangle(pen, rc);
			}
			else {
				// 通常の背景色を描画			
				using (Brush brush = new SolidBrush(button.IsSelected ? 
						button.ActiveBackColor : button.InactiveBackColor))
				{
					Rectangle rc = button.Bounds;
					Size border = SystemInformation.Border3DSize;

					rc.Inflate(-border.Width, -border.Height);

					g.FillRectangle(brush, rc);
				}
			}

			// アイコンを描画
			if (imageList != null &&
				button.ImageIndex >= 0 && button.ImageIndex < imageList.Images.Count)
			{
				g.DrawImage(imageList.Images[button.ImageIndex], imageRect);
			}

			// テキストを描画
			Brush foreBrush = new SolidBrush(button.IsSelected ?
					button.ActiveForeColor : button.InactiveForeColor);

			Font textFont = button.IsSelected ? 
				new Font(button.ActiveFontFamily, Font.Size, button.ActiveFontStyle) :
				new Font(button.InactiveFontFamily, Font.Size, button.InactiveFontStyle);

			g.DrawString(button.Text, textFont, foreBrush, textRect, format);
			
			textFont.Dispose();
			foreBrush.Dispose();
		}

		private void DrawButtonBorder(Graphics g, TabButton button)
		{
			switch (buttonStyle)
			{
			case TabButtonStyle.Flat:
				// 平らなタブコントロール
				if (button.IsSelected)
				{
					ControlPaint.DrawBorder3D(g, button.Bounds, Border3DStyle.Sunken);
				}

				// タブとタブの間に境界線を描画
				Size border = SystemInformation.Border3DSize;
				Rectangle rect = button.Bounds;
				rect.X = rect.Right + border.Width / 2;
				rect.Width = border.Width;
				ControlPaint.DrawBorder3D(g, rect, Border3DStyle.Etched, Border3DSide.Right);
				break;

			case TabButtonStyle.Button:
				// ボタン式タブコントロール
				if (button.IsSelected)
				{
					ControlPaint.DrawBorder3D(g, button.Bounds, Border3DStyle.Sunken);
				}
				else {
					ControlPaint.DrawButton(g,
						button.Bounds, ButtonState.Normal);
				}
				break;
			}
		}

		/// <summary>
		/// すべてのボタンの Rectangle 座標を更新します。
		/// </summary>
		/// <param name="g"></param>
		/// <param name="target"></param>
		/// <returns></returns>
		private void UpdateButtonRect(Graphics g)
		{
			Size borderSize = SystemInformation.Border3DSize;
			Rectangle rect = new Rectangle(borderSize.Width, borderSize.Height,0,0);
			int height = 0;

			foreach (TabButton button in buttons)
			{
				Size size = AutoAdjustButtonSize ?
					GetButtonSize(g, button) : ButtonSize;

				rect.Width = size.Width;
				rect.Height = size.Height;

				height = Math.Max(height, size.Height);

				if (rect.Right > Width)
				{
					if (Wrappable)
					{
						rect.X = borderSize.Width;
						rect.Y += height + 2;
					}
				}

				button.bounds = rect;

				rect.X += size.Width + SystemInformation.Border3DSize.Width * 2;
			}
		}

		/// <summary>
		/// 指定したボタンのサイズを返します。
		/// </summary>
		/// <param name="g"></param>
		/// <param name="button"></param>
		/// <returns></returns>
		private Size GetButtonSize(Graphics g, TabButton button)
		{
			Font tempFont = new Font(
				button.IsSelected ? button.ActiveFontFamily : button.InactiveFontFamily, Font.Size,
				button.IsSelected ? button.ActiveFontStyle : button.InactiveFontStyle);

			Size itemSize = g.MeasureString(button.Text, tempFont).ToSize();

			tempFont.Dispose();

			// ちょっとずれるので適当に調整
			itemSize.Width += 8; 
			itemSize.Height += 5;

			// アイコンが存在すればアイコンサイズを足す
			if (button.ImageIndex != -1 && imageList != null)
			{
				Size imageSize = imageList.ImageSize;
				itemSize.Width += imageSize.Width;
				itemSize.Height = Math.Max(itemSize.Height, imageSize.Height);
			}

			return itemSize;
		}

		/// <summary>
		/// 指定した座標に有る TabButton を取得します。
		/// 存在しなければ null を返します。
		/// </summary>
		/// <param name="pt"></param>
		/// <returns></returns>
		public TabButton GetButtonAt(Point pt)
		{
			foreach (TabButton b in buttons)
			{
				if (b.Bounds.Contains(pt))
					return b;
			}
			return null;
		}
	}

	/// <summary>
	/// TabButton の外観を表す列挙体です。
	/// </summary>
	public enum TabButtonStyle
	{
		/// <summary>平らです。</summary>
		Flat,
		/// <summary>ボタンです。</summary>
		Button,
	}
}
