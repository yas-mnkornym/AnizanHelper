// KatjuBoardTable.cs
// #2.0

namespace Twin
{
	using System;
	using System.IO;
	using System.Text;
	using System.Text.RegularExpressions;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Net;
	using Twin.Text;
	using Twin.Tools;
	using System.Threading;

	/// <summary>
	/// かちゅ～しゃ互換 (2channel.brd形式) ボードテーブル
	/// </summary>
	public class KatjuBoardTable : IBoardTable
	{
		private List<Category> items = new List<Category>();

		public List<Category> Items
		{
			get
			{
				return items;
			}
		}

		public KatjuBoardTable()
		{
		}

		public void Add(IBoardTable table)
		{
			if (table == null)
			{
				throw new ArgumentNullException("table");
			}

			foreach (Category cate in table.Items)
				items.Add(cate);
		}

		public void Clear()
		{
			items.Clear();
		}

		/// <summary>
		/// オンラインで板一覧を更新 ([BBS MENU for 2ch]に対応)
		/// </summary>
		/// <param name="url">更新先URL</param>
		/// <param name="callback">板が移転していた場合に呼ばれるコールバック</param>
		public void OnlineUpdate(string url, BoardUpdateEventHandler callback)
		{
			HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
			req.Headers.Add("Pragma", "no-cache");
			req.Headers.Add("Cache-Control", "no-cache");

			HttpWebResponse res = (HttpWebResponse)req.GetResponse();

			try
			{
				IBoardTable newTable = new KatjuBoardTable();
				string htmlData;

				using (StreamReader sr = new StreamReader(res.GetResponseStream(), Encoding.GetEncoding("Shift_Jis")))
					htmlData = sr.ReadToEnd();

				res.Close();
				res = null;

				// 2012/12/05 Mizutama実装
				// 板情報を抽出
				// ＜BR＞＜BR＞＜B＞カテゴリ名＜/B＞＜BR＞
				// ＜A HREF=http://[サーバー]/[板名]/＞名前＜/A＞
				MatchCollection cats = Regex.Matches
									   (
										 htmlData,
										 @"<BR><BR><B>(?<cat>.+?)</B><BR>(?<brds>.+?)(?=\<BR\>\<BR\>\<B\>)",
										 RegexOptions.Singleline | RegexOptions.IgnoreCase
									   );
				foreach (Match m in cats)
				{
					Category category = new Category(m.Groups["cat"].Value);

					MatchCollection brds = Regex.Matches
										   (
											 m.Groups["brds"].Value,
											 @"<A HREF=(?<url>[^\s>]+).*?>(?<subj>.+?)</A>",
											 RegexOptions.Singleline | RegexOptions.IgnoreCase
											);
					foreach (Match matchBrd in brds)
					{
						// ボード情報を作成
						BoardInfo newBoard = URLParser.ParseBoard(matchBrd.Groups["url"].Value);
						if (newBoard != null)
						{
							newBoard.Name = matchBrd.Groups["subj"].Value;
							category.Children.Add(newBoard);

							if (callback != null)
							{
								// 新板＆移転チェック
								BoardInfo old = FromName(newBoard.Name, newBoard.DomainPath);
								BoardUpdateEventArgs args = null;

								// 見つからなければ新板と判断
								if (old == null)
								{
									args = new BoardUpdateEventArgs(BoardUpdateEvent.New, null, newBoard);
								}
								// 見つかったが板のURLが違う場合は移転と判断
								else if (old.Server != newBoard.Server)
								{
									args = new BoardUpdateEventArgs(BoardUpdateEvent.Change, old, newBoard);
								}

								if (args != null)
									callback(this, args);
							}
						}
					}

					if (category.Children.Count > 0)
					{
						newTable.Items.Add(category);
					}
				}

				if (newTable.Items.Count > 0)
				{
					// 新しい板一覧を設定
					Items.Clear();
					Items.AddRange(newTable.Items);
				}
				else
				{
					throw new ApplicationException("板一覧の更新に失敗しました");
				}
			}
			catch (ThreadAbortException)
			{
				if (callback != null)
					callback(this, new BoardUpdateEventArgs(BoardUpdateEvent.Cancelled, null, null));
			}
			catch (Exception ex)
			{
				TwinDll.ShowOutput(ex);
			}
			finally
			{
				if (res != null)
					res.Close();
			}
		}

