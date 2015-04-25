// ThreadListReader.cs
// #2.0

namespace Twin.IO
{
	using System;
	using System.Collections.Generic;

	/// <summary>
	/// スレッド一覧を読み込むインターフェースです。
	/// </summary>
	public abstract class ThreadListReader
	{
		/// <summary>
		/// 受信用バッファサイズを取得または設定します。
		/// </summary>
		public abstract int BufferSize
		{
			set;
			get;
		}

		/// <summary>
		/// データの長さを取得します。
		/// </summary>
		public abstract int Length
		{
			get;
		}

		/// <summary>
		/// ストリームの現在位置を取得します。
		/// </summary>
		public abstract int Position
		{
			get;
		}

		/// <summary>
		/// リーダーが開かれていれば true、それ以外は false を返します。
		/// </summary>
		public abstract bool IsOpen
		{
			get;
		}

		/// <summary>
		/// User-Agent を取得または設定します。
		/// </summary>
		public abstract string UserAgent
		{
			set;
			get;
		}

		/// <summary>
		/// 板が移転していた場合に自動で追尾するかどうかを示す値を取得または設定します。
		/// </summary>
		public abstract bool AutoRedirect {
			set;
			get;
		}

		/// <summary>サーバーが移転したときに発生します。</summary>
		public event EventHandler<ServerChangeEventArgs> ServerChange;

		/// <summary>
		/// 指定した板を開きます。
		/// </summary>
		/// <param name="board"></param>
		public abstract bool Open(BoardInfo board);

		/// <summary>
		/// データを受信し解析されたデータを items に追加します。
		/// </summary>
		/// <returns>読み込まれたバイト数を返します。</returns>
		public abstract int Read(List<ThreadHeader> items);

		/// <summary>
		/// データを受信し解析されたデータを items に追加します。解析されたバイト数が byteParsed に格納されます。
		/// </summary>
		/// <returns>読み込まれたバイト数を返します。</returns>
		public abstract int Read(List<ThreadHeader> items, out int byteParsed);

		/// <summary>
		/// 通信処理をキャンセルします。
		/// </summary>
		public abstract void Cancel();

		/// <summary>
		/// 使用しているリソースを解放し、開いているストリームを閉じます。
		/// </summary>
		public abstract void Close();

		/// <summary>
		/// ServerChangeイベントを発生させます。
		/// </summary>
		protected void OnServerChange(ServerChangeEventArgs e)
		{
			if (ServerChange != null)
				ServerChange(this, e);
		}
	}
}
