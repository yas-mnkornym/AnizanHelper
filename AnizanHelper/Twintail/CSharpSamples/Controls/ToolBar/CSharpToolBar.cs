// CSharpToolBar.cs

namespace CSharpSamples
{
	using System;
	using System.Windows.Forms;
	using System.ComponentModel;
	using System.ComponentModel.Design;
	using System.Drawing;
	using System.Drawing.Design;

	using CSharpToolBarButtonCollection = 
		CSharpToolBarButton.CSharpToolBarButtonCollection;
	using System.Drawing.Imaging;
	using System.Runtime.InteropServices;

	/// <summary>
	/// C#で作ったWindowsのToolBarもどき
	/// </summary>
	[DefaultEvent("ButtonClick")]
	public class CSharpToolBar : Control
	{
		private CSharpToolBarButtonCollection buttons;
		private CSharpToolBarAppearance appearance;
		private ToolBarTextAlign textAlign;
		private Border3DStyle borderStyle;
		private ImageList imageList;
		private Size buttonSize;
		private bool autoToolBarSize;
		private bool autoAdjustSize;
		private bool wrappable;
		private bool allowDragButton;

		// ボタンの余白。
		private Rectangle _Margin = new Rectangle(2,2,2,4);

		private CSharpToolBarButton activeButton = null;
		private Rectangle tempDropLine = Rectangle.Empty;

		protected override Size DefaultSize {
			get {
				return new Size(100, 50);
			}
		}

		protected Rectangle ClientRect {
			get {
				Rectangle client = Bounds;
				client.X = client.Y = 0;

				return client;
			}
		}

		/// <summary>
		/// ツールバーのボタンが格納されているコレクションを取得
		/// </summary>
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[Editor(typeof(CollectionEditor), typeof(UITypeEditor))]
		public CSharpToolBarButtonCollection Buttons {
			get {
				return buttons;
			}
		}

		/// <summary>
		/// ツールバーの境界線を取得または設定
		/// </summary>
		[DefaultValue(typeof(Border3DStyle), "Adjust")]
		public Border3DStyle BorderStyle {
			set {
				if (borderStyle != value) {
					borderStyle = value;
					UpdateButtons();
				}
			}
			get { return borderStyle; }
		}

		/// <summary>
		/// ツールバーのボタンスタイルを取得または設定
		/// </summary>
		[DefaultValue(typeof(CSharpToolBarAppearance), "Normal")]
		public CSharpToolBarAppearance Appearance {
			set {
				if (appearance != value) {
					appearance = value;
					Refresh();
				}
			}
			get { return appearance; }
		}

		/// <summary>
		/// ボタンテキスト配置の位置を取得または設定
		/// </summary>
		[DefaultValue(typeof(ToolBarTextAlign), "Underneath")]
		public ToolBarTextAlign TextAlign {
			set {
				if (textAlign != value) {
					textAlign = value;
					UpdateButtons();
				}
			}
			get { return textAlign; }
		}

		/// <summary>
		/// イメージリストを取得または設定
		/// </summary>
		public ImageList ImageList {
			set {
				imageList = value;
				
				foreach (CSharpToolBarButton b in buttons) 
					b.imageList = value;

				UpdateButtons();
			}
			get {
				return imageList;
			}
		}

		/// <summary>
		/// ツールバーボタンの固定サイズを取得または設定。
		/// このプロパティを有効にするにはautoAdjustSizeプロパティがfalseに設定されている必要がある。
		/// </summary>
		[DefaultValue(typeof(Size), "80,25")]
		public Size ButtonSize {
			set {
				buttonSize = value;
				UpdateButtons();
			}
			get { return buttonSize; }
		}

		/// <summary>
		/// ツールバーのサイズを自動で調整するかどうかを取得または設定
		/// </summary>
		[DefaultValue(false)]
		public bool AutoToolBarSize {
			set {
				if (autoToolBarSize != value) {
					autoToolBarSize = value;
					UpdateButtons();
				}
			}
			get { return autoToolBarSize; }

		}
		/// <summary>
		/// ボタンの幅を自動で調整するかどうかを取得または設定
		/// </summary>
		[DefaultValue(true)]
		public bool AutoAdjustSize {
			set {
				if (autoAdjustSize != value) {
					autoAdjustSize = value;
					UpdateButtons();
				}
			}
			get { return autoAdjustSize; }
		}