		public void SaveTable(string fileName)
		{
			if (fileName == null)
			{
				throw new ArgumentNullException("fileName", "fileNameがnull参照です");
			}

			StreamWriter sw = null;

			try
			{
				sw = new StreamWriter(fileName, false, TwinDll.DefaultEncoding);
				sw.WriteLine("2");

				foreach (Category cate in items)
				{
					sw.WriteLine("{0}\t{1}",
						cate.Name, cate.IsExpanded ? 1 : 0);

					foreach (BoardInfo board in cate.Children)
					{
						sw.WriteLine("\t{0}\t{1}\t{2}",
							board.Server, board.Path, board.Name);
					}
				}
			}
			finally
			{
				if (sw != null)
					sw.Close();
			}
		}

		/// <summary>
		/// 2ちゃんねるボードテーブル(2channel.brd型式)を読み込む
		/// </summary>
		/// <param name="fileName">読み込むファイル名</param>
		/// <exception cref="System.ArgumentNullException">fileNameがnull参照です</exception>
		/// <exception cref="System.IO.FileNotFoundException">fileNameは存在しません</exception>
		public void LoadTable(string fileName)
		{
			if (fileName == null)
			{
				throw new ArgumentNullException("fileName", "fileNameがnull参照です");
			}

			StreamReader sr = null;
			Category category = null;
			string text;

			try
			{
				sr = new StreamReader(fileName, TwinDll.DefaultEncoding);

				while ((text = sr.ReadLine()) != null)
				{
					string[] elems = text.Split('\t');

					if (elems.Length < 2)
					{

					}
					// カテゴリ開始
					else if (elems[0] != String.Empty)
					{
						int expandedBool;

						category = new Category(elems[0]);
						if (Int32.TryParse(elems[1], out expandedBool))
							category.IsExpanded = Convert.ToBoolean(expandedBool);
						items.Add(category);
					}
					// ボード追加
					else if (elems.Length >= 4)
					{
						//if (BoardInfo.IsSupport(elems[1]))
						{
							string serv = elems[1];
							string dir = elems[2];
							string name = elems[3];
							BoardInfo item = new BoardInfo(serv, dir, name);
							category.Children.Add(item);
						}
					}
				}
			}
			finally
			{
				if (sr != null)
					sr.Close();
			}
		}

		public void Replace(BoardInfo oldBoard, BoardInfo newBoard)
		{
			if (oldBoard == null)
			{
				throw new ArgumentNullException("oldBoard");
			}
			if (newBoard == null)
			{
				throw new ArgumentNullException("newBoard");
			}

			foreach (Category cate in items)
			{
				int index = cate.Children.IndexOf(oldBoard);

				if (index != -1)
				{
					BoardInfo a = (BoardInfo)cate.Children[index];
					a.Path = newBoard.Path;
					a.Server = newBoard.Server;
				}
			}
		}

		public bool Contains(BoardInfo board)
		{
			if (board == null)
			{
				throw new ArgumentNullException("board");
			}

			return Items.Exists(new Predicate<Category>(delegate(Category category)
			{
				return category.Children.Contains(board);
			}));
		}

		/// <summary>
		/// 板一覧の中から指定した URL を持つ板情報を検索します。
		/// </summary>
		/// <param name="url"></param>
		/// <returns>一致した板情報を返します。見つからなければ null です。</returns>
		public BoardInfo FromUrl(string url)
		{
			foreach (Category category in Items)
			{
				int index = category.Children.IndexOfUrl(url);

				if (index >= 0)
				{
					return category.Children[index];
				}
			}
			return null;
		}

		/// <summary>
		/// 指定した板名を持つ板を検索
		/// </summary>
		/// <param name="name"></param>
		/// <param name="domainPath"></param>
		/// <returns>見つからなければnullを返す</returns>
		public BoardInfo FromName(string name, string domainPath)
		{
			foreach (Category cate in Items)
			{
				int index = cate.Children.IndexOfName(name);
				if (index >= 0)
				{
					BoardInfo brd = cate.Children[index];
					if (brd.DomainPath.Equals(domainPath))
						return brd;
				}
			}
			return null;
		}

		public BoardInfo[] ToArray()
		{
			List<BoardInfo> list = new List<BoardInfo>();

			foreach (Category cate in items)
			{
				foreach (BoardInfo board in cate.Children)
				{
					list.Add(board);
				}
			}

			return list.ToArray();
		}
	}
}
