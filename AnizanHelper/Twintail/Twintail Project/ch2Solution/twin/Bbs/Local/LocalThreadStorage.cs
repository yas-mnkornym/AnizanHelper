// LocalThreadStorage.cs

namespace Twin.IO
{
	using System;
	using System.IO;
	using System.Text;
	using System.Collections;
	using Twin.Text;
	using Twin.Bbs;

	/// <summary>
	/// ローカルディスクにスレッドを保存する機能を提供
	/// </summary>
	public class LocalThreadStorage : ThreadStorage
	{
		private Cache cache;
		private Stream baseStream;
		private ThreadParser dataParser;
		private ThreadFormatter formatter;
		private Encoding encoding;
		private StorageMode mode;
		private bool isOpen;

		private int position;
		private int index;

		/// <summary>
		/// ストリームを開いているかどうかを判断
		/// </summary>
		public override bool IsOpen {
			get { return isOpen; }
		}

		/// <summary>
		/// キャッシュデータの長さを取得
		/// </summary>
		public override int Length {
			get {
				if (isOpen) {
					return (int)baseStream.Length;
				}
				throw new InvalidOperationException("ストリームが開かれていません");
			}
		}

		/// <summary>
		/// 現在のストリームの位置を取得
		/// </summary>
		public override int Position {
			get {
				if (isOpen) {
					return position;
				}
				throw new InvalidOperationException("ストリームが開かれていません");
			}
		}

		/// <summary>
		/// バッファサイズを取得または設定
		/// </summary>
		public override int BufferSize {
			set {
				if (value < 512) {
					throw new ArgumentOutOfRangeException("BufferSize");
				}
				bufferSize = value;
			}
			get { return bufferSize; }
		}

		/// <summary>
		/// 読み込みモードで開かれているかどうかを表す
		/// </summary>
		public override bool CanRead {
			get { return (mode == StorageMode.Read); }
		}

		/// <summary>
		/// 書き込みモードで開かれているかどうかを表す
		/// </summary>
		public override bool CanWrite {
			get { return (mode == StorageMode.Write); }
		}

		/// <summary>
		/// LocalThreadStorageクラスのインスタンスを初期化
		/// </summary>
		/// <param name="cache">基になるキャッシュ情報</param>
		/// <param name="parser">データを解析するときに使用するパーサ</param>
		/// <param name="formatter">書き込み時に使用するフォーマッタ</param>
		/// <param name="enc">書き込み時に使用するエンコーダ</param>
		public LocalThreadStorage(Cache cache, ThreadParser parser, ThreadFormatter formatter, Encoding enc)
		{
			if (cache == null) {
				throw new ArgumentNullException("cache");
			}
			if (parser == null) {
				throw new ArgumentNullException("parser");
			}
			if (formatter == null) {
				throw new ArgumentNullException("formatter");
			}
			if (enc == null) {
				throw new ArgumentNullException("enc");
			}

			this.cache = cache;
			this.dataParser = parser;
			this.formatter = formatter;
			this.encoding = enc;
			this.bufferSize = 4096;
			this.isOpen = false;
		}

		/// <summary>
		/// LocalThreadStorageクラスのインスタンスを初期化
		/// </summary>
		/// <param name="cache"></param>
		public LocalThreadStorage(Cache cache)
			: this(cache, new X2chThreadParser(), new X2chThreadFormatter(), TwinDll.DefaultEncoding)
		{
		}

		/// <summary>
		/// LocalThreadStorageクラスのインスタンスを初期化すると同時に、ストレージを開く
		/// </summary>
		/// <param name="cache"></param>
		/// <param name="header"></param>
		/// <param name="mode"></param>
		public LocalThreadStorage(Cache cache, ThreadHeader header, StorageMode mode)
			: this(cache)
		{
			Open(header, mode);
		}

		/// <summary>
		/// 出力ストリームを開く
		/// </summary>
		/// <param name="header">書き込むスレッドのヘッダ情報</param>
		/// <param name="mode">ストレージを開く方法</param>
		public override bool Open(ThreadHeader header, StorageMode mode)
		{
			if (header == null) {
				throw new ArgumentNullException("header");
			}
			if (isOpen) {
				throw new InvalidCastException("既にストリームが開かれています");
			}

			// 書き込み先ファイル名
			string filePath = cache.GetDatPath(header);

			// ストリームを開く
			if (mode == StorageMode.Read) {
				baseStream = StreamCreator.CreateReader(filePath, header.UseGzip);
			} else {
				baseStream = StreamCreator.CreateWriter(filePath, header.UseGzip, true);
			}

			this.mode = mode;
			this.index = 1;
			this.position = 0;
			this.isOpen = true;

			return true;
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

		/// <summary>
		/// レスを読み込む
		/// </summary>
		/// <param name="resSets"></param>
		/// <returns></returns>
		public override int Read(ResSetCollection resSets, out int byteParsed)
		{
			if (resSets == null) {
				throw new ArgumentNullException("resSets");
			}
			if (!isOpen) {
				throw new InvalidOperationException("ストリームが開かれていません");
			}

			// バッファにデータを読み込む
			int readCount = baseStream.Read(buffer, 0, buffer.Length);

			// 解析してコレクションに格納
			ICollection collect = dataParser.Parse(buffer, readCount, out byteParsed);

			foreach (ResSet resSet in collect)
			{
				ResSet res = resSet;
				res.Index = index++;

				resSets.Add(res);
			}

			// 実際に読み込まれたバイト数を計算
			position += readCount;

			return readCount;
		}

		/// <summary>
		/// resCollectionをファイルに書き込む
		/// </summary>
		/// <param name="resCollection">スレッドの内容が格納されたResSetCollectionクラス</param>
		/// <returns>実際に書き込まれたバイト数</returns>
		public override int Write(ResSetCollection resCollection)
		{
			if (!CanWrite) {
				throw new NotSupportedException("書き込み用に開かれていません");
			}
			if (resCollection == null) {
				throw new ArgumentNullException("resCollection");
			}
			if (!isOpen) {
				throw new InvalidOperationException("ストリームが開かれていません");
			}

			string textData = formatter.Format(resCollection);
			byte[] byteData = encoding.GetBytes(textData);

			baseStream.Write(byteData, 0, byteData.Length);
			position += byteData.Length;

			return byteData.Length;
		}

		/// <summary>
		/// ストリームを閉じる
		/// </summary>
		public override void Close()
		{
			if (isOpen)
			{
				if (baseStream != null)
					baseStream.Close();

				baseStream = null;
				buffer = null;
				isOpen = false;
				position = 0;
			}
		}

		/// <summary>
		/// 使用しているリソースを解放
		/// </summary>
		public override void Dispose()
		{
			Close();
		}

		/// <summary>
		/// 指定したインデックス番号のレスのみを読み込む
		/// </summary>
		/// <param name="cache"></param>
		/// <param name="header"></param>
		/// <param name="indices"></param>
		/// <returns></returns>
		public static ResSetCollection ReadResSet(Cache cache, ThreadHeader header, int[] indices)
		{
			ResSetCollection items = new ResSetCollection();
			ResSetCollection temp = new ResSetCollection();

			using (LocalThreadStorage sto = new LocalThreadStorage(cache, header, StorageMode.Read))
			{
				while (sto.Read(temp) != 0);

				foreach (ResSet res in temp)
				{
					foreach (int index in indices)
					{
						if (res.Index == index)
							items.Add(res);
					}
				}

				temp.Clear();
				temp = null;
			}

			return items;
		}
	}
}
