// CheckOnlyPatroller.cs

namespace Twin.Tools
{
	using System;
	using System.Net;
	using System.Collections;
	using System.Collections.Generic;
	using Twin.IO;

	/// <summary>
	/// subject.txtの比較により更新チェックのみを行うクラス
	/// </summary>
	public class CheckOnlyPatroller : PatrolBase
	{
		/// <summary>
		/// CheckOnlyPatrollerクラスのインスタンスを初期化
		/// </summary>
		/// <param name="cache"></param>
		public CheckOnlyPatroller(Cache cache) : base(cache)
		{
			// 
			// TODO: コンストラクタ ロジックをここに追加してください。
			//
		}

		/// <summary>
		/// 巡回を開始 (更新チェックのみ)
		/// </summary>
		public override void Patrol()
		{
			// 更新対象の板をすべてコレクションに詰める
			Hashtable boardList = new Hashtable();

			foreach (ThreadHeader item1 in Items)
			{
				BoardInfo board = item1.BoardInfo;
				OnPatroling(new PatrolEventArgs(item1));

				// スレッド一覧を受信
				if (!boardList.Contains(board.Url))
				{
					OnStatusTextChanged(board.Url + "subject.txt を取得中...");

					List<ThreadHeader> headers = new List<ThreadHeader>();
					ThreadListReader listReader = TypeCreator.CreateThreadListReader(board.Bbs);
					
					listReader.ServerChange += 
						new EventHandler<ServerChangeEventArgs>(delegate (object sender, ServerChangeEventArgs e)
					{
						item1.BoardInfo.Server = board.Server = e.NewBoard.Server;
					});

					try {
						if (listReader.Open(board))
							while (listReader.Read(headers) != 0);
					}
					catch {}
					finally {
						if (listReader != null)
							listReader.Close();
					}

					boardList[board.Url] = headers;
				}

				// レス数が増えていれば更新されている、存在しなければdat落ち
				List<ThreadHeader> targetList = (List<ThreadHeader>)boardList[board.Url];
				int index = targetList.IndexOf(item1);

				if (index == -1 && targetList.Count > 0)
				{
					item1.Pastlog = true;
					OnUpdated(new PatrolEventArgs(item1));
				}
				else if (index >= 0 && item1.GotResCount < targetList[index].ResCount)
				{
					item1.ResCount = targetList[index].ResCount;
					OnUpdated(new PatrolEventArgs(item1));
				}
			}
		}
	}
}
