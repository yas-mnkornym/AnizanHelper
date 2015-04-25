// ThreadListStorage.cs
// #2.0

namespace Twin.IO
{
	using System;
	using System.Collections.Generic;

	/// <summary>
	/// ローカルにスレッド一覧を保存・管理するインターフェース。
	/// </summary>
	public abstract class ThreadListStorage : IDisposable
	{
		private byte[] _buffer;
		protected int bufferSize = 4096;

		protected byte[] buffer
		{
			set
			{
				_buffer = null;
				_buffer = value;
			}
			get
			{
				if (_buffer == null)
					_buffer = new byte[bufferSize];

				return _buffer;
			}
		}

		/// <summary>
		/// ファイルが開かれていれば true、閉じられていれば false を返します。
		/// </summary>
		public abstract bool IsOpen
		{
			get;
		}

		/// <summary>
		/// キャッシュデータのサイズを返します。
		/// </summary>
		public abstract int Length
		{
			get;
		}

		/// <summary>
		/// 現在のストリームの位置を返します。
		/// </summary>
		public abstract int Position
		{
			get;
		}

		/// <summary>
		/// 読み込みモードで開かれているかどうかを表します。
		/// </summary>
		public abstract bool CanRead
		{
			get;
		}

		/// <summary>
		/// 書き込みモードで開かれているかどうかを表します。
		/// </summary>
		public abstract bool CanWrite
		{
			get;
		}

		/// <summary>
		/// 指定した板のキャッシュを、StorageMode.Read なら読みとりモード、
		/// StorageMode.Write なら書き込みモードで開きます。
		/// </summary>
		/// <param name="board"></param>
		/// <param name="mode"></param>
		/// <returns></returns>
		public abstract bool Open(BoardInfo board, StorageMode mode);

		/// <summary>
		/// キャッシュからスレッド一覧を読み込み headerList に格納します。
		/// 読みとりモードで開かれている必要があります。
		/// </summary>
		/// <param name="headerList"></param>
		/// <returns></returns>
		public abstract int Read(List<ThreadHeader> headerList);

		/// <summary>
		/// キャッシュからスレッド一覧を読み込み headerList に格納します。
		/// 読みとりモードで開かれている必要があります。
		/// </summary>
		/// <param name="headerList"></param>
		/// <param name="byteParsed"></param>
		/// <returns></returns>
		public abstract int Read(List<ThreadHeader> headerList, out int byteParsed);

		/// <summary>
		/// headerList に格納されているスレッド一覧をキャッシュに書き込みます。
		/// 書き込みモードで開かれている必要があります。
		/// </summary>
		public abstract int Write(List<ThreadHeader> headerList);

		/// <summary>
		/// Open メソッドで開かれているストリームを閉じます。
		/// </summary>
		public abstract void Close();

		/// <summary>
		/// 使用しているリソースを解放し、開かれているストリームを閉じます。
		/// </summary>
		public abstract void Dispose();
	}
}
