// OfflineThreadListReader.cs

namespace Twin.IO
{
	using System;
	using System.IO;
	using System.Collections.Generic;

	/// <summary>
	/// 既得済みスレッドの一覧を読み込む機能を提供
	/// </summary>
	public class OfflineThreadListReader : ThreadListReaderBase
	{
		private Cache cache;
		private string[] indexFiles;

		/// <summary>
		/// OfflineThreadListReaderクラスのインスタンスを初期化
		/// </summary>
		public OfflineThreadListReader(Cache log) : base(null)
		{
			if (log == null) {
				throw new ArgumentNullException("log");
			}
			// 
			// TODO: コンストラクタ ロジックをここに追加してください。
			//
			cache = log;
		}

		/// <summary>
		/// ローカルに保存された一覧を検索
		/// </summary>
		/// <param name="board"></param>
		public override bool Open(BoardInfo board)
		{
			if (board == null) {
				throw new ArgumentNullException("board");
			}
			if (isOpen) {
				throw new InvalidOperationException("一覧を読み込み中です");
			}

			// ログディレクトリ
			string folder = cache.GetFolderPath(board);

			// すべてのインデックスファイルを検索
			indexFiles = Directory.GetFiles(folder, "*.idx");
			length = indexFiles.Length;
			boardinfo = board;
			position = 0;
			isOpen = true;

			return isOpen;
		}

		/// <summary>
		/// ローカルに存在するすべてのスレッド情報を検索
		/// </summary>
		/// <param name="headers"></param>
		/// <returns></returns>
		public override int Read(List<ThreadHeader> headers)
		{
			int temp;
			return Read(headers, out temp);
		}

		/// <summary>
		/// ローカルに存在するすべてのスレッド情報を検索
		/// </summary>
		/// <param name="headers"></param>
		/// <returns></returns>
		public override int Read(List<ThreadHeader> headers, out int cntParsed)
		{
			if (!isOpen) {
				throw new InvalidOperationException("開かれていません");
			}
			if (headers == null) {
				throw new ArgumentNullException("headers");
			}

			int max = Math.Min(position + 16, length);
			int temp = position;

			while (position < max)
			{
				ThreadHeader head = TypeCreator.CreateThreadHeader(boardinfo.Bbs);
				head.BoardInfo = boardinfo;
				head.Key = Path.GetFileNameWithoutExtension(indexFiles[position]);

				ThreadIndexer.Read(cache, head);
				headers.Add(head);
				position++;
			}

			cntParsed = (position - temp);
			return cntParsed;
		}
	}
}
