// CSharpToolBar.cs

namespace CSharpSamples
{
	using System;
	using System.ComponentModel;
	using System.ComponentModel.Design;
	using System.Drawing;
	using System.Drawing.Design;
	using System.Windows.Forms;
	using CSharpToolBarButtonCollection =
		CSharpToolBarButton.CSharpToolBarButtonCollection;

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
		private Rectangle _Margin = new Rectangle(2, 2, 2, 4);

		private CSharpToolBarButton activeButton = null;
		private Rectangle tempDropLine = Rectangle.Empty;

		protected override Size DefaultSize
		{
			get
			{
				return new Size(100, 50);
			}
		}

		protected Rectangle ClientRect
		{
			get
			{
				Rectangle client = this.Bounds;
				client.X = client.Y = 0;

				return client;
			}
		}

		/// <summary>
		/// ツールバーのボタンが格納されているコレクションを取得
		/// </summary>
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[Editor(typeof(CollectionEditor), typeof(UITypeEditor))]
		public CSharpToolBarButtonCollection Buttons
		{
			get
			{
				return this.buttons;
			}
		}

		/// <summary>
		/// ツールバーの境界線を取得または設定
		/// </summary>
		[DefaultValue(typeof(Border3DStyle), "Adjust")]
		public Border3DStyle BorderStyle
		{
			set
			{
				if (this.borderStyle != value)
				{
					this.borderStyle = value;
					this.UpdateButtons();
				}
			}
			get { return this.borderStyle; }
		}

		/// <summary>
		/// ツールバーのボタンスタイルを取得または設定
		/// </summary>
		[DefaultValue(typeof(CSharpToolBarAppearance), "Normal")]
		public CSharpToolBarAppearance Appearance
		{
			set
			{
				if (this.appearance != value)
				{
					this.appearance = value;
					this.Refresh();
				}
			}
			get { return this.appearance; }
		}

		/// <summary>
		/// ボタンテキスト配置の位置を取得または設定
		/// </summary>
		[DefaultValue(typeof(ToolBarTextAlign), "Underneath")]
		public ToolBarTextAlign TextAlign
		{
			set
			{
				if (this.textAlign != value)
				{
					this.textAlign = value;
					this.UpdateButtons();
				}
			}
			get { return this.textAlign; }
		}

		/// <summary>
		/// イメージリストを取得または設定
		/// </summary>
		public ImageList ImageList
		{
			set
			{
				this.imageList = value;

				foreach (CSharpToolBarButton b in this.buttons)
				{
					b.imageList = value;
				}

				this.UpdateButtons();
			}
			get
			{
				return this.imageList;
			}
		}

		/// <summary>
		/// ツールバーボタンの固定サイズを取得または設定。
		/// このプロパティを有効にするにはautoAdjustSizeプロパティがfalseに設定されている必要がある。
		/// </summary>
		[DefaultValue(typeof(Size), "80,25")]
		public Size ButtonSize
		{
			set
			{
				this.buttonSize = value;
				this.UpdateButtons();
			}
			get { return this.buttonSize; }
		}

		/// <summary>
		/// ツールバーのサイズを自動で調整するかどうかを取得または設定
		/// </summary>
		[DefaultValue(false)]
		public bool AutoToolBarSize
		{
			set
			{
				if (this.autoToolBarSize != value)
				{
					this.autoToolBarSize = value;
					this.UpdateButtons();
				}
			}
			get { return this.autoToolBarSize; }

		}
		/// <summary>
		/// ボタンの幅を自動で調整するかどうかを取得または設定
		/// </summary>
		[DefaultValue(true)]
		public bool AutoAdjustSize
		{
			set
			{
				if (this.autoAdjustSize != value)
				{
					this.autoAdjustSize = value;
					this.UpdateButtons();
				}
			}
			get { return this.autoAdjustSize; }
		}

		/// <summary>
		/// ツールバーのボタンが一行に収まらないときに
		/// 次の行に折り返すかどうかを取得または設定
		/// </summary>
		[DefaultValue(true)]
		public bool Wrappable
		{
			set
			{
				if (this.wrappable != value)
				{
					this.wrappable = value;
					this.UpdateButtons();
				}
			}
			get { return this.wrappable; }
		}

