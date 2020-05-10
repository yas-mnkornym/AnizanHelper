// ThreadReaderRelay.cs

namespace Twin.IO
{
	using System;
	using System.IO;
	using System.Text;
	using System.Diagnostics;
	using System.Net;

	using Twin.Bbs;

	/// <summary>
	/// リーダーから読み取ったスレッドをキャッシュするための中継
	/// </summary>
	public class ThreadReaderRelay : ThreadReader
	{
		private Cache cache;
		private ThreadStorage storage;
		private ThreadReader baseReader;
		private ThreadHeader headerInfo;
		private bool isOpen;
		private bool readCache;

		private int length;
		private int position;

		/// <summary>
		/// 基になるキャッシュ情報を取得
		/// </summary>
		public Cache Cache
		{
			get
			{
				return cache;
			}
		}

		/// <summary>
		/// スレッドのヘッダ情報を取得
		/// </summary>
		public ThreadHeader Header
		{
			get
			{
				return headerInfo;
			}
		}

		/// <summary>
		/// データの長さを取得
		/// </summary>
		public override int Length
		{
			get
			{
				return length;
			}
		}

		/// <summary>
		/// ストリームの現在位置を取得
		/// </summary>
		public override int Position
		{
			get
			{
				return position;
			}
		}

		/// <summary>
		/// ストリームの読み込みに使用するバッファサイズを取得または設定
		/// </summary>
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

		/// <summary>
		/// ファイルが開かれているかどうかを取得
		/// </summary>
		public override bool IsOpen
		{
			get
			{
				return isOpen;
			}
		}

		/// <summary>
		/// キャッシュを読み込むかどうかを取得または設定
		/// </summary>
		public bool ReadCache
		{
			set
			{
				if (readCache != value)
					readCache = value;
			}
			get
			{
				return readCache;
			}
		}

		/// <summary>
		/// User-Agentを取得または設定
		/// </summary>
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

		/// <summary>
		/// 差分取得時にあぼーんを検出したときに発生
		/// </summary>
		public override event EventHandler ABone
		{
			add
			{
				baseReader.ABone += value;
			}
			remove
			{
				baseReader.ABone -= value;
			}
		}

		/// <summary>
		/// dat落ちしているときに発生
		/// </summary>
		public override event EventHandler<PastlogEventArgs> Pastlog
		{
			add
			{
				baseReader.Pastlog += value;
			}
			remove
			{
				baseReader.Pastlog -= value;
			}
		}

		/// <summary>
		/// キャッシュの読み込みを完了したときに発生
		/// </summary>
		public event EventHandler CacheComplete;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="cache"></param>
		/// <param name="baseReader"></param>
		/// <param name="storage"></param>
		protected ThreadReaderRelay(Cache cache,
			ThreadReader baseReader,
			ThreadStorage storage)
		{
			if (cache == null)
			{
				throw new ArgumentNullException("cache");
			}
			if (baseReader == null)
			{
				throw new ArgumentNullException("baseReader");
			}
			if (storage == null)
			{
				throw new ArgumentNullException("storage");
			}

			this.cache = cache;
			this.storage = storage;
			this.baseReader = baseReader;
			this.isOpen = false;
			this.readCache = true;
		}

		/// <summary>
		/// ThreadReaderRelayクラスのインスタンスを初期化
		/// </summary>
		/// <param name="cache">基になるキャッシュ情報</param>
		/// <param name="baseReader">基になるリーダーのインスタンス</param>
		public ThreadReaderRelay(Cache cache, ThreadReader baseReader)
			: this(cache, baseReader, new LocalThreadStorage(cache))
		{
			// 
			// TODO: コンストラクタ ロジックをここに追加してください。
			//
		}

