// ProgressCtrl.cs

namespace CSharpSamples
{
	using System;
	using System.Windows.Forms;

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
		public int Minimum
		{
			set
			{
				if (value > this.maximum)
				{
					throw new ArgumentOutOfRangeException("Minimum");
				}

				if (this.minimum != value)
				{
					this.minimum = value;
					this.Refresh();
				}
			}
			get { return this.minimum; }
		}

		/// <summary>
		/// プログレスバーの最大値を取得または設定
		/// </summary>
		public int Maximum
		{
			set
			{
				if (value < this.minimum)
				{
					throw new ArgumentOutOfRangeException("Maximum");
				}

				if (this.maximum != value)
				{
					this.maximum = value;
					this.Refresh();
				}
			}
			get { return this.maximum; }
		}

		/// <summary>
		/// プログレスバーの現在値を取得または設定
		/// </summary>
		public int Position
		{
			set
			{
				if (value < 0 || value > this.maximum)
				{
					throw new ArgumentOutOfRangeException("Position");
				}

				if (this.position != value)
				{
					this.position = value;
					this.Refresh();
				}
			}
			get { return this.position; }
		}

		/// <summary>
		/// PerformStepメソッドを使用した時の増量分を取得または設定
		/// </summary>
		public int Step
		{
			set
			{
				if (value > this.maximum)
				{
					throw new ArgumentOutOfRangeException("Step");
				}

				if (this.step != value)
				{
					this.step = value;
				}
			}
			get { return this.step; }
		}

		/// <summary>
		/// プログレスバーの境界線を取得または設定
		/// </summary>
		public Border3DStyle BorderStyle
		{
			set
			{
				if (value != this.border)
				{
					this.border = value;
					this.Refresh();
				}
			}
			get { return this.border; }
		}

		/// <summary>
		/// 百分率を取得
		/// </summary>
		protected int Percent
		{
			get
			{
				float range = (float)(Math.Abs(this.minimum) + Math.Abs(this.maximum));

				if (range == 0)
				{
					return 0;
				}

				float result = (float)this.position / range * 100.0f;
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
			this.SetStyle(ControlStyles.UserPaint, true);
			this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
			this.SetStyle(ControlStyles.DoubleBuffer, true);
			this.SetStyle(ControlStyles.ResizeRedraw, true);

			this.border = Border3DStyle.SunkenOuter;
			this.minimum = 0;
			this.position = 0;
			this.maximum = 100;
			this.step = 1;
		}

		/// <summary>
		/// 現在の位置からStepの分だけ進める
		/// </summary>
		public virtual void PerformStep()
		{
			this.Increment(this.Step);
		}

		/// <summary>
		/// 指定した量だけ現在位置を進める
		/// </summary>
		/// <param name="value">現在位置をインクリメントする量</param>
		public virtual void Increment(int value)
		{
			if (this.Position + value >= this.Maximum)
			{
				this.Position = this.Maximum;
			}
			else
			{
				this.Position += value;
			}
		}

		/// <summary>
		/// 現在位置を最小値にリセット
		/// </summary>
		public virtual void Reset()
		{
			this.Position = 0;
		}
	}
}