		/// <summary>
		/// ボタンをドラッグで移動できるかどうかを示す値を取得または設定。
		/// </summary>
		[DefaultValue(false)]
		public bool AllowDragButton
		{
			set
			{
				this.allowDragButton = value;
			}
			get { return this.allowDragButton; }
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

			this.buttons = new CSharpToolBarButtonCollection(this);
			this.appearance = CSharpToolBarAppearance.Normal;
			this.borderStyle = Border3DStyle.Adjust;
			this.textAlign = ToolBarTextAlign.Underneath;
			this.imageList = null;
			this.buttonSize = new Size(80, 25);
			this.autoToolBarSize = false;
			this.autoAdjustSize = true;
			this.wrappable = true;
			this.allowDragButton = true;

			// ちらつきを押さえるために各スタイルを設定
			this.SetStyle(ControlStyles.AllPaintingInWmPaint |
				ControlStyles.DoubleBuffer |
				ControlStyles.UserPaint, true);

			//this.BackColor = Color.Transparent;
			//Dock = DockStyle.Top;
		}

		private void UpdateRegion()
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

			foreach (CSharpToolBarButton button in this.buttons)
			{
				this.DrawButton(g, button, false, false);
			}

			// 境界線を描画
			Rectangle rc = this.Bounds;

			ControlPaint.DrawBorder3D(g, this.ClientRectangle, this.borderStyle);
		}

		protected override void OnMouseEnter(EventArgs e)
		{
			base.OnMouseEnter(e);
		}

		protected override void OnMouseLeave(EventArgs e)
		{
			base.OnMouseLeave(e);
			this.UpdateButton(this.activeButton);
			this.activeButton = null;
		}

		protected override void OnMouseMove(MouseEventArgs e)
		{
			base.OnMouseMove(e);

			CSharpToolBarButton button = this.ButtonFromPoint(e.X, e.Y);

			// セパレータの場合は何もしない
			if (button != null && button.Style == CSharpToolBarButtonStyle.Separator)
			{
				return;
			}

			// フラット形式の場合は、浮き出る境界線を描画
			if (e.Button == MouseButtons.None)
			{
				if (button == this.activeButton)
				{
					return;
				}

				this.UpdateButton(this.activeButton);

				this.activeButton = button;

				if (this.appearance == CSharpToolBarAppearance.Flat &&
					button != null)
				{
					using (Graphics g = this.CreateGraphics())
					{
						ControlPaint.DrawBorder3D(g, button.Bounds, Border3DStyle.RaisedInner);
					}
				}
				else if (this.appearance == CSharpToolBarAppearance.VisualStudio &&
					button != null)
				{
					using (Graphics g = this.CreateGraphics())
					{
						this.DrawButton(g, button, false, true);
					}
				}

			}
			// ボタンのドラッグ操作
			else if (e.Button == MouseButtons.Left && this.allowDragButton)
			{
				if (this.ClientRect.Contains(e.X, e.Y))
				{
					this.DrawHorzLine(this.GetDropButtonIndex(e.X, e.Y));
					this.Cursor = Cursors.Default;
				}
				// クライアント領域から出ていればドラッグ操作を中止
				else
				{
					this.DrawHorzLine(-1);
					this.Cursor = Cursors.No;
				}
			}
		}

		protected override void OnMouseDown(MouseEventArgs e)
		{
			base.OnMouseDown(e);

			this.activeButton = this.ButtonFromPoint(e.X, e.Y);

			// ボタンが押されたように描画
			if (e.Button == MouseButtons.Left &&
				this.activeButton != null)
			{
				if (this.activeButton.Style != CSharpToolBarButtonStyle.Separator)
				{
					using (Graphics g = this.CreateGraphics())
					{
						this.DrawButton(g, this.activeButton, true, true);
					}
				}
			}
		}

		protected override void OnMouseUp(MouseEventArgs e)
		{
			base.OnMouseUp(e);

			// ボタンが押されたように描画
			if (e.Button == MouseButtons.Left &&
				this.activeButton != null)
			{
				if (this.activeButton.Style == CSharpToolBarButtonStyle.Separator)
				{
					return;
				}

				CSharpToolBarButton button = this.ButtonFromPoint(e.X, e.Y);

				this.UpdateButton(this.activeButton);
				this.DrawHorzLine(-1);

				// クリックされたボタンと現在のマウス座標にあるボタンが別の物であれば、
				// activeButtonsを移動
				if (this.activeButton != button)
				{
					if (this.allowDragButton && this.ClientRect.Contains(e.X, e.Y))
					{
						int index = this.GetDropButtonIndex(e.X, e.Y);

						if (index >= 0 && index <= this.buttons.Count)
						{
							this.buttons.ChangeIndex(this.activeButton, index);
						}
					}
					this.Cursor = Cursors.Default;
				}
				else
				{
					// クリックイベントを発生させる
					this.OnButtonClick(new CSharpToolBarButtonEventArgs(this.activeButton));
				}

				this.activeButton = null;
			}
		}

		protected override void OnResize(EventArgs e)
		{
			base.OnResize(e);
			this.UpdateButtons();
		}
		#endregion

		/// <summary>
		/// ボタンを最新の状態に更新
		/// </summary>
		internal void UpdateButtons()
		{
			int height = 0;

			// ボタンのRectangle座標を更新
			using (Graphics g = this.CreateGraphics())
			{
				foreach (CSharpToolBarButton button in this.buttons)
				{
					button.bounds = this.GetButtonRect(g, button);
					height = Math.Max(height, button.Bounds.Bottom);
				}
			}

			// 高さを調整 (3Dボーダーのサイズも足す)
			if (this.AutoToolBarSize)
			{
				this.Height = height + SystemInformation.Border3DSize.Height;
			}

			this.UpdateRegion();
			this.Refresh();
		}

		/// <summary>
		/// 指定したボタンを再描画
		/// </summary>
		/// <param name="button">再描画するボタン (nullを指定した場合は何もしない)</param>
		protected void UpdateButton(CSharpToolBarButton button)
		{
			if (button != null)
			{
				this.Invalidate(button.Bounds, false);
				this.Update();
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
			CSharpToolBarButton button = this.ButtonFromPoint(x, y);

			if (button != null)
			{
				int x2 = x - button.Bounds.X;

				if (x2 >= button.Bounds.Width / 2)
				{
					return button.Index + 1;
				}
				else
				{
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
			if (this.tempDropLine != Rectangle.Empty)
			{
				ControlPaint.FillReversibleRectangle(this.tempDropLine, Color.Black);
			}

			if (index >= 0)
			{
				CSharpToolBarButton button = (index < this.Buttons.Count) ?
					this.Buttons[index] : this.Buttons[this.Buttons.Count - 1];

				Rectangle rc = button.Bounds;
				rc.Width = 2;

				if (index >= this.Buttons.Count)
				{
					rc.X = button.Bounds.Right - 2;
				}

				this.tempDropLine = this.RectangleToScreen(rc);

				using (Graphics g = this.CreateGraphics())
				{
					ControlPaint.FillReversibleRectangle(this.tempDropLine, Color.Black);
				}
			}
			else
			{
				this.tempDropLine = Rectangle.Empty;
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
			if (g == null)
			{
				throw new ArgumentNullException("g");
			}
			if (button == null)
			{
				throw new ArgumentNullException("button");
			}

			StringFormat format = StringFormat.GenericDefault;
			format.Alignment = StringAlignment.Center;
			format.LineAlignment = StringAlignment.Center;

			if (this.textAlign == ToolBarTextAlign.Right)
			{
				format.FormatFlags = StringFormatFlags.NoWrap;
			}

			Rectangle bounds = button.Bounds;
			Rectangle imageRect = Rectangle.Empty, textRect = Rectangle.Empty;
			Size imgSize = (this.imageList != null) ? this.imageList.ImageSize : new Size(0, 0);

			if (button.Style == CSharpToolBarButtonStyle.Separator)
			{
				// 境界線を描画
				Size border = SystemInformation.Border3DSize;
				Rectangle rect = button.Bounds;
				rect.X += rect.Width / 2 - border.Width / 2;
				rect.Y += this._Margin.Y;
				rect.Height -= this._Margin.Y;
				rect.Width = border.Width;
				ControlPaint.DrawBorder3D(g, rect, Border3DStyle.Etched, Border3DSide.Right);
				return;
			}

			switch (this.textAlign)
			{
				// イメージを上辺、テキストを下辺に配置
				case ToolBarTextAlign.Underneath:
					imageRect = new Rectangle(bounds.X + bounds.Width / 2 - imgSize.Width / 2, bounds.Y + this._Margin.Y, imgSize.Width, imgSize.Height);
					textRect = new Rectangle(bounds.X, imageRect.Bottom, bounds.Width, bounds.Height - imageRect.Height);
					break;
				// イメージを左辺、テキストを右辺に配置
				case ToolBarTextAlign.Right:
					imageRect = new Rectangle(bounds.X + this._Margin.X, bounds.Y + bounds.Height / 2 - imgSize.Height / 2, imgSize.Width, imgSize.Height);
					textRect = new Rectangle(imageRect.Right, bounds.Y, bounds.Width - imageRect.Width, bounds.Height);
					break;
			}

			if (this.appearance == CSharpToolBarAppearance.Normal)
			{
				if (pushed)
				{
					// 通常のボタンが押された状態を描画
					ControlPaint.DrawButton(g,
						this.activeButton.Bounds, ButtonState.Pushed);
				}
				else
				{
					// 通常のボタンを描画
					ControlPaint.DrawButton(g,
						bounds, ButtonState.Normal);
				}
			}
			else if (this.appearance == CSharpToolBarAppearance.Flat)
			{
				if (pushed)
				{
					// フラットボタンが押された状態を描画
					ControlPaint.DrawBorder3D(g,
						this.activeButton.Bounds, Border3DStyle.SunkenOuter);
				}
			}
			else if (this.appearance == CSharpToolBarAppearance.VisualStudio)
			{
				if (active)
				{
					Rectangle rc = button.Bounds;

					rc.Width -= 2;
					rc.Height -= 2;

					Color color = pushed ?
						SystemColors.ControlDark : SystemColors.Highlight;

					using (Brush b = new SolidBrush(Color.FromArgb(50, color)))
					{
						g.FillRectangle(b, rc);
					}

					using (Pen pen = new Pen(color))
					{
						g.DrawRectangle(pen, rc);
					}
				}

			}

			if (this.imageList != null &&
				button.ImageIndex >= 0 && button.ImageIndex < this.imageList.Images.Count)
			{
				// アイコンを描画
				g.DrawImage(this.imageList.Images[button.ImageIndex], imageRect.X, imageRect.Y);
			}

			if (button.Text.Length > 0)
			{
				// テキストを描画
				g.DrawString(button.Text, this.Font, SystemBrushes.ControlText, textRect, format);
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
			foreach (CSharpToolBarButton button in this.buttons)
			{
				if (button.Bounds.Contains(x, y))
				{
					return button;
				}
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
			Size borderSize = this.borderStyle == Border3DStyle.Adjust ? new Size(0, 0) : SystemInformation.Border3DSize;
			Rectangle rect = new Rectangle(borderSize.Width, borderSize.Height, 0, 0);
			int height = 0;

			foreach (CSharpToolBarButton b in this.buttons)
			{
				Size size;

				if (b.Style == CSharpToolBarButtonStyle.Separator)
				{
					size = this.GetButtonSize(g, b);
				}
				else
				{
					size = this.autoAdjustSize ? this.GetButtonSize(g, b) : this.buttonSize;
				}

				rect.Width = size.Width;
				rect.Height = size.Height;
				height = Math.Max(height, size.Height);

				// 座標がツールバーの幅をはみ出して、
				// なおかつWrappableプロパティがtrueの場合
				if ((rect.X + rect.Width) > this.ClientSize.Width && this.Wrappable)
				{
					rect.X = borderSize.Width;
					rect.Y += height;
				}

				if (b.Equals(button))
				{
					return rect;
				}

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
			if (g == null)
			{
				throw new ArgumentNullException("g");
			}
			if (button == null)
			{
				throw new ArgumentNullException("button");
			}

			Size size, space = g.MeasureString(" ", this.Font).ToSize();

			// セパレータ
			if (button.Style == CSharpToolBarButtonStyle.Separator)
			{
				size = space;
				size.Width = SystemInformation.Border3DSize.Width;

				if (this.textAlign == ToolBarTextAlign.Underneath)
				{
					size.Height += space.Height;
				}
			}
			// 文字、画像ともに設定されていない
			else if (button.Text.Length == 0 && button.ImageIndex == -1)
			{
				size = space;

				if (this.textAlign == ToolBarTextAlign.Underneath)
				{
					size.Height += space.Height;
				}
			}
			// 文字のみ設定されている
			else if (button.Text.Length > 0 && button.ImageIndex == -1)
			{
				size = g.MeasureString(button.Text, this.Font).ToSize();

				if (this.textAlign == ToolBarTextAlign.Underneath)
				{
					size.Height += space.Height;
				}
			}
			// 画像のみ設定されている
			else if (button.Text.Length == 0 && button.ImageIndex != -1)
			{
				if (this.imageList != null)
				{
					size = this.imageList.ImageSize;
				}
				else
				{// 画像が設定されているのに ImageList が無い場合は空白でサイズ調整
					size = space;
				}
				if (this.textAlign == ToolBarTextAlign.Underneath)
				{
					size.Height += space.Height;
				}
			}
			else
			{
				size = g.MeasureString(button.Text, this.Font).ToSize();

				// アイコンが存在すればアイコンサイズを足す
				if (this.imageList != null && button.ImageIndex != -1)
				{
					Size imageSize = this.imageList.ImageSize;

					switch (this.textAlign)
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

			size.Width += this._Margin.X + this._Margin.Width;
			size.Height += this._Margin.Y + this._Margin.Height;

			return size;
		}

		/// <summary>
		/// ButtonClickイベントを発生させる
		/// </summary>
		/// <param name="e"></param>
		protected void OnButtonClick(CSharpToolBarButtonEventArgs e)
		{
			if (ButtonClick != null)
			{
				ButtonClick(this, e);
			}
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