		/// <summary>
		/// スレッドを開く
		/// </summary>
		/// <param name="th"></param>
		public override bool Open(ThreadHeader header)
		{
			if (header == null)
			{
				throw new ArgumentNullException("header");
			}

retry:

			// ヘッダが存在する場合はキャッシュストリームを開く
			if (ThreadIndexer.Exists(cache, header))
			{
				int resCount = header.ResCount;

				ThreadIndexer.Read(cache, header);
				header.ResCount = resCount;

				if (readCache)
				{
					storage.Open(header, StorageMode.Read);
					storage.BufferSize = BufferSize;
					length = storage.Length;
					isOpen = storage.IsOpen;
				}
			}

			if (!storage.IsOpen)
			{
				try
				{
					isOpen = OpenBaseStream(header);
				}
				catch (X2chRetryKakologException ex)
				{
					header.BoardInfo = ex.RetryBoard;
					goto retry;
				}
			}

			headerInfo = header;
			headerInfo.NewResCount = 0;

			position = 0;

			return isOpen;
		}

		/// <summary>
		/// ディスクにキャッシュしながらレスを読み込む
		/// </summary>
		/// <param name="resSets"></param>
		/// <param name="byteParsed">解析された総バイト数が格納される</param>
		/// <returns>読み込まれたバイト数を返す</returns>
		public override int Read(ResSetCollection resSets, out int byteParsed)
		{
			if (!isOpen)
			{
				throw new InvalidOperationException("ストリームが開かれていません");
			}

			byteParsed = 0;

			ResSetCollection tempCollection = new ResSetCollection();
			int byteCount = 0;
			int writeCount = 0;

			// キャッシュを読む
			if (storage.IsOpen && storage.CanRead)
			{
				byteCount = storage.Read(tempCollection, out byteParsed);
				tempCollection.IsNew = false;

				// データがなければキャッシュストリームは閉じる
				if (byteCount == 0)
				{
					storage.Close();
					OnCacheComplete(this, new EventArgs());
					OpenBaseStream(headerInfo);
				}
			}

			// キャッシュがなければ実際に基本ストリームから読み込む
			if (baseReader.IsOpen)
			{
				byteCount = baseReader.Read(tempCollection, out byteParsed);
				tempCollection.IsNew = true;

				// あぼーんがあった場合、処理を中止。
				if (byteCount == -1)
					return -1;

				try
				{
					if (storage.IsOpen)
						writeCount = storage.Write(tempCollection);
				}
				finally
				{
					headerInfo.GotByteCount += byteParsed;
					headerInfo.GotResCount += tempCollection.Count;
					headerInfo.NewResCount += tempCollection.Count;
				}
			}

			resSets.AddRange(tempCollection);
			position += byteCount;

			return byteCount;
		}

		/// <summary>
		/// ディスクにキャッシュしながらスレッド一覧を読み込む
		/// </summary>
		/// <param name="resSets"></param>
		/// <returns></returns>
		public override int Read(ResSetCollection resSets)
		{
			int byteParsed;
			return Read(resSets, out byteParsed);
		}

		public override void Cancel()
		{
			if (baseReader != null)
			{
				baseReader.Cancel();
			}
		}

		/// <summary>
		/// ストリームを閉じインデックス情報を保存
		/// </summary>
		public override void Close()
		{
			try
			{
				if (isOpen)
				{
					// スレッドのインデックスを作成
					bool success = ThreadIndexer.Write(cache, headerInfo);
					Debug.Assert(success);

					// 既得インデックスを作成
					GotThreadListIndexer.Write(Cache, headerInfo);
				}
			}
			finally
			{
				isOpen = false;
				length = position = 0;

				storage.Close();
				baseReader.Close();
			}
		}

		/// <summary>
		/// 基本ストリームを開く
		/// </summary>
		protected bool OpenBaseStream(ThreadHeader headerInfo)
		{
			if (baseReader.IsOpen)
				return false;

			// 過去ログ落ちしていない場合のみ開く
			//0324 if (!headerInfo.Pastlog)
			{
				baseReader.Open(headerInfo);

				// スレッドを開くのに成功した場合、ストレージも開く
				if (baseReader.IsOpen)
				{
					length = baseReader.Length;
					position = 0;
					return storage.Open(headerInfo, StorageMode.Write);
				}
			}
			return false;
		}

		/// <summary>
		/// CacheCompleteイベントを発生させる
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void OnCacheComplete(object sender, EventArgs e)
		{
			if (CacheComplete != null)
				CacheComplete(sender, e);
		}
	}
}
