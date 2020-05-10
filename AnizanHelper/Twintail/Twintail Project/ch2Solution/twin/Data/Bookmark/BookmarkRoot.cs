// BookmarkRoot.cs

namespace Twin
{
	using System;
	using System.Collections;
	using System.Windows.Forms;
	using System.Xml;
	using System.Text;
	using System.IO;
	using Twin.Text;

	/// <summary>
	/// お気に入りの最上層
	/// </summary>
	public class BookmarkRoot : BookmarkFolder
	{
		/// <summary>
		/// BookmarkRootクラスのインスタンスを初期化
		/// </summary>
		public BookmarkRoot() : this("お気に入り")
		{
			// 
			// TODO: コンストラクタ ロジックをここに追加してください。
			//
		}

		/// <summary>
		/// BookmarkRootクラスのインスタンスを初期化
		/// </summary>
		/// <param name="name"></param>
		public BookmarkRoot(string name) : base(name, -1)
		{
			Expanded = true;
		}

		/// <summary>
		/// このメソッドはサポートしていません
		/// </summary>
		/// <returns></returns>
		public override BookmarkEntry Clone()
		{
			throw new NotSupportedException("ルートフォルダを複製することは出来ません");
		}

		/// <summary>
		/// お気に入り情報をxml形式でファイルに保存
		/// </summary>
		/// <param name="filePath"></param>
		public virtual void Save(string filePath)
		{
			if (filePath == null) {
				throw new ArgumentNullException("filePath");
			}

			XmlDocument doc = new XmlDocument();
			XmlElement root = doc.CreateElement("Bookmarks");

			// 再起ですべての項目を追加
			AppendRecursive(doc, root, this);
			doc.AppendChild(root);

			// 保存
			XmlTextWriter writer = new XmlTextWriter(filePath, Encoding.UTF8);
			writer.Formatting = Formatting.Indented;

			doc.Save(writer);
			writer.Close();
		}

		/// <summary>
		/// 再起検索を利用してサブフォルダとお気に入りの要素をelementに追加
		/// </summary>
		/// <param name="doc"></param>
		/// <param name="parent"></param>
		/// <param name="folders"></param>
		private void AppendRecursive(XmlDocument doc, XmlElement parent, BookmarkFolder folder)
		{
			// フォルダ名の属性を作成
			XmlAttribute attr1 = doc.CreateAttribute("Name");
			attr1.Value = folder.Name;

			// 識別ID属性を作成
			XmlAttribute attr2 = doc.CreateAttribute("ID");
			attr2.Value = folder.Id.ToString();

			// 展開されているかどうかの属性を作成
			XmlAttribute attr3 = doc.CreateAttribute("Expanded");
			attr3.Value = folder.Expanded.ToString();

			// サブフォルダコレクションノードを作成
			XmlElement children = doc.CreateElement("Children");

			foreach (BookmarkEntry entry in folder.Children)
			{
				// お気に入りの場合
				if (entry.IsLeaf)
				{
					BookmarkThread item = (BookmarkThread)entry;

					// URL属性を作成
					XmlAttribute url = doc.CreateAttribute("URL");
					url.Value = item.HeaderInfo.Url;

					//XmlAttribute id = doc.CreateAttribute("ID");
					//id.Value = entry.Id.ToString();

					// お気に入りノードを作成
					XmlElement node = doc.CreateElement("Item");
					node.Attributes.Append(url);
					//node.Attributes.Append(id);

					// beta18
					//node.InnerText = item.HeaderInfo.Subject;
					node.InnerText = item.Name;

					// お気に入りコレクションノードに追加
					children.AppendChild(node);
				}
				// フォルダであれば潜る
				else {
					AppendRecursive(doc, children, (BookmarkFolder)entry);
				}
			}

			// 最後にフォルダノードの作成
			XmlElement folderNode = doc.CreateElement("Folder");
			folderNode.Attributes.Append(attr1);
			folderNode.Attributes.Append(attr2);
			folderNode.Attributes.Append(attr3);
			folderNode.AppendChild(children);

			// 親ノードに追加
			parent.AppendChild(folderNode);
		}

		/// <summary>
		/// お気に入りファイルを読み込む
		/// </summary>
		/// <param name="filePath"></param>
		public virtual void Load(string filePath)
		{
			if (filePath == null) {
				throw new ArgumentNullException("filePath");
			}

			// 子をすべて削除してから読み込む
			Children.Clear();

			if (File.Exists(filePath))
			{
				XmlDocument doc = new XmlDocument();
				doc.Load(filePath);

				// 再起を利用してノードを検索
				XmlNode root = doc.DocumentElement.FirstChild;
				LoadRecursive(root, this);
			}
		}

		private void LoadRecursive(XmlNode node, BookmarkFolder folder)
		{
			// フォルダを作成
			folder.Name = node.Attributes["Name"].Value;
			folder.Expanded = Boolean.Parse(node.Attributes["Expanded"].Value);

			XmlAttribute id = node.Attributes["ID"];
			if (id != null)
				BookmarkEntry.SetEntryId(folder, Int32.Parse(id.Value));

			// 子ノードを検索
			foreach (XmlNode subNode in node.SelectNodes("Children/Folder"))
			{
				BookmarkFolder subFolder = new BookmarkFolder();
				folder.Children.Add(subFolder);
				LoadRecursive(subNode, subFolder);
			}

			// お気に入りコレクションを検索
			foreach (XmlNode child in node.SelectNodes("Children/Item"))
			{
				string url = child.Attributes["URL"].Value;
				ThreadHeader header = URLParser.ParseThread(url);
				
				if (header != null)
				{
					//XmlAttribute idattr = child.Attributes["ID"];
					BookmarkEntry entry = null;

					header.Subject = child.InnerText;

					//if (idattr != null)
					//	entry = new BookmarkThread(header, Int32.Parse(idattr.Value));
					//else 
						entry = new BookmarkThread(header);

					folder.Children.Add(entry);
				}
			}
		}

		/// <summary>
		/// 再起検索で指定したお気に入りを検索
		/// </summary>
		/// <param name="header"></param>
		/// <returns></returns>
		public BookmarkThread Search(ThreadHeader header)
		{
			return Search(this, header);
		}

		/// <summary>
		/// headerを検索
		/// </summary>
		/// <param name="folder"></param>
		/// <param name="header"></param>
		/// <returns></returns>
		private BookmarkThread Search(BookmarkFolder folder, ThreadHeader header)
		{
			foreach (BookmarkEntry entry in folder.Children)
			{
				if (entry.IsLeaf)
				{
					BookmarkThread item = (BookmarkThread)entry;
					bool equal = item.HeaderInfo.Equals(header);

					if (equal)
						return item;
				}
				else {
					BookmarkThread r = Search((BookmarkFolder)entry, header);
					if (r != null) return r;
				}
			}
			return null;
		}

		/// <summary>
		/// お気に入りにitemが含まれているかどうかを判断
		/// </summary>
		/// <param name="header"></param>
		/// <returns></returns>
		public bool Contains(ThreadHeader header)
		{
			return Search(header) != null;
		}

		/// <summary>
		/// お気に入りからitemを削除
		/// </summary>
		/// <param name="header"></param>
		public void Remove(ThreadHeader header)
		{
			BookmarkEntry item = Search(header);
			if (item != null) item.Remove();
		}

		/// <summary>
		/// このインスタンスを文字列に変換
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			return Name;
		}
	}
}
