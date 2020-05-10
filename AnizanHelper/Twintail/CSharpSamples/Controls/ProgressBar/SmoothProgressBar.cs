// SmoothProgressBar.cs

namespace CSharpSamples
{
	using System;
	using System.Drawing;
	using System.Windows.Forms;

	/// <summary>
	/// スムースな表示方式のプログレスバー。
	/// .NET 2.0 は標準であるので必要なくなった。
	/// </summary>
	public class SmoothProgressBar : ProgressCtrl
	{
		private ProgressTextStyle style;
		private Color valueColor;

		/// <summary>
		/// 値部分の色を取得または設定
		/// </summary>
		public Color ValueColor
		{
			set { this.valueColor = value; }
			get { return this.valueColor; }
		}

		/// <summary>
		/// テキストの表示スタイルを取得または設定
		/// </summary>
		public ProgressTextStyle TextStyle
		{
			set
			{
				if (value != this.style)
				{
					this.style = value;
					this.Refresh();
				}
			}
			get { return this.style; }
		}

		/// <summary>
		/// SmoothProgressBarクラスのインスタンスを初期化
		/// </summary>
		public SmoothProgressBar() : base()
		{
			// 
			// TODO: コンストラクタ ロジックをここに追加してください。
			//
			this.ValueColor = SystemColors.Highlight;
			this.ForeColor = Color.Black;
			this.style = ProgressTextStyle.Percent;
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			base.OnPaint(e);

			Graphics g = e.Graphics;
			Rectangle rect = e.ClipRectangle;

			// ブラシを作成
			Brush brush = new SolidBrush(this.ValueColor);
			Brush blank = new SolidBrush(SystemColors.Control);

			// position(現在位置)から描画範囲を計算
			float range = (float)(Math.Abs(this.Minimum) + Math.Abs(this.Maximum));
			float pos = range != 0 ? ((float)this.Position / range) : 0;
			float right = rect.Width * pos;

			g.FillRectangle(brush, 0, 0, right, rect.Height);
			g.FillRectangle(blank, right, 0, rect.Width - right, rect.Height);

			// 数字を描画
			StringFormat format = StringFormat.GenericDefault;
			Brush textbrush = new SolidBrush(this.ForeColor);
			string text = null;

			switch (this.style)
			{
				case ProgressTextStyle.Percent:
					text = this.Percent + "%";
					break;

				case ProgressTextStyle.Length:
					text = string.Format("{0}/{1}", this.Position, this.Maximum);
					break;

				case ProgressTextStyle.None:
					text = string.Empty;
					break;
			}

			// 全体の中央に配置
			format.Alignment = StringAlignment.Center;
			format.LineAlignment = StringAlignment.Center;

			// 文字列を描画
			g.DrawString(text, this.Font, textbrush, rect, format);

			// 境界線を描画
			Rectangle bounds = new Rectangle(0, 0, this.Width, this.Height);
			ControlPaint.DrawBorder3D(g, bounds, this.BorderStyle);
		}
	}

	/// <summary>
	/// プログレスに表示するテキストの表示スタイルを表す
	/// </summary>
	public enum ProgressTextStyle
	{
		/// <summary>
		/// テキストを表示しない
		/// </summary>
		None = 0,
		/// <summary>
		/// 百分率表示
		/// </summary>
		Percent,
		/// <summary>
		/// 全体の長さを表示
		/// </summary>
		Length,
	}
}
