// ProgressCtrl.cs

namespace CSharpSamples
{
	using System;
	using System.Windows.Forms;
	using System.Drawing;

	/// <summary>
	/// プログレスバーの基本
	/// </summary>
	public abstract class ProgressCtrl : Control
	{
		private int minimum;
		private int maximum;
		private int position;
		private int step;
		private Border3DStyle border;

		/// <summary>
		/// プログレスバーの最小値を取得または設定
		/// </summary>
		public int Minimum {
			set {
				if (value > maximum) {
					throw new ArgumentOutOfRangeException("Minimum");
				}

				if (minimum != value)
				{
					minimum = value;
					Refresh();
				}
			}
			get { return minimum; }
		}

		/// <summary>
		/// プログレスバーの最大値を取得または設定
		/// </summary>
		public int Maximum {
			set {
				if (value < minimum) {
					throw new ArgumentOutOfRangeException("Maximum");
				}

				if (maximum != value)
				{
					maximum = value;
					Refresh();
				}
			}
			get { return maximum; }
		}

		/// <summary>
		/// プログレスバーの現在値を取得または設定
		/// </summary>
		public int Position {
			set {
				if (value < 0 || value > maximum) {
					throw new ArgumentOutOfRangeException("Position");
				}

				if (position != value)
				{
					position = value;
					Refresh();
				}
			}
			get { return position; }
		}

		/// <summary>
		/// PerformStepメソッドを使用した時の増量分を取得または設定
		/// </summary>
		public int Step {
			set {
				if (value > maximum) {
					throw new ArgumentOutOfRangeException("Step");
				}

				if (step != value)
				{
					step = value;
				}
			}
			get { return step; }
		}

		/// <summary>
		/// プログレスバーの境界線を取得または設定
		/// </summary>
		public Border3DStyle BorderStyle {
			set {
				if (value != border) {
					border = value;
					Refresh();
				}
			}
			get { return border; }
		}

		/// <summary>
		/// 百分率を取得
		/// </summary>
		protected int Percent {
			get {
				float range = (float)(Math.Abs(minimum) + Math.Abs(maximum));

				if (range == 0)
					return 0;
				
				float result = (float)position / range * 100.0f;
				return (int)result;
			}
		}

		/// <summary>
		/// ProgressCtrlクラスのインスタンスを初期化
		/// </summary>
		public ProgressCtrl()
		{
			// 
			// TODO: コンストラクタ ロジックをここに追加してください。
			//
			SetStyle(ControlStyles.UserPaint, true);
			SetStyle(ControlStyles.AllPaintingInWmPaint, true);
			SetStyle(ControlStyles.DoubleBuffer, true);
			SetStyle(ControlStyles.ResizeRedraw, true);

			border = Border3DStyle.SunkenOuter;
			minimum = 0;
			position = 0;
			maximum = 100;
			step = 1;
		}

		/// <summary>
		/// 現在の位置からStepの分だけ進める
		/// </summary>
		public virtual void PerformStep()
		{
			Increment(Step);
		}

		/// <summary>
		/// 指定した量だけ現在位置を進める
		/// </summary>
		/// <param name="value">現在位置をインクリメントする量</param>
		public virtual void Increment(int value)
		{
			if (Position + value >= Maximum)
			{
				Position = Maximum;
			}
			else {
				Position += value;
			}
		}

		/// <summary>
		/// 現在位置を最小値にリセット
		/// </summary>
		public virtual void Reset()
		{
			Position = 0;
		}
	}
}
