// SmoothProgressBar.cs

namespace CSharpSamples
{
	using System;
	using System.Windows.Forms;
	using System.Drawing;

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
		public Color ValueColor {
			set { valueColor = value; }
			get { return valueColor; }
		}

		/// <summary>
		/// テキストの表示スタイルを取得または設定
		/// </summary>
		public ProgressTextStyle TextStyle {
			set {
				if (value != style) {
					style = value;
					Refresh();
				}
			}
			get { return style; }
		}

		/// <summary>
		/// SmoothProgressBarクラスのインスタンスを初期化
		/// </summary>
		public SmoothProgressBar() : base()
		{
			// 
			// TODO: コンストラクタ ロジックをここに追加してください。
			//
			ValueColor = SystemColors.Highlight;
			ForeColor = Color.Black;
			style = ProgressTextStyle.Percent;
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			base.OnPaint(e);

			Graphics g = e.Graphics;
			Rectangle rect = e.ClipRectangle;

			// ブラシを作成
			Brush brush = new SolidBrush(ValueColor);
			Brush blank = new SolidBrush(SystemColors.Control);

			// position(現在位置)から描画範囲を計算
			float range = (float)(Math.Abs(Minimum) + Math.Abs(Maximum));
			float pos = range != 0 ? ((float)Position / range) : 0;
			float right = rect.Width * pos;

			g.FillRectangle(brush, 0, 0, right, rect.Height);
			g.FillRectangle(blank, right, 0, rect.Width - right, rect.Height);

			// 数字を描画
			StringFormat format = StringFormat.GenericDefault;
			Brush textbrush = new SolidBrush(ForeColor);
			string text = null;

			switch (style) 
			{
			case ProgressTextStyle.Percent:
				text = Percent + "%";
				break;

			case ProgressTextStyle.Length:
				text = String.Format("{0}/{1}", Position, Maximum);
				break;

			case ProgressTextStyle.None:
				text = String.Empty;
				break;
			}

			// 全体の中央に配置
			format.Alignment = StringAlignment.Center;
			format.LineAlignment = StringAlignment.Center;

			// 文字列を描画
			g.DrawString(text, this.Font, textbrush, rect, format);

			// 境界線を描画
			Rectangle bounds = new Rectangle(0, 0, Width, Height);
			ControlPaint.DrawBorder3D(g, bounds, BorderStyle);
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
