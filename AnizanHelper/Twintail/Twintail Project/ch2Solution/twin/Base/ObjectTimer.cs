// ObjectTimer.cs

namespace Twin.Bbs
{
	using System;
	using System.Timers;

	/// <summary>
	/// タイマーイベントにobject型を渡すクラス
	/// </summary>
	public class ObjectTimer
	{
		private Timer timer;
		private object tag;

		/// <summary>
		/// タイマー間隔を取得または設定
		/// </summary>
		public int Interval {
			set {
				if (value < 1) {
					throw new ArgumentOutOfRangeException("Interval");
				}
				timer.Interval = value;
			}
			get { return (int)timer.Interval; }
		}

		/// <summary>
		/// 時間が経過したら発生
		/// </summary>
		public event ObjectTimerEventHandler Elapsed;

		/// <summary>
		/// ObjectTimerクラスのインスタンスを初期化
		/// </summary>
		public ObjectTimer()
		{
			// 
			// TODO: コンストラクタ ロジックをここに追加してください。
			//
			timer = new Timer();
			timer.Interval = 250;
			timer.Elapsed += new ElapsedEventHandler(OnElapsed);
		}

		/// <summary>
		/// タイマー開始
		/// </summary>
		public void Start(object obj)
		{
			tag = obj;
			timer.Start();
		}

		/// <summary>
		/// タイマー終了
		/// </summary>
		public void Stop()
		{
			timer.Stop();
		}

		private void OnElapsed(object sender, ElapsedEventArgs e)
		{
			timer.Stop();

			if (Elapsed != null)
				Elapsed(this, new ObjectTimerEventArgs(tag));
		}
	}

	/// <summary>
	/// ObjectTimer.Elapsedイベントを処理するメソッドを表す
	/// </summary>
	public delegate void ObjectTimerEventHandler(object sender, ObjectTimerEventArgs e);

	/// <summary>
	/// ObjectTimer.Elapsedイベントのデータを提供
	/// </summary>
	public class ObjectTimerEventArgs : EventArgs
	{
		private readonly object tag;

		/// <summary>
		/// タグを取得
		/// </summary>
		public object Tag {
			get { return tag; }
		}

		/// <summary>
		/// ObjectTimerEventArgsクラスのインスタンスを初期化
		/// </summary>
		/// <param name="obj"></param>
		public ObjectTimerEventArgs(object obj)
		{
			tag = obj;
		}
	}
}
