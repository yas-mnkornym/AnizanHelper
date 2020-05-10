// DefaultPatroller.cs

namespace Twin.Tools
{
	using System;
	using System.IO;
	using System.Text;
	using System.Net;
	using System.Collections;
	using Twin.IO;
	using Twin.Bbs;
	using CSharpSamples;

	/// <summary>
	/// デフォルトの巡回クラス
	/// </summary>
	public class DefaultPatroller : PatrolBase
	{
		/// <summary>
		/// DefaultPatrollerクラスのインスタンスを初期化
		/// </summary>
		public DefaultPatroller(Cache cacheInfo) : base(cacheInfo)
		{
			// 
			// TODO: コンストラクタ ロジックをここに追加してください。
			//
		}

		/// <summary>
		/// 巡回開始
		/// </summary>
		public override void Patrol()
		{
			try {
				ThreadReaderRelay reader = null;
				BbsType  bbs = BbsType.None;

				foreach (ThreadHeader header in Items)
				{
					if (header.Pastlog || header.IsLimitOverThread)
						continue;

					ResSetCollection temp = new ResSetCollection();
					BoardInfo board = header.BoardInfo;

					// リーダーを作成
					if (bbs != board.Bbs)
					{
						reader = null;
						reader = new ThreadReaderRelay(Cache, TypeCreator.CreateThreadReader(board.Bbs));
						bbs = board.Bbs;
					}

					// 取得前の新着を保存
					int newResCount = header.NewResCount;

					try {
						OnStatusTextChanged(header.Subject + " の巡回中...");

						ClientBase.Connect.WaitOne();

						PatrolEventArgs e = new PatrolEventArgs(header);
						OnPatroling(e);

						if (!e.Cancel)
						{
							// 新着のみを読み取る
							reader.ReadCache = false;

							if (reader.Open(header))
								while (reader.Read(temp) != 0);
						}
					}
					catch {}
					finally {
						if (reader != null)
							reader.Close();
						ClientBase.Connect.Set();
					}

					if (header.NewResCount > 0)
					{
						// 前回の新着を足して、インデックスを保存
						header.NewResCount += newResCount;
						OnUpdated(new PatrolEventArgs(header));
					}
				}
			}
			finally {
				OnStatusTextChanged("巡回を完了しました");
			}
		}
	}
}
