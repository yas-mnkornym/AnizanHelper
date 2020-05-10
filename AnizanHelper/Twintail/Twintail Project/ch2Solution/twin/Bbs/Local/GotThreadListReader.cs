// GotThreadListReader.cs
// #2.0

namespace Twin.IO
{
	using System;
	using System.Text;
	using System.Collections.Generic;
	using System.IO;
	using Twin;
	using Twin.Text;

	/// <summary>
	/// 既得済みスレッドの一覧を読み込む機能を提供
	/// (OfflineThreadListReaderより早い)
	/// </summary>
	public class GotThreadListReader : ThreadListReaderBase
	{
		private Cache cache;
		private BoardInfo boardInfo;
		private List<ThreadHeader> items;

		/// <summary>
		/// GotThreadListReaderクラスのインスタンスを初期化
		/// </summary>
		public GotThreadListReader(Cache log)
			: base(null)
		{
			if (log == null)
			{
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
			if (board == null)
			{
				throw new ArgumentNullException("board");
			}
			if (isOpen)
			{
				throw new InvalidOperationException("一覧を読み込み中です");
			}

			// ログディレクトリ
			string folder = cache.GetFolderPath(board);

			// インデックスファイルを読み込む
			items = GotThreadListIndexer.Read(cache, board);
			length = items.Count;

			position = 0;
			boardInfo = board;
			isOpen = true;

			return isOpen;
		}

		/// <summary>
		/// ディスクにキャッシュしながらスレッド一覧を読み込む
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
		/// <param name="headers">解析されたヘッダが格納されるコレクション</param>
		/// <param name="cntParsed">解析されたヘッダ数が格納される (この値は戻り値と同じになる)</param>
		/// <returns>読み込まれたヘッダ数を返す</returns>
		public override int Read(List<ThreadHeader> headers, out int cntParsed)
		{
			if (!isOpen)
			{
				throw new InvalidOperationException("開かれていません");
			}
			if (headers == null)
			{
				throw new ArgumentNullException("headers");
			}

			int max = Math.Min(position + 32, length);
			int temp = position;

			while (position < max)
			{
				headers.Add(items[position]);
				position++;
			}

			cntParsed = (position - temp);
			return cntParsed;
		}

		/// <summary>
		/// 指定したテーブルのすべての板の既得スレッド情報を取得
		/// </summary>
		/// <param name="table"></param>
		/// <returns></returns>
		public static List<ThreadHeader> GetAllThreads(Cache cache, IBoardTable table)
		{
			List<ThreadHeader> items = new List<ThreadHeader>();

			foreach (Category categ in table.Items)
			{
				foreach (BoardInfo board in categ.Children)
				{
					items.AddRange(GotThreadListIndexer.Read(cache, board));
				}
			}

			return items;
		}
	}
}
