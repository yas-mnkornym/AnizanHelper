// ThreadListReaderRelay.cs
// #2.0

namespace Twin.IO
{
	using System;
	using System.Collections.Generic;

	/// <summary>
	/// リーダーから読み取ったスレッド一覧をキャッシュするための中継クラスです。
	/// </summary>
	public class ThreadListReaderRelay : ThreadListReader
	{
		private Cache cache;						// キャッシュを管理します。
		private ThreadListStorage storage;			// ローカルにキャッシュを保存するクラスです。
		private ThreadListReader baseReader;		// ネットワークから最新のスレッド一覧を受信するクラスです。
		private List<ThreadHeader> cacheItems;		// キャッシュされているスレッド一覧です。
		private BoardInfo boardInfo;
		private bool isOpen;

		/// <summary>
		/// 基になるキャッシュ情報を取得します。
		/// </summary>
		public Cache Cache
		{
			get
			{
				return cache;
			}
		}

		/// <summary>
		/// 現在開かれている板の情報を取得します。
		/// </summary>
		public BoardInfo BoardInfo
		{
			get
			{
				return boardInfo;
			}
		}

		public override int Length
		{
			get
			{
				return baseReader.Length;
			}
		}

		public override int Position
		{
			get
			{
				return baseReader.Position;
			}
		}

		public override int BufferSize
		{
			set
			{
				baseReader.BufferSize = value;
			}
			get
			{
				return baseReader.BufferSize;
			}
		}

		public override bool IsOpen
		{
			get
			{
				return isOpen;
			}
		}

		public override string UserAgent
		{
			set
			{
				baseReader.UserAgent = value;
			}
			get
			{
				return baseReader.UserAgent;
			}
		}

		public override bool AutoRedirect
		{
			set
			{
				baseReader.AutoRedirect = value;
			}
			get
			{
				return baseReader.AutoRedirect;
			}
		}

		/// <summary>
		/// キャッシュされているスレッド一覧を取得します。
		/// </summary>
		public List<ThreadHeader> CacheItems
		{
			get
			{
				return cacheItems;
			}
		}

		/// <summary>
		/// ThreadListReaderRelayクラスのインスタンスを初期化。
		/// </summary>
		/// <param name="cache">基になるキャッシュ情報</param>
		/// <param name="baseReader">基になるリーダーのインスタンス</param>
		public ThreadListReaderRelay(Cache cache, ThreadListReader baseReader)
		{
			if (cache == null)
			{
				throw new ArgumentNullException("cache");
			}

			if (baseReader == null)
			{
				throw new ArgumentNullException("baseReader");
			}

			this.cache = cache;
			this.baseReader = baseReader;
			this.storage = new LocalThreadListStorage(cache);
			this.cacheItems = new List<ThreadHeader>();
			this.isOpen = false;
		}

		public override bool Open(BoardInfo board)
		{
			if (board == null)
			{
				throw new ArgumentNullException("board");
			}

			if (baseReader.IsOpen)
			{
				throw new InvalidOperationException("リーダーが開かれています");
			}

			cacheItems.Clear();
			boardInfo = board;

			// キャッシュをすべて読み込む
			if (storage.Open(board, StorageMode.Read))
			{
				while (storage.Read(cacheItems) != 0)
					;
				storage.Close();
			}

			// ストリームを開く
			if (baseReader.Open(board))
			{
				isOpen = storage.Open(board, StorageMode.Write);
			}

			return isOpen;
		}

		public override int Read(List<ThreadHeader> headers)
		{
			int byteParsed;
			return Read(headers, out byteParsed);
		}

		public override int Read(List<ThreadHeader> headers, out int byteParsed)
		{
			if (!isOpen)
			{
				throw new InvalidOperationException("ストリームが開かれていません");
			}

			int read = baseReader.Read(headers, out byteParsed);

			// 2010/12/05 水玉さんthx!
			if (byteParsed > 0)
			{
				storage.Write(headers);
			} 

			return read;
		}

		public override void Cancel()
		{
			if (baseReader != null && baseReader.IsOpen)
			{
				baseReader.Cancel();
			}
		}

		public override void Close()
		{
			isOpen = false;
			baseReader.Close();
			storage.Close();
		}
	}
}
