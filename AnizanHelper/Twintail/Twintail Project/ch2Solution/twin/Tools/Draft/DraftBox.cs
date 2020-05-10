// DraftBox.cs

namespace Twin.Tools
{
	using System;
	using System.Collections;
	using System.Xml;
	using System.Xml.XPath;
	using System.IO;

	/// <summary>
	/// 書きかけのメッセージを一時的に保存しておく草稿箱
	/// </summary>
	public class DraftBox
	{
		private const string DraftFileName = "draft.txt";
		private Cache cache;

		/// <summary>
		/// DraftBoxクラスのインスタンスを初期化
		/// </summary>
		public DraftBox(Cache cache)
		{
			// 
			// TODO: コンストラクタ ロジックをここに追加してください。
			//
			this.cache = cache;
		}

		/// <summary>
		/// 指定した草稿のxml要素を作成
		/// </summary>
		/// <param name="doc"></param>
		/// <param name="draft"></param>
		/// <returns></returns>
		private XmlNode CreateDraftElement(XmlDocument doc, Draft draft)
		{
			// スレッド番号を表す属性を作成
			XmlAttribute key = doc.CreateAttribute("key");
			key.Value = draft.HeaderInfo.Key;

			// 投稿者名
			XmlElement from = doc.CreateElement("from");
			from.AppendChild(doc.CreateCDataSection(draft.PostRes.From));

			// email
			XmlElement email = doc.CreateElement("email");
			email.AppendChild(doc.CreateCDataSection(draft.PostRes.Email));

			// 本文
			XmlElement body = doc.CreateElement("message");
			body.AppendChild(doc.CreateCDataSection(draft.PostRes.Body));

			// 草稿の要素を作成し、親に追加
			XmlElement child = doc.CreateElement("thread");
			child.Attributes.Append(key);
			child.AppendChild(from);
			child.AppendChild(email);
			child.AppendChild(body);

			return child;
		}

		/// <summary>
		/// 指定した板の草稿を削除
		/// </summary>
		/// <param name="filePath"></param>
		/// <param name="draft"></param>
		public void Remove(BoardInfo board, Draft draft)
		{
			if (board == null) {
				throw new ArgumentNullException("board");
			}
			if (draft == null) {
				throw new ArgumentNullException("draft");
			}

			XmlDocument doc = new XmlDocument();
			XmlElement root;

			string filePath = cache.GetFolderPath(board);
			filePath = Path.Combine(filePath, DraftFileName);

			if (File.Exists(filePath))
			{
				doc.Load(filePath);
				root = doc.DocumentElement;

				// 同じスレッド番号を持つ草稿要素を検索するためのXPath
				string xpath = String.Format("draft/thread[@key=\"{0}\"]", draft.HeaderInfo.Key);

				// 既に同じ草稿が存在した場合、一端削除してから新しい要素を追加
				XmlNode node = doc.SelectSingleNode(xpath);
				if (node != null)
					root.RemoveChild(node);

				doc.Save(filePath);
			}
		}

		/// <summary>
		/// 現在の草稿箱に上書き保存
		/// </summary>
		/// <param name="filePath"></param>
		/// <param name="draft"></param>
		public void Save(BoardInfo board, Draft draft)
		{
			if (board == null) {
				throw new ArgumentNullException("board");
			}
			if (draft == null) {
				throw new ArgumentNullException("draft");
			}

			XmlDocument doc = new XmlDocument();
			XmlElement root;

			string filePath = cache.GetFolderPath(board);
			filePath = Path.Combine(filePath, DraftFileName);

			// ファイルが存在しなければルート要素を作成しておく
			if (!File.Exists(filePath))
			{
				root = doc.CreateElement("draft");
				doc.AppendChild(root);
			}
			else {
				doc.Load(filePath);
				root = doc.DocumentElement;
			}

			// 同じスレッド番号を持つ草稿要素を検索するためのXPath
			string xpath = String.Format("draft/thread[@key=\"{0}\"]", draft.HeaderInfo.Key);

			// 新しい草稿の要素
			XmlNode newChild = CreateDraftElement(doc, draft);

			// 既に同じ草稿が存在した場合、一端削除してから新しい要素を追加
			XmlNode node = doc.SelectSingleNode(xpath);
			if (node != null)
				root.RemoveChild(node);

			root.AppendChild(newChild);
			doc.Save(filePath);
		}

		/// <summary>
		/// 指定したスレッドの草稿を取得
		/// </summary>
		/// <param name="header"></param>
		/// <returns></returns>
		public Draft Load(ThreadHeader header)
		{
			if (header == null) {
				throw new ArgumentNullException("header");
			}

			Draft draft = null;

			string filePath = cache.GetFolderPath(header.BoardInfo);
			filePath = Path.Combine(filePath, DraftFileName);

			if (File.Exists(filePath))
			{
				XmlDocument doc = new XmlDocument();
				doc.Load(filePath);
				
				// 同じスレッド番号を持つ草稿要素を検索するためのXPath
				string xpath = String.Format("draft/thread[@key=\"{0}\"]", header.Key);
				XmlNode node = doc.SelectSingleNode(xpath);

				if (node != null && ThreadIndexer.Read(cache, header) != null)
				{
					string name = node.SelectSingleNode("from").InnerText;
					string email = node.SelectSingleNode("email").InnerText;
					string body = node.SelectSingleNode("message").InnerText;

					// 草稿情報を作成
					PostRes res = new PostRes(name, email, body);
					draft = new Draft(header, res);
				}
			}

			return draft;
		}

		/// <summary>
		/// 指定した板に存在する草稿を取得
		/// </summary>
		/// <param name="filePath"></param>
		public Draft[] Load(BoardInfo board)
		{
			if (board == null) {
				throw new ArgumentNullException("board");
			}

			XmlDocument doc = new XmlDocument();
			ArrayList arrayList = new ArrayList();

			string filePath = cache.GetFolderPath(board);
			filePath = Path.Combine(filePath, DraftFileName);

			try {
				doc.Load(filePath);
				
				// 草稿要素をすべて取得
				XmlNodeList nodeList = doc.SelectNodes("draft/thread");
				
				foreach (XmlNode node in nodeList)
				{					
					ThreadHeader header = TypeCreator.CreateThreadHeader(board.Bbs);
					header.BoardInfo = board;
					header.Key = node.Attributes.GetNamedItem("key").Value;

					if (ThreadIndexer.Read(cache, header) != null)
					{
						string name = node.SelectSingleNode("from").InnerText;
						string email = node.SelectSingleNode("email").InnerText;
						string body = node.SelectSingleNode("message").InnerText;

						// 草稿情報を作成
						PostRes res = new PostRes(name, email, body);
						Draft draft = new Draft(header, res);
						arrayList.Add(draft);
					}
				}
			}
			catch (FileNotFoundException) {}

			return (Draft[])arrayList.ToArray(typeof(Draft));
		}
	}
}
