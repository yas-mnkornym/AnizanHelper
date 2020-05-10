// ClientBase.cs

namespace Twin.Bbs
{
	using System;
	using System.Collections;
	using System.Threading;
	using System.Diagnostics;
	using System.Windows.Forms;

	/// <summary>
	/// 掲示板からデータを取得するための基本クラス。
	/// クライアント共通の機能とイベントなど。
	/// </summary>
	public abstract class ClientBase : Control
	{
		public static readonly ManualResetEvent Stopper = new ManualResetEvent(true);
		public static readonly AutoResetEvent Connect = new AutoResetEvent(true);

		private static bool connectionLimitter = false;
		public static bool ConnectionLimitter
		{
			get
			{
				return connectionLimitter;
			}
			set
			{
				connectionLimitter = value;
			}
		}
	
		/// <summary>
		/// スレッドの優先順位を取得または設定
		/// </summary>
		public static ThreadPriority Priority = ThreadPriority.Normal;

		/// <summary>
		/// キャッシュ情報を管理するクラスを取得
		/// </summary>
		public readonly Cache Cache;

		/// <summary>
		/// 読み込み開始時に発生
		/// </summary>
		public event EventHandler Loading;

		/// <summary>
		/// データ受信時に発生
		/// </summary>
		public event ReceiveEventHandler Receive;

		/// <summary>
		/// 読み込み完了時に発生
		/// </summary>
		public event CompleteEventHandler Complete;

		/// <summary>
		/// ステータスが変更された時に発生
		/// </summary>
		public event StatusTextEventHandler StatusTextChanged;

		/// <summary>
		/// ClientBaseクラスのインスタンスを初期化
		/// </summary>
		/// <param name="cache">キャッシュ情報</param>
		protected ClientBase(Cache cache)
		{
			Cache = cache;
		}

		/// <summary>
		/// Loadingイベントを発生させる
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected virtual void OnLoading(EventArgs e)
		{
			Stopper.WaitOne();

			if (Loading != null)
			{
				if (InvokeRequired)
				{
					Invoke(new EventHandler(Loading), new object[] {this, e});
				}
				else {
					Loading(this, e);
				}
			}

			if (ConnectionLimitter)
				Connect.WaitOne();
		}

		/// <summary>
		/// Receiveイベントを発生させる
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected virtual void OnReceive(ReceiveEventArgs e)
		{
			Stopper.WaitOne();

			if (Receive != null)
			{
				if (InvokeRequired)
				{
					Invoke(new ReceiveEventHandler(Receive), new object[] {this, e});
				}
				else {
					Receive(this, e);
				}
			}
		}

		/// <summary>
		/// Completeイベントを発生させる
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected virtual void OnComplete(CompleteEventArgs e)
		{
			Stopper.WaitOne();

			if (ConnectionLimitter)
				Connect.Set();

			if (Complete != null)
			{
				if (InvokeRequired)
				{
					Invoke(new CompleteEventHandler(Complete), new object[] {this, e});
				}
				else {
					Complete(this, e);
				}
			}
		}

		/// <summary>
		/// StatusTextChangedイベントを発生させる
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected virtual void OnStatusTextChanged(string text)
		{
			if (StatusTextChanged != null)
			{
				StatusTextEventArgs e =
					new StatusTextEventArgs(text);

				if (InvokeRequired)
				{
					Invoke(new StatusTextEventHandler(StatusTextChanged), 
						new object[] {this, e});
				}
				else {
					StatusTextChanged(this, e);
				}
			}
		}

		/// <summary>
		/// コントロールを選択
		/// </summary>
		public abstract void _Select();
	}
	
	public abstract class ClientBaseEx<THeader> : ClientBase
	{
		public abstract THeader HeaderInfo
		{
			get;
		}

		public ClientBaseEx(Cache cache)
			: base(cache)
		{
		}
	}
}
