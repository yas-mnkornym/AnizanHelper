// ThreadReaderBase.cs

namespace Twin.IO
{
	using System;
	using System.Text;
	using System.IO;
	using System.Collections;
	using System.Net;
	using Twin.Text;
	using System.Threading;

	/// <summary>
	/// スレッドを読み込むための基本クラス
	/// </summary>
	public abstract class ThreadReaderBase : ThreadReader
	{
		private string uagent;

		protected Stream baseStream;
		protected ThreadParser dataParser;

		private byte[] _buffer;
		private int buffSize;

		protected bool isOpen;
		protected int index;
		protected int length;
		protected int position;

		protected bool canceled = false;

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
					_buffer = new byte[BufferSize];

				return _buffer;
			}
		}

		/// <summary>
		/// スレッドのデータサイズ
		/// </summary>
		public override int Length
		{
			get
			{
				return length;
			}
		}

		/// <summary>
		/// ストリームの現在位置
		/// </summary>
		public override int Position
		{
			get
			{
				return position;
			}
		}

		/// <summary>
		/// ストリームの読み書きに使用するバッファサイズを取得または設定
		/// </summary>
		public override int BufferSize
		{
			set
			{
				buffSize = Math.Max(4096, value);
			}
			get
			{
				return buffSize;
			}
		}

		/// <summary>
		/// 読み込み中かどうかを判断
		/// </summary>
		public override bool IsOpen
		{
			get
			{
				return isOpen;
			}
		}

		/// <summary>
		/// User-Agentを取得または設定
		/// </summary>
		public override string UserAgent
		{
			set
			{
				if (value == null)
				{
					throw new ArgumentNullException("UserAgent");
				}
				uagent = value;
			}
			get
			{
				return uagent;
			}
		}

		/// <summary>
		/// ThreadReaderBase
		/// </summary>
		private ThreadReaderBase()
		{
			uagent = TwinDll.UserAgent;
			buffSize = 32768;
			index = 1;
			isOpen = false;
		}

		/// <summary>
		/// ThreadReaderBaseクラスのインスタンスを初期化
		/// </summary>
		/// <param name="parser">読み込み時に解析を行うためのパーサー</param>
		protected ThreadReaderBase(ThreadParser parser)
			: this()
		{
			if (parser == null)
			{
				throw new ArgumentNullException("parser");
			}
			dataParser = parser;
		}

		/// <summary>
		/// レスを読み込む
		/// </summary>
		/// <param name="resSets"></param>
		/// <returns>読み込まれた総byte数を返す</returns>
		public override int Read(ResSetCollection resSets)
		{
			int temp;
			return Read(resSets, out temp);
		}

		/// <summary>
		/// レスを読み込む
		/// </summary>
		/// <param name="resSets"></param>
		/// <param name="byteParsed">解析されたバイト数が格納される</param>
		/// <returns>読み込まれた総byte数を返す</returns>
		public override int Read(ResSetCollection resSets, out int byteParsed)
		{
			if (resSets == null)
			{
				throw new ArgumentNullException("resSets");
			}
			if (!isOpen)
			{
				throw new InvalidOperationException("ストリームが開かれていません");
			}

			// バッファにデータを読み込む
			int readCount = baseStream.Read(buffer, 0, buffer.Length);

			// 解析してコレクションに格納
			ResSet[] items = dataParser.Parse(buffer, readCount, out byteParsed);

			foreach (ResSet resSet in items)
			{
				ResSet res = resSet;
				res.Index = index++;
				resSets.Add(res);
			}

			// 実際に読み込まれたバイト数を計算
			position += readCount;

			return readCount;
		}

		public override void Cancel()
		{
			canceled = true;
		}

		/// <summary>
		/// ストリームを閉じる
		/// </summary>
		public override void Close()
		{
			canceled = false;

			if (isOpen)
			{
				isOpen = false;

				if (dataParser != null)
				{
					// 正常終了するときは必ず0になるはず
					//System.Diagnostics.Debug.Assert(dataParser.RemainderLength == 0);
					dataParser.Empty();
				}

				if (baseStream != null)
					baseStream.Close();

				position = 0;
				length = 0;
				index = 1;
				baseStream = null;
				_buffer = null;
			}
		}
	}
}