		/// <summary>
		/// ツールバーのボタンが一行に収まらないときに
		/// 次の行に折り返すかどうかを取得または設定
		/// </summary>
		[DefaultValue(true)]
		public bool Wrappable {
			set {
				if (wrappable != value) {
					wrappable = value;
					UpdateButtons();
				}
			}
			get { return wrappable; }
		}

		/// <summary>
		/// ボタンをドラッグで移動できるかどうかを示す値を取得または設定。
		/// </summary>
		[DefaultValue(false)]
		public bool AllowDragButton {
			set {
				allowDragButton = value;
			}
			get { return allowDragButton; }
		}

		/// <summary>
		/// ツールバーのボタンがクリックされた時に発生
		/// </summary>
		public event CSharpToolBarButtonEventHandler ButtonClick;

		/// <summary>
		/// CSharpToolBarクラスのインスタンスを初期化
		/// </summary>
		public CSharpToolBar()
		{
			// 
			// TODO: コンストラクタ ロジックをここに追加してください。
			//

			buttons = new CSharpToolBarButtonCollection(this);
			appearance = CSharpToolBarAppearance.Normal;
			borderStyle = Border3DStyle.Adjust;
			textAlign = ToolBarTextAlign.Underneath;
			imageList = null;
			buttonSize = new Size(80, 25);
			autoToolBarSize = false;
			autoAdjustSize = true;
			wrappable = true;
			allowDragButton = true;

			// ちらつきを押さえるために各スタイルを設定
			SetStyle(ControlStyles.AllPaintingInWmPaint |
				ControlStyles.DoubleBuffer |
				ControlStyles.UserPaint, true);

			//this.BackColor = Color.Transparent;
			//Dock = DockStyle.Top;
		}

		void UpdateRegion()
		{/*
			var region = new Region(this.ClientRectangle);
			int clientWidth = this.ClientSize.Width;
			int clientHeight = this.ClientSize.Height;

			var bg = new Bitmap(clientWidth, clientHeight);

			// bg に文字を描画
			using (Graphics g = Graphics.FromImage(bg))
			{
				foreach (CSharpToolBarButton button in buttons)
					DrawButton(g, button, false, false);
			}

			BitmapData bitdata = bg.LockBits(this.ClientRectangle, ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
			int stride = bitdata.Stride;
			var buffer = new byte[stride * clientHeight];

			Marshal.Copy(bitdata.Scan0, buffer, 0, buffer.Length);
			bg.UnlockBits(bitdata);
			bg.Dispose();

			int line = 0;
			for (int y = 0; y < clientHeight; y++)
			{
				line = stride * y;
				for (int x = 0; x < clientWidth; x++)
				{
					if (buffer[line + x * 4 + 3] == 0)
					{
						region.Exclude(new Rectangle(x, y, 1, 1));
					}
				}
			}

			this.Region = region;*/
		}



		#region Override Events
		protected override void OnPaint(PaintEventArgs e)
		{
			base.OnPaint(e);
			
			Graphics g = e.Graphics;

			foreach (CSharpToolBarButton button in buttons)
				DrawButton(g, button, false, false);
			
			// 境界線を描画
			Rectangle rc = Bounds;

			ControlPaint.DrawBorder3D(g, ClientRectangle, borderStyle);
		}

		protected override void OnMouseEnter(EventArgs e)
		{
			base.OnMouseEnter(e);
		}

		protected override void OnMouseLeave(EventArgs e)
		{
			base.OnMouseLeave(e);
			UpdateButton(activeButton);
			activeButton = null;
		}

		protected override void OnMouseMove(MouseEventArgs e)
		{
			base.OnMouseMove(e);

			CSharpToolBarButton button = ButtonFromPoint(e.X, e.Y);

			// セパレータの場合は何もしない
			if (button != null && button.Style == CSharpToolBarButtonStyle.Separator)
				return;

			// フラット形式の場合は、浮き出る境界線を描画
			if (e.Button == MouseButtons.None)
			{
				if (button == activeButton)
					return;

				UpdateButton(activeButton);

				activeButton = button;

				if (appearance == CSharpToolBarAppearance.Flat &&
					button != null)
				{
					using (Graphics g = CreateGraphics())
						ControlPaint.DrawBorder3D(g, button.Bounds, Border3DStyle.RaisedInner);
				}
				else if (appearance == CSharpToolBarAppearance.VisualStudio &&
					button != null)
				{
					using (Graphics g = CreateGraphics())
						DrawButton(g, button, false, true);
				}

			}
			// ボタンのドラッグ操作
			else if (e.Button == MouseButtons.Left && allowDragButton)
			{
				if (ClientRect.Contains(e.X, e.Y))
				{
					DrawHorzLine(GetDropButtonIndex(e.X, e.Y));
					Cursor = Cursors.Default;
				}
				// クライアント領域から出ていればドラッグ操作を中止
				else {
					DrawHorzLine(-1);
					Cursor = Cursors.No;
				}
			}
		}

