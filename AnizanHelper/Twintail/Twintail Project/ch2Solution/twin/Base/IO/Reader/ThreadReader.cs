// ThreadReader.cs

namespace Twin.IO
{
	using System;
	using System.Collections;
	using System.Net;

	/// <summary>
	/// 掲示板のスレッドを読み込むインターフェース
	/// </summary>
	public abstract class ThreadReader
	{
		/// <summary>
		/// データの長さを取得
		/// </summary>
		public abstract int Length
		{
			get;
		}

		/// <summary>
		/// ストリームの現在位置を取得
		/// </summary>
		public abstract int Position
		{
			get;
		}

		/// <summary>
		/// 読み込みに使用するバッファサイズを取得または設定
		/// </summary>
		public abstract int BufferSize
		{
			set;
			get;
		}

		/// <summary>
		/// ファイルが開かれているかどうかを取得
		/// </summary>
		public abstract bool IsOpen
		{
			get;
		}

		/// <summary>
		/// User-Agentを取得または設定
		/// </summary>
		public abstract string UserAgent
		{
			set;
			get;
		}

		/// <summary>
		/// 差分取得時にあぼーんを検出したときに発生
		/// </summary>
		public virtual event EventHandler ABone;

		/// <summary>
		/// スレッドを取得できなかったときに発生
		/// </summary>
		public virtual event EventHandler<PastlogEventArgs> Pastlog;

		/// <summary>
		/// スレッドを開く
		/// </summary>
		/// <param name="header"></param>
		public abstract bool Open(ThreadHeader header);

		/// <summary>
		/// データを解析してresCollectionに格納
		/// </summary>
		/// <returns>読み込まれたバイト数。末端の場合は 0。あぼーんを検知した場合は -1 を返す</returns>
		public abstract int Read(ResSetCollection resCollection);

		/// <summary>
		/// データを解析してresCollectionに格納
		/// </summary>
		/// <returns>読み込まれたバイト数。末端の場合は 0。あぼーんを検知した場合は -1 を返す</returns>
		public abstract int Read(ResSetCollection resCollection, out int byteParsed);

		/// <summary>
		/// 読み込みストリームを閉じる
		/// </summary>
		public abstract void Close();

		/// <summary>
		/// 通信処理をキャンセルします。
		/// </summary>
		public abstract void Cancel();

		/// <summary>
		/// ABoneイベントを発生させる
		/// </summary>
		protected void OnABone()
		{
			if (ABone != null)
				ABone(this, EventArgs.Empty);
		}

		/// <summary>
		/// Pastlogイベントを発生させる
		/// </summary>
		protected void OnPastlog(PastlogEventArgs argument)
		{
			if (Pastlog != null)
				Pastlog(this, argument);
		}
	}

	public delegate void KakologEventHandler(bool gzipCompress);

	public class AboneEventArgs : EventArgs
	{
		private bool reget = false;
		/// <summary>
		/// ログを再取得するかどうかを示す値を取得または設定します。
		/// </summary>
		public bool IsReget
		{
			get
			{
				return reget;
			}
			set
			{
				reget = value;
			}
		}
	
		public AboneEventArgs()
		{
		}
	}
}
