// BoardLinker.cs

namespace Twin
{
	using System;
	using System.Collections.Generic;
	using System.Text;
	using System.IO;
	using System.Xml;
	using Twin.IO;

	/// <summary>
	/// 板と板をリンクし関連づけるクラス
	/// </summary>
	public class BoardLinker
	{
		private Cache cache;

		/// <summary>
		/// BoardLinkerクラスのインスタンスを初期化
		/// </summary>
		/// <param name="cache"></param>
		public BoardLinker(Cache cache)
		{
			if (cache == null)
				throw new ArgumentNullException("cache");

			this.cache = cache;
		}

		/// <summary>
		/// ログの移転と、古い板に移転履歴を残す
		/// </summary>
		/// <param name="oldBoard移転元の板</param>
		/// <param name="newBoard">移転先の板</param>
		public void Replace(BoardInfo oldBoard, BoardInfo newBoard)
		{
			if (oldBoard == null) {
				throw new ArgumentNullException("oldBoard");
			}
			if (newBoard == null) {
				throw new ArgumentNullException("newBoard");
			}

			// dat落ちスレと生きているスレを分離
			//List<ThreadHeader> leaveItems, liveItems;
			//Separate(oldBoard, newBoard, out leaveItems, out liveItems);

			//// 既得インデックスを更新
			//GotThreadListIndexer.Write(cache, oldBoard, leaveItems);
			//GotThreadListIndexer.Write(cache, newBoard, liveItems);

			ThreadIndexer.Indexing(cache, newBoard);
/*
			// 移転前の既得ログを取得
			List<ThreadHeader> gotItems = GotThreadListIndexer.Read(cache, oldBoard);
			
			// 新しい板情報に変更
			foreach (ThreadHeader h in gotItems)
				h.BoardInfo = newBoard;

			// 新しい板に既得インデックスを作成
			GotThreadListIndexer.Write(cache, newBoard, gotItems);

			// ログも移動
			CopyDatFiles(oldBoard, newBoard);
*/
		}


		/// <summary>
		/// 生きているスレとdat落ちしているスレを分離
		/// </summary>
		/// <param name="oldBoard">ログが存在する板</param>
		/// <param name="newBoard">生きているスレの移転先</param>
		/// <param name="leaveItems">dat落ちしているスレが格納される</param>
		/// <param name="liveItems">生きているスレが格納される</param>
		private void Separate(BoardInfo oldBoard, BoardInfo newBoard,
			out List<ThreadHeader> leaveItems, out List<ThreadHeader> liveItems)
		{
			leaveItems = new List<ThreadHeader>();
			liveItems = new List<ThreadHeader>();

			if (cache.Exists(oldBoard))
			{
				List<ThreadHeader> oldItems = GotThreadListIndexer.Read(cache, oldBoard);
				List<ThreadHeader> newItems = new List<ThreadHeader>();
				ThreadListReader listReader = null;

				if (oldItems.Count > 0)
				{
					try {
						listReader = TypeCreator.CreateThreadListReader(newBoard.Bbs);
						listReader.Open(newBoard);

						while (listReader.Read(newItems) != 0);

						// 移転先のスレ一覧に存在するログのみ移転 (dat落ちしているスレは移転しない)
						foreach (ThreadHeader header in oldItems)
						{
							Predicate<ThreadHeader> p = new Predicate<ThreadHeader>(delegate (ThreadHeader h)
							{
								return h.Key == header.Key;
							});

							if (newItems.Exists(p))
							{
								// 生きているスレの板情報を移転先板に書き換える
								if  (ThreadIndexer.Read(cache, header) != null)
								{
									ThreadIndexer.Delete(cache, header);

									liveItems.Add(header);
									header.BoardInfo = newBoard;

									ThreadIndexer.Write(cache, header);
								}
							}
							else {
								leaveItems.Add(header);
							}
						}
					}
					finally {
						if (listReader != null)
							listReader.Close();
					}
				}
			}
		}

		/// <summary>
		/// oldBoardからnewBoardにログを移動
		/// </summary>
		/// <param name="oldItems"></param>
		/// <param name="newBoard"></param>
		private void CopyDatFiles(BoardInfo oldItems, BoardInfo newBoard)
		{
			string fromFolder = cache.GetFolderPath(oldItems);
			string toFolder = cache.GetFolderPath(newBoard, true);

			string[] fileNames = Directory.GetFiles(fromFolder, "*.dat*");// .dat .dat.gz を検索

			foreach (string fromPath in fileNames)
			{
				try {
					string fromName = Path.GetFileName(fromPath);
					string destPath = Path.Combine(toFolder, fromName);

					File.Copy(fromPath, destPath, true);
					File.Delete(fromPath);

					int token = fromName.IndexOf('.');
					if (token != -1)
					{
						string key = fromName.Substring(0, token);
						string fromIndexFile = Path.Combine(fromFolder, key + ".idx");
						string toIndexFile = Path.Combine(toFolder, key + ".idx");

						ThreadHeader h = ThreadIndexer.Read(fromIndexFile);

						if (h != null)
						{
							h.BoardInfo.Server = newBoard.Server;

							ThreadIndexer.Write(toIndexFile, h);

							File.Delete(fromIndexFile);
						}
					}
				}
				catch (IOException ex) 
				{
					TwinDll.Output(ex);
				}
			}
		}

//		/// <summary>
//		/// boardにリンク情報を作成
//		/// </summary>
//		/// <param name="cache"></param>
//		/// <param name="board"></param>
//		private void CreateLinkInfo(Cache cache, BoardInfo oldBoard, BoardInfo newBoard)
//		{
//			try {
//				// 移転元にリンク情報を残す
//				string filePath = 
//					Path.Combine(cache.GetFolderPath(oldBoard), "moved.txt");
//
//				XmlDocument doc = new XmlDocument();
//				XmlNode root = doc.CreateElement("Link");
//					
//				XmlAttribute serv = doc.CreateAttribute("Server");
//				serv.Value = newBoard.Server;
//
//				XmlAttribute path = doc.CreateAttribute("Path");
//				path.Value = newBoard.Path;
//
//				XmlNode node = doc.CreateElement("Item");
//				node.Attributes.Append(serv);
//				node.Attributes.Append(path);
//				node.InnerText = newBoard.Name;
//				root.AppendChild(node);
//
//				doc.AppendChild(root);
//				doc.Save(filePath);
//			}
//			catch (Exception ex) {
//				throw new ApplicationException("リンク情報の作成に失敗しました", ex);
//			}
//		}

//		/// <summary>
//		/// 指定した板がリンクされている板を取得
//		/// </summary>
//		/// <param name="board"></param>
//		/// <param name="recursive"></param>
//		/// <returns></returns>
//		public BoardInfo GetLinked(BoardInfo board, bool recursive)
//		{
//			if (board == null) {
//				throw new ArgumentNullException("board");
//			}
//
//			string filePath = 
//				Path.Combine(cache.GetFolderPath(board), "moved.txt");
//
//			if (File.Exists(filePath))
//			{
//				XmlDocument doc = new XmlDocument();
//				doc.Load(filePath);
//
//				XmlNode node = doc.SelectSingleNode("link/item");
//
//				BoardInfo result = new BoardInfo(
//					node.Attributes["Server"].Value, 
//					node.Attributes["Path"].Value,
//					node.InnerText);
//
//				if (recursive)
//				{
//					BoardInfo sub = GetLinked(result, true);
//					if (sub != null) result = sub;
//				}
//
//				return result;
//			}
//			return null;
//		}
	}
}