		protected override void OnMouseDown(MouseEventArgs e)
		{
			base.OnMouseDown(e);

			activeButton = ButtonFromPoint(e.X, e.Y);

			// ボタンが押されたように描画
			if (e.Button == MouseButtons.Left &&
				activeButton != null)
			{
				if (activeButton.Style != CSharpToolBarButtonStyle.Separator)
				{
					using (Graphics g = CreateGraphics())
						DrawButton(g, activeButton, true, true);
				}
			}
		}

		protected override void OnMouseUp(MouseEventArgs e)
		{
			base.OnMouseUp(e);

			// ボタンが押されたように描画
			if (e.Button == MouseButtons.Left &&
				activeButton != null)
			{
				if (activeButton.Style == CSharpToolBarButtonStyle.Separator)
					return;

				CSharpToolBarButton button = ButtonFromPoint(e.X, e.Y);

				UpdateButton(activeButton);
				DrawHorzLine(-1);

				// クリックされたボタンと現在のマウス座標にあるボタンが別の物であれば、
				// activeButtonsを移動
				if (activeButton != button)
				{
					if (allowDragButton && ClientRect.Contains(e.X, e.Y))
					{
						int index = GetDropButtonIndex(e.X, e.Y);

						if (index >= 0 && index <= buttons.Count)
						{
							buttons.ChangeIndex(activeButton, index);
						}
					}
					Cursor = Cursors.Default;
				}
				else
				{
					// クリックイベントを発生させる
					OnButtonClick(new CSharpToolBarButtonEventArgs(activeButton));
				}

				activeButton = null;
			}
		}

		protected override void OnResize(EventArgs e)
		{
			base.OnResize(e);
			UpdateButtons();
		}
		#endregion

		/// <summary>
		/// ボタンを最新の状態に更新
		/// </summary>
		internal void UpdateButtons()
		{
			int height = 0;

			// ボタンのRectangle座標を更新
			using (Graphics g = CreateGraphics())
			{
				foreach (CSharpToolBarButton button in buttons)
				{
					button.bounds = GetButtonRect(g, button);
					height = Math.Max(height, button.Bounds.Bottom);
				}
			}

			// 高さを調整 (3Dボーダーのサイズも足す)
			if (AutoToolBarSize)
				this.Height = height + SystemInformation.Border3DSize.Height;

			UpdateRegion();
			Refresh();
		}

		/// <summary>
		/// 指定したボタンを再描画
		/// </summary>
		/// <param name="button">再描画するボタン (nullを指定した場合は何もしない)</param>
		protected void UpdateButton(CSharpToolBarButton button)
		{
			if (button != null)
			{
				Invalidate(button.Bounds, false);
				Update();
			}
		}

		/// <summary>
		/// ドロップ先のボタンを取得
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <returns></returns>
		protected int GetDropButtonIndex(int x, int y)
		{
			CSharpToolBarButton button = ButtonFromPoint(x, y);
			
			if (button != null)
			{
				int x2 = x - button.Bounds.X;

				if (x2 >= button.Bounds.Width / 2)
				{
					return button.Index + 1;
				}
				else {
					return button.Index;
				}
			}
			return -1;
		}

		/// <summary>
		/// ドラッグ先を表す縦のラインを描画
		/// </summary>
		/// <param name="index">描画するボタンのインデックス (-1なら線を消す)</param>
		protected void DrawHorzLine(int index)
		{
			if (tempDropLine != Rectangle.Empty)
				ControlPaint.FillReversibleRectangle(tempDropLine, Color.Black);

			if (index >= 0)
			{
				CSharpToolBarButton button = (index < Buttons.Count) ?
					Buttons[index] : Buttons[Buttons.Count-1];

				Rectangle rc = button.Bounds;
				rc.Width = 2;

				if (index >= Buttons.Count)
					rc.X = button.Bounds.Right - 2;

				tempDropLine = RectangleToScreen(rc);

				using (Graphics g = CreateGraphics())
					ControlPaint.FillReversibleRectangle(tempDropLine, Color.Black);
			}
			else {
				tempDropLine = Rectangle.Empty;
			}
		}

