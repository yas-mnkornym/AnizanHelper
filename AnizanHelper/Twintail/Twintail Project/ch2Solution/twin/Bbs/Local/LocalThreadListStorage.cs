// LocalThreadListStorage.cs

namespace Twin.IO
{
	using System;
	using System.IO;
	using System.Text;
	using System.Collections.Generic;
	using Twin.Text;
	using Twin.Bbs;

	/// <summary>
	/// ローカルディスクにスレッド一覧を保存する機能を提供
	/// </summary>
	public class LocalThreadListStorage : ThreadListStorage
	{
		private Cache cache;
		private Stream baseStream;
		private ThreadListParser dataParser;
		private ThreadListFormatter formatter;
		private Encoding encoding;
		private StorageMode mode;
		private BoardInfo boardInfo;

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
		/// LocalThreadListStorageクラスのインスタンスを初期化
		/// </summary>
		/// <param name="cache">基になるキャッシュ情報</param>
		/// <param name="parser">データを解析するときに使用するパーサ</param>
		/// <param name="formatter">書き込み時に使用するフォーマッタ</param>
		/// <param name="enc">書き込み時に使用するエンコーダ</param>
		public LocalThreadListStorage(Cache cache, ThreadListFormatter formatter, Encoding enc)
		{
			if (cache == null) {
				throw new ArgumentNullException("cache");
			}
			if (formatter == null) {
				throw new ArgumentNullException("formatter");
			}
			if (enc == null) {
				throw new ArgumentNullException("enc");
			}

			this.cache = cache;
			this.formatter = formatter;
			this.encoding = enc;
			this.isOpen = false;
		}

		/// <summary>
		/// LocalThreadListStorageクラスのインスタンスを初期化
		/// </summary>
		/// <param name="cache"></param>
		public LocalThreadListStorage(Cache cache)
			: this(cache, new X2chThreadListFormatter(), Encoding.GetEncoding("Shift_Jis"))
		{
		}

		/// <summary>
		/// ストリームを開く
		/// </summary>
		/// <param name="board">書き込む板のヘッダ情報</param>
		/// <param name="modeRead">ストレージを開く方法</param>
		public override bool Open(BoardInfo board, StorageMode mode)
		{
			if (board == null) {
				throw new ArgumentNullException("board");
			}
			if (isOpen) {
				throw new InvalidCastException("既にストリームが開かれています");
			}

			// 書き込み先ファイル名
			string filePath = Path.Combine(cache.GetFolderPath(board), "subject.txt");

			// ストリームを開く
			if (mode == StorageMode.Read) {
				baseStream = StreamCreator.CreateReader(filePath, false);
			} else {
				baseStream = StreamCreator.CreateWriter(filePath, false, false);
			}

			// パーサを初期化
			dataParser = new X2chThreadListParser(board.Bbs, encoding);

			this.index = 1;
			this.position = 0;
			this.mode = mode;
			this.isOpen = true;
			this.boardInfo = board;

			return true;
		}

		/// <summary>
		/// ディスクにキャッシュしながらスレッド一覧を読み込む
		/// </summary>
		/// <param name="headers"></param>
		/// <returns></returns>
		public override int Read(List<ThreadHeader> headers)
		{
			int byteParsed;
			return Read(headers, out byteParsed);
		}

		/// <summary>
		/// スレッド情報を読み込む
		/// </summary>
		/// <param name="headerCollects"></param>
		/// <returns></returns>
		public override int Read(List<ThreadHeader> headerCollects, out int byteParsed)
		{
			if (headerCollects == null) {
				throw new ArgumentNullException("headerCollects");
			}
			if (!isOpen) {
				throw new InvalidOperationException("ストリームが開かれていません");
			}

			// バッファにデータを読み込む
			int readCount = baseStream.Read(buffer, 0, buffer.Length);

			// 解析してコレクションに格納
			Array array = dataParser.Parse(buffer, readCount, out byteParsed);

			foreach (ThreadHeader header in array)
			{
				header.No = index++;
				header.BoardInfo = boardInfo;
				headerCollects.Add(header);
			}

			// 実際に読み込まれたバイト数を計算
			position += readCount;

			return readCount;
		}

		/// <summary>
		/// resCollectionをファイルに書き込む
		/// </summary>
		/// <param name="headerCollects">スレッドの内容が格納されたList<ThreadHeader>クラス</param>
		/// <returns>実際に書き込まれたバイト数</returns>
		public override int Write(List<ThreadHeader> headerCollects)
		{
			if (!CanWrite) {
				throw new NotSupportedException("書き込み用に開かれていません");
			}
			if (headerCollects == null) {
				throw new ArgumentNullException("headerCollects");
			}
			if (!isOpen) {
				throw new InvalidOperationException("ストリームが開かれていません");
			}

			string textData = formatter.Format(headerCollects);
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
			if (baseStream != null)
				baseStream.Close();

			dataParser = null;
			baseStream = null;
			boardInfo = null;
			buffer = null;
			position = 0;
			isOpen = false;
		}

		/// <summary>
		/// 使用しているリソースを解放
		/// </summary>
		public override void Dispose()
		{
			Close();
		}
	}
}
