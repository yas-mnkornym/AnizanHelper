// ThreadStorage.cs

namespace Twin.IO
{
	using System;

	/// <summary>
	/// ローカルにスレッドを保存・管理するインターフェース
	/// </summary>
	public abstract class ThreadStorage : IDisposable
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
		/// ファイルが開かれているかどうかを判断
		/// </summary>
		public abstract bool IsOpen
		{
			get;
		}

		/// <summary>
		/// キャッシュデータのサイズ
		/// </summary>
		public abstract int Length
		{
			get;
		}

		/// <summary>
		/// 現在のストリームの位置
		/// </summary>
		public abstract int Position
		{
			get;
		}

		/// <summary>
		/// バッファサイズを取得または設定
		/// </summary>
		public abstract int BufferSize
		{
			set;
			get;
		}

		/// <summary>
		/// 読み込みモードで開かれているかどうかを表す
		/// </summary>
		public abstract bool CanRead
		{
			get;
		}

		/// <summary>
		/// 書き込みモードで開かれているかどうかを表す
		/// </summary>
		public abstract bool CanWrite
		{
			get;
		}

		/// <summary>
		/// スレッドを開く
		/// </summary>
		/// <param name="header"></param>
		public abstract bool Open(ThreadHeader header, StorageMode mode);

		/// <summary>
		/// resCollectionにキャッシュ情報を読み込む
		/// </summary>
		/// <param name="resCollection"></param>
		/// <returns></returns>
		public abstract int Read(ResSetCollection resCollection);

		/// <summary>
		/// resCollectionにキャッシュ情報を読み込む
		/// </summary>
		/// <param name="resCollection"></param>
		/// <returns></returns>
		public abstract int Read(ResSetCollection resCollection, out int byteParsed);

		/// <summary>
		/// resCollectionをファイルに書き込む
		/// </summary>
		public abstract int Write(ResSetCollection resCollection);

		/// <summary>
		/// ストリームを閉じる
		/// </summary>
		public abstract void Close();

		/// <summary>
		/// 使用しているリソースを解放
		/// </summary>
		public abstract void Dispose();
	}
}