		/// <summary>
		/// ボタンを描画
		/// </summary>
		/// <param name="g"></param>
		/// <param name="button"></param>
		/// <param name="pushed"></param>
		protected void DrawButton(Graphics g, CSharpToolBarButton button, bool pushed, bool active)
		{
			if (g == null) {
				throw new ArgumentNullException("g");
			}
			if (button == null) {
				throw new ArgumentNullException("button");
			}

			StringFormat format = StringFormat.GenericDefault;
			format.Alignment = StringAlignment.Center;
			format.LineAlignment = StringAlignment.Center;
			
			if (textAlign == ToolBarTextAlign.Right)
				format.FormatFlags = StringFormatFlags.NoWrap;

			Rectangle bounds = button.Bounds;
			Rectangle imageRect = Rectangle.Empty, textRect = Rectangle.Empty;
			Size imgSize = (imageList != null) ? imageList.ImageSize : new Size(0, 0);

			if (button.Style == CSharpToolBarButtonStyle.Separator)
			{
				// 境界線を描画
				Size border = SystemInformation.Border3DSize;
				Rectangle rect = button.Bounds;
				rect.X += rect.Width / 2 - border.Width / 2;
				rect.Y += _Margin.Y;
				rect.Height -= _Margin.Y;
				rect.Width = border.Width;
				ControlPaint.DrawBorder3D(g, rect, Border3DStyle.Etched, Border3DSide.Right);
				return;
			}

			switch (textAlign)
			{
				// イメージを上辺、テキストを下辺に配置
			case ToolBarTextAlign.Underneath:
				imageRect = new Rectangle(bounds.X + bounds.Width / 2 - imgSize.Width / 2, bounds.Y+_Margin.Y, imgSize.Width, imgSize.Height);
				textRect = new Rectangle(bounds.X, imageRect.Bottom, bounds.Width, bounds.Height - imageRect.Height);
				break;
				// イメージを左辺、テキストを右辺に配置
			case ToolBarTextAlign.Right:
				imageRect = new Rectangle(bounds.X+_Margin.X, bounds.Y + bounds.Height / 2 - imgSize.Height / 2, imgSize.Width, imgSize.Height);
				textRect = new Rectangle(imageRect.Right, bounds.Y, bounds.Width - imageRect.Width, bounds.Height);
				break;
			}

			if (appearance == CSharpToolBarAppearance.Normal)
			{
				if (pushed)
				{
					// 通常のボタンが押された状態を描画
					ControlPaint.DrawButton(g, 
						activeButton.Bounds, ButtonState.Pushed);
				}
				else {
					// 通常のボタンを描画
					ControlPaint.DrawButton(g, 
						bounds, ButtonState.Normal);
				}
			}
			else if (appearance == CSharpToolBarAppearance.Flat)
			{
				if (pushed)
				{
					// フラットボタンが押された状態を描画
					ControlPaint.DrawBorder3D(g, 
						activeButton.Bounds, Border3DStyle.SunkenOuter);
				}
			}
			else if (appearance == CSharpToolBarAppearance.VisualStudio)
			{
				if (active)
				{
					Rectangle rc = button.Bounds;

					rc.Width -= 2;
					rc.Height -= 2;

					Color color = pushed ?
						SystemColors.ControlDark : SystemColors.Highlight;

					using (Brush b = new SolidBrush(Color.FromArgb(50, color)))
						g.FillRectangle(b, rc);

					using (Pen pen = new Pen(color))
						g.DrawRectangle(pen, rc);
				}

			}

			if (imageList != null &&
				button.ImageIndex >= 0 && button.ImageIndex < imageList.Images.Count)
			{
				// アイコンを描画
				g.DrawImage(imageList.Images[button.ImageIndex], imageRect.X, imageRect.Y);
			}

			if (button.Text.Length > 0)
			{
				// テキストを描画
				g.DrawString(button.Text, Font, SystemBrushes.ControlText, textRect, format);
			}
		}

		/// <summary>
		/// 指定した座標にあるCSharpToolBarButtonを取得
		/// </summary>
		/// <param name="x">クライアント座標のX軸</param>
		/// <param name="y">クライアント座標のY軸</param>
		/// <returns>指定した座標に存在するCSharpToolBarButton。見つからなければnullを返す。</returns>
		public CSharpToolBarButton ButtonFromPoint(int x, int y)
		{
			foreach (CSharpToolBarButton button in buttons)
			{
				if (button.Bounds.Contains(x, y))
					return button;
			}

			return null;
		}

