// ColorProgressBar.cs

namespace CSharpSamples
{
	using System;
	using System.Drawing;
	using System.Windows.Forms;

	/// <summary>
	/// カラフルなプログレスバー
	/// </summary>
	public class ColorProgressBar : ProgressCtrl
	{
		private Color[] colors;
		private Color borderColor;
		private int scaleSize;
		private int borderSize;

		/// <summary>
		/// 目盛りのサイズを取得または設定
		/// </summary>
		public int ScaleSize
		{
			set
			{
				if (value < 1)
				{
					throw new ArgumentOutOfRangeException("ScaleSizeは1以上の値でなければなりません");
				}
				this.scaleSize = value;
			}
			get { return this.scaleSize; }
		}

		/// <summary>
		/// 境界の色を取得または設定
		/// </summary>
		public Color BorderColor
		{
			set { this.borderColor = value; }
			get { return this.borderColor; }
		}

		/// <summary>
		/// プログレスバーの色配列を取得または設定
		/// </summary>
		public Color[] Colors
		{
			set
			{
				if (value == null)
				{
					throw new ArgumentNullException("Colors");
				}
				this.colors = value;
				this.Refresh();
			}
			get { return this.colors; }
		}

		/// <summary>
		/// ColorProgressBarクラスのインスタンスを初期化
		/// </summary>
		public ColorProgressBar() : base()
		{
			// 
			// TODO: コンストラクタ ロジックをここに追加してください。
			//
			this.scaleSize = 8;
			this.borderSize = 1;
			this.borderColor = Color.Black;
			this.BackColor = Color.DarkGreen;
			this.BorderStyle = Border3DStyle.Flat;
			this.colors = new Color[1] { Color.Green };
		}

		/// <summary>
		/// 目盛りを描く
		/// </summary>
		/// <param name="g"></param>
		private void DrawScale(Graphics g)
		{
			Pen pen = new Pen(this.borderColor, this.borderSize);
			Rectangle rect = new Rectangle(0, 0, this.Width, this.Height);

			// 四角を描く
			g.DrawRectangle(pen, rect);

			// 縦線を描く
			Point from = new Point(0, 0);
			Point to = new Point(0, this.Height);

			for (int i = this.Minimum; i <= this.Maximum; i++)
			{
				from.X = (this.scaleSize + this.borderSize) * i;
				to.X = (this.scaleSize + this.borderSize) * i;
				g.DrawLine(pen, from, to);
			}
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			base.OnPaint(e);
			Graphics g = e.Graphics;

			// 目盛りを描く
			this.DrawScale(g);

			// １つの色に対してのメモリ数を計算
			int clrCount = this.Maximum / this.colors.Length;

			// 座標
			Rectangle rect = new Rectangle(this.borderSize, this.borderSize,
				this.scaleSize, this.Height - this.borderSize * 2);
			SolidBrush brush = null;

			for (int i = this.Minimum; i < this.Position; i++)
			{
				int clridx = i / clrCount;

				if (clridx >= this.colors.Length)
				{
					clridx = this.colors.Length - 1;
				}

				Color color = this.colors[clridx];

				// ブラシを作成
				if (brush == null || brush.Color != color)
				{
					brush = new SolidBrush(color);
				}

				g.FillRectangle(brush, rect);
				rect.X += this.scaleSize + this.borderSize;
			}

			// 境界線を描画
			Rectangle bounds = new Rectangle(0, 0, this.Width, this.Height);
			ControlPaint.DrawBorder3D(g, bounds, this.BorderStyle);
		}

		/// <summary>
		/// 目盛りと幅を丁度合うようにリサイズ
		/// </summary>
		public void ResizeBar()
		{
			this.Width = (this.ScaleSize + this.borderSize) * this.Maximum + this.borderSize;
		}
	}

}
