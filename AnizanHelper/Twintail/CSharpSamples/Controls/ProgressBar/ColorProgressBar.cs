// ColorProgressBar.cs

namespace CSharpSamples
{
	using System;
	using System.Windows.Forms;
	using System.Drawing;

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
		public int ScaleSize {
			set {
				if (value < 1) {
					throw new ArgumentOutOfRangeException("ScaleSizeは1以上の値でなければなりません");
				}
				scaleSize = value;
			}
			get { return scaleSize; }
		}

		/// <summary>
		/// 境界の色を取得または設定
		/// </summary>
		public Color BorderColor {
			set { borderColor = value; }
			get { return borderColor; }
		}

		/// <summary>
		/// プログレスバーの色配列を取得または設定
		/// </summary>
		public Color[] Colors {
			set {
				if (value == null) {
					throw new ArgumentNullException("Colors");
				}
				colors = value;
				Refresh();
			}
			get { return colors; }
		}

		/// <summary>
		/// ColorProgressBarクラスのインスタンスを初期化
		/// </summary>
		public ColorProgressBar() : base()
		{
			// 
			// TODO: コンストラクタ ロジックをここに追加してください。
			//
			scaleSize = 8;
			borderSize = 1;
			borderColor = Color.Black;
			BackColor = Color.DarkGreen;
			BorderStyle = Border3DStyle.Flat;
			colors = new Color[1] { Color.Green };
		}

		/// <summary>
		/// 目盛りを描く
		/// </summary>
		/// <param name="g"></param>
		private void DrawScale(Graphics g)
		{
			Pen pen = new Pen(borderColor, borderSize);
			Rectangle rect = new Rectangle(0, 0, this.Width, this.Height);
			
			// 四角を描く
			g.DrawRectangle(pen, rect);

			// 縦線を描く
			Point from = new Point(0, 0);
			Point to = new Point(0, this.Height);

			for (int i = Minimum; i <= Maximum; i++)
			{
				from.X = (scaleSize + borderSize) * i;
				to.X = (scaleSize + borderSize) * i;
				g.DrawLine(pen, from, to);
			}
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			base.OnPaint(e);
			Graphics g = e.Graphics;

			// 目盛りを描く
			DrawScale(g);

			// １つの色に対してのメモリ数を計算
			int clrCount = Maximum / colors.Length;

			// 座標
			Rectangle rect = new Rectangle(borderSize, borderSize, 
				scaleSize, this.Height - borderSize * 2);
			SolidBrush brush = null;

			for (int i = Minimum; i < Position; i++)
			{
				int clridx = i / clrCount;
				
				if (clridx >= colors.Length)
					clridx = colors.Length-1;

				Color color = colors[clridx];

				// ブラシを作成
				if (brush == null || brush.Color != color)
					brush = new SolidBrush(color);

				g.FillRectangle(brush, rect);
				rect.X += scaleSize + borderSize;
			}

			// 境界線を描画
			Rectangle bounds = new Rectangle(0, 0, Width, Height);
			ControlPaint.DrawBorder3D(g, bounds, BorderStyle);
		}

		/// <summary>
		/// 目盛りと幅を丁度合うようにリサイズ
		/// </summary>
		public void ResizeBar()
		{
			this.Width = (ScaleSize + borderSize) * Maximum + borderSize;
		}
	}

}