		/// <summary>
		/// 指定したbuttonのRectangle座標を計算
		/// </summary>
		/// <param name="g">文字列幅の計算に使用するGraphicsクラスのインスタンス</param>
		/// <param name="button">サイズを計算するCSharpToolBarButton</param>
		/// <returns>buttonのRectangle座標を返す</returns>
		protected Rectangle GetButtonRect(Graphics g, CSharpToolBarButton button)
		{
			Size borderSize = borderStyle == Border3DStyle.Adjust ? new Size(0,0) : SystemInformation.Border3DSize;
			Rectangle rect = new Rectangle(borderSize.Width, borderSize.Height,0,0);
			int height = 0;

			foreach (CSharpToolBarButton b in buttons)
			{
				Size size;
				
				if (b.Style == CSharpToolBarButtonStyle.Separator)
				{
					size = GetButtonSize(g, b);
				}
				else {
					size = autoAdjustSize ? GetButtonSize(g, b) : buttonSize;
				}

				rect.Width = size.Width;
				rect.Height = size.Height;
				height = Math.Max(height, size.Height);

				// 座標がツールバーの幅をはみ出して、
				// なおかつWrappableプロパティがtrueの場合
				if ((rect.X + rect.Width) > ClientSize.Width && Wrappable)
				{
					rect.X = borderSize.Width;
					rect.Y += height;
				}

				if (b.Equals(button))
					return rect;

				rect.X += size.Width;
			}

			return Rectangle.Empty;
		}

		/// <summary>
		/// 指定したbuttonのサイズを計算
		/// </summary>
		/// <param name="g">文字列幅の計算に使用するGraphicsクラスのインスタンス</param>
		/// <param name="button">サイズを計算するCSharpToolBarButton</param>
		/// <returns>buttonのサイズを返す</returns>
		protected Size GetButtonSize(Graphics g, CSharpToolBarButton button)
		{
			if (g == null) {
				throw new ArgumentNullException("g");
			}
			if (button == null) {
				throw new ArgumentNullException("button");
			}

			Size size, space = g.MeasureString(" ", Font).ToSize();
			
			// セパレータ
			if (button.Style == CSharpToolBarButtonStyle.Separator)
			{
				size = space;
				size.Width = SystemInformation.Border3DSize.Width;
				
				if (textAlign == ToolBarTextAlign.Underneath)
					size.Height += space.Height;
			}
			// 文字、画像ともに設定されていない
			else if (button.Text.Length == 0 && button.ImageIndex == -1)
			{
				size = space;
			
				if (textAlign == ToolBarTextAlign.Underneath)
					size.Height += space.Height;
			}
			// 文字のみ設定されている
			else if (button.Text.Length > 0 && button.ImageIndex == -1)
			{
				size = g.MeasureString(button.Text, Font).ToSize();
			
				if (textAlign == ToolBarTextAlign.Underneath)
					size.Height += space.Height;
			}
			// 画像のみ設定されている
			else if (button.Text.Length == 0 && button.ImageIndex != -1)
			{
				if (imageList != null)
				{
					size = imageList.ImageSize;
				}
				else {// 画像が設定されているのに ImageList が無い場合は空白でサイズ調整
					size = space;
				}
				if (textAlign == ToolBarTextAlign.Underneath)
					size.Height += space.Height;
			}
			else {
				size = g.MeasureString(button.Text, Font).ToSize();
		
				// アイコンが存在すればアイコンサイズを足す
				if (imageList != null && button.ImageIndex != -1)
				{
					Size imageSize = imageList.ImageSize;

					switch (textAlign)
					{
						// テキストがイメージの下に配置される
					case ToolBarTextAlign.Underneath:
						size.Width = Math.Max(size.Width, imageSize.Width);
						size.Height += imageSize.Height;
						break;
						// テキストがイメージの左に配置される
					case ToolBarTextAlign.Right:
						size.Width += imageSize.Width;
						size.Height = Math.Max(size.Height, imageSize.Height);
						break;
					}
				}
			}

			size.Width += _Margin.X + _Margin.Width;
			size.Height += _Margin.Y + _Margin.Height;

			return size;
		}

		/// <summary>
		/// ButtonClickイベントを発生させる
		/// </summary>
		/// <param name="e"></param>
		protected void OnButtonClick(CSharpToolBarButtonEventArgs e)
		{
			if (ButtonClick != null)
				ButtonClick(this, e);
		}

		private void InitializeComponent()
		{
			this.SuspendLayout();
			this.ResumeLayout(false);

		}
	}

	public enum CSharpToolBarAppearance
	{
		Normal,
		Flat,
		VisualStudio,
	}
}
