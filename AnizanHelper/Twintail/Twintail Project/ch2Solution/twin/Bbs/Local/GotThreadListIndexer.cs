// ThreadListIndexer.cs
// #2.0

namespace Twin
{
	using System;
	using System.IO;
	using System.Text;
	using System.Xml;
	using System.Collections.Generic;
	using Twin.Util;
	using Twin.Text;

	/// <summary>
	/// 既得スレッド一覧のインデックスを作成・管理する
	/// </summary>
	public class GotThreadListIndexer
	{
		/// <summary>
		/// 指定した板のインデックス保存先パスを取得または設定
		/// </summary>
		/// <param name="cache"></param>
		/// <param name="board"></param>
		/// <returns></returns>
		public static string GetIndicesPath(Cache cache, BoardInfo board)
		{
			if (cache == null)
			{
				throw new ArgumentNullException("cache");
			}
			if (board == null)
			{
				throw new ArgumentNullException("board");
			}

			string folderPath = cache.GetFolderPath(board, false);
			string listPath = Path.Combine(folderPath, "indices.txt");

			return listPath;
		}

		/// <summary>
		/// 指定した板の既得インデックスを読み込む
		/// </summary>
		/// <param name="cache"></param>
		/// <param name="board"></param>
		/// <returns></returns>
		public static List<ThreadHeader> Read(Cache cache, BoardInfo board)
		{
			if (cache == null)
			{
				throw new ArgumentNullException("cache");
			}
			if (board == null)
			{
				throw new ArgumentNullException("board");
			}

			List<ThreadHeader> items = new List<ThreadHeader>();
			string indicesPath = GetIndicesPath(cache, board);

			if (File.Exists(indicesPath))
			{
				XmlDocument document = new XmlDocument();

				lock (typeof(GotThreadListIndexer))
				{
					document.Load(indicesPath);
				}

				XmlElement root = document.DocumentElement;
				XmlNodeList children = root.ChildNodes;

				foreach (XmlNode node in children)
				{
					try
					{
						ThreadHeader header = TypeCreator.CreateThreadHeader(board.Bbs);
						header.BoardInfo = board;
						header.Key = node.Attributes.GetNamedItem("key").Value;
						header.Subject = node.SelectSingleNode("subject").InnerText;

						int resCount;
						if (Int32.TryParse(node.SelectSingleNode("resCount").InnerText, out resCount))
							header.ResCount = resCount;

						items.Add(header);
					}
					catch (Exception ex)
					{
						TwinDll.Output(ex);
					}
				}
			}

			return items;
		}

		/// <summary>
		/// 指定した板のインデックスを作成
		/// </summary>
		/// <param name="cache"></param>
		/// <param name="board"></param>
		/// <param name="items"></param>
		public static void Write(Cache cache, BoardInfo board, List<ThreadHeader> items)
		{
			if (cache == null)
			{
				throw new ArgumentNullException("cache");
			}
			if (board == null)
			{
				throw new ArgumentNullException("board");
			}
			if (items == null)
			{
				throw new ArgumentNullException("items");
			}

			string indicesPath =
				GetIndicesPath(cache, board);

			// ヘッダ情報を書式化してファイルに保存
			ThreadListFormatter formatter = new GotThreadListFormatter();

			lock (typeof(GotThreadListIndexer))
			{
				FileUtility.Write(indicesPath, formatter.Format(items), false, TwinDll.DefaultEncoding);
			}
		}

		/// <summary>
		/// 指定したスレッドのインデックスを削除
		/// </summary>
		/// <param name="cache"></param>
		/// <param name="header"></param>
		public static void Remove(Cache cache, ThreadHeader header)
		{
			if (cache == null)
			{
				throw new ArgumentNullException("cache");
			}
			if (header == null)
			{
				throw new ArgumentNullException("header");
			}

			XmlDocument document = new XmlDocument();
			string indicesPath = GetIndicesPath(cache, header.BoardInfo);

			lock (typeof(GotThreadListIndexer))
			{
				if (File.Exists(indicesPath))
				{
					document.Load(indicesPath);
					XmlNode node = document.SelectSingleNode("indices/item[@key=\"" + header.Key + "\"]");

					if (node != null)
						document.DocumentElement.RemoveChild(node);

					// １つも要素が無くなったらファイル自体を削除
					if (document.DocumentElement.ChildNodes.Count == 0)
					{
						File.Delete(indicesPath);
					}
					else
					{
						XmlTextWriter writer = new XmlTextWriter(indicesPath, TwinDll.DefaultEncoding);
						writer.Formatting = Formatting.Indented;

						document.Save(writer);
						writer.Close();
					}
				}

			}
		}

		/// <summary>
		/// 指定したスレッドのインデックスを作成
		/// </summary>
		/// <param name="cache"></param>
		/// <param name="header"></param>
		public static void Write(Cache cache, ThreadHeader header)
		{
			if (cache == null)
			{
				throw new ArgumentNullException("cache");
			}
			if (header == null)
			{
				throw new ArgumentNullException("header");
			}

			XmlDocument document = new XmlDocument();
			string indicesPath = GetIndicesPath(cache, header.BoardInfo);
			bool overwrite = false;

			lock (typeof(GotThreadListIndexer))
			{
				if (File.Exists(indicesPath))
				{
					document.Load(indicesPath);
					XmlNode node = document.SelectSingleNode("indices/item[@key=\"" + header.Key + "\"]");
					if (node != null)
					{
						// レス数を更新
						node.SelectSingleNode("resCount").InnerText = header.ResCount.ToString();
						overwrite = true;
					}
				}
				else
				{
					// ルートを作成
					document.AppendChild(
						document.CreateElement("indices"));
				}

				// 存在しなければ新しく作成
				if (!overwrite)
				{
					GotThreadListFormatter formatter = new GotThreadListFormatter();
					formatter.AppendChild(document, document.DocumentElement, header);
				}

				// ドキュメントを保存
				XmlTextWriter writer = new XmlTextWriter(indicesPath, TwinDll.DefaultEncoding);
				writer.Formatting = Formatting.Indented;

				document.Save(writer);
				writer.Close();
			}
		}

		internal static void Regeneration(Cache Cache)
		{
			throw new NotImplementedException();
		}
	}
}
