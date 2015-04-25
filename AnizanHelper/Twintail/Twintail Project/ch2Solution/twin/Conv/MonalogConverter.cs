// MonalogConverter.cs

namespace Twin.Conv
{
	using System;
	using System.IO;
	using System.Text;
	using System.Xml;
	using System.Diagnostics;

	/// <summary>
	/// Monalog/1.0 の中間形式の相互コンバーター
	/// 参考URL: http://logconvert.s28.xrea.com/
	/// </summary>
	public class MonalogConverter : IConvertible
	{
		/// <summary>
		/// MonalogConverterクラスのインスタンスを初期化
		/// </summary>
		public MonalogConverter()
		{
			// 
			// TODO: コンストラクタ ロジックをここに追加してください。
			//
		}

		public void Read(string filePath, out ThreadHeader header, 
			out ResSetCollection resCollection)
		{
			XmlDocument doc = new XmlDocument();
			doc.Load(filePath);

			header = TypeCreator.CreateThreadHeader(BbsType.X2ch);
			resCollection = new ResSetCollection();

			// ルート要素を取得
			XmlNode root = doc.DocumentElement;

			// <thread>要素の処理
			XmlNode thread = root.SelectSingleNode("thread");
			if (thread == null)
				throw new ConvertException("<thread>要素が存在しません");

			XmlNode serv = thread.Attributes.GetNamedItem("server");
			if (serv == null)
				throw new ConvertException("server属性が存在しません");

			XmlNode board = thread.Attributes.GetNamedItem("board");
			if (board == null)
				throw new ConvertException("board属性が存在しません");

			XmlNode key = thread.Attributes.GetNamedItem("key");
			if (key == null)
				throw new ConvertException("key属性が存在しません");

			// スレッドタイトルを取得
			XmlNode title = thread.SelectSingleNode("title");
			if (title == null)
				throw new ConvertException("<title>要素が存在しません");

			// <info>要素の処理
			XmlNode info = thread.SelectSingleNode("info");
			if (info == null)
				throw new ConvertException("<info>要素が存在しません");

			XmlNode lastmod = info.Attributes.GetNamedItem("last-modified");
			if (lastmod == null)
				throw new ConvertException("last-modified属性が存在しません");

			XmlNode size = info.Attributes.GetNamedItem("size");
			if (size == null)
				throw new ConvertException("size属性が存在しません");

			// スレッドの情報を設定
			header.Key = key.Value;
			header.BoardInfo.Server = serv.Value;
			header.BoardInfo.Path = board.Value;
			header.Subject = title.InnerText;
			header.LastModified = DateTime.Parse(lastmod.Value);
			
			int gotByteCount;
			if (Int32.TryParse(size.Value, out gotByteCount))
				header.GotByteCount = gotByteCount;

			// <res-set>要素の処理
			XmlNode children = thread.SelectSingleNode("res-set");
			if (children == null)
				throw new ConvertException("<res-set>要素が存在しません");

			resCollection = GetResCollection(doc, children);
			header.GotResCount = resCollection.Count;
		}

		private ResSetCollection GetResCollection(XmlDocument doc, XmlNode resset)
		{
			ResSetCollection collection = new ResSetCollection();

			foreach (XmlNode child in resset.SelectNodes("res"))
			{
				XmlNode num = child.Attributes.GetNamedItem("num");
				if (num == null)
					throw new ConvertException("num属性が存在しません");

				// state属性は無視
				// ...

				XmlNode name = child.SelectSingleNode("name");
				if (name == null)
					throw new ConvertException("<name>要素が存在しません");

				XmlNode email = child.SelectSingleNode("email");
				if (email == null)
					throw new ConvertException("<email>要素が存在しません");

				XmlNode timestamp = child.SelectSingleNode("timestamp");
				if (timestamp == null)
					throw new ConvertException("<timestamp>要素が存在しません");

				XmlNode message = child.SelectSingleNode("message");
				if (message == null)
					throw new ConvertException("<message>要素が存在しません");

				int index;
				Int32.TryParse(num.Value, out index);

				ResSet res = new ResSet(
					index,
					name.InnerText,
					email.InnerText,
					timestamp.InnerText,
					message.InnerText);

				collection.Add(res);
			}

			return collection;
		}
		
		public void Write(string filePath, ThreadHeader header,
			ResSetCollection resCollection)
		{
			XmlDocument doc = new XmlDocument();

			// バージョン属性を作成
			XmlAttribute version = doc.CreateAttribute("version");
			version.Value = "1.0";

			// ルートを作成
			XmlNode root = doc.CreateElement("monalog", "http://www.monazilla.org/Monalog/1.0");
			root.Attributes.Append(version);

			// スレッド要素を作成
			XmlNode thread = CreateThreadElement(doc, header, resCollection);
			root.AppendChild(thread);
			doc.AppendChild(root);

			// エンコーダをShift_Jisで保存
			XmlTextWriter writer = null;
			try {
				writer = new XmlTextWriter(filePath, Encoding.GetEncoding("Shift_Jis"));
				writer.Formatting = Formatting.Indented;
				doc.Save(writer);
			}
			finally {
				if (writer != null)
					writer.Close();
			}
		}

		private XmlNode CreateThreadElement(XmlDocument doc,
			ThreadHeader header, ResSetCollection resCollection)
		{
			// サーバー属性を作成
			XmlAttribute serv = doc.CreateAttribute("server");
			serv.Value = header.BoardInfo.Server;

			// 板属性を作成
			XmlAttribute board = doc.CreateAttribute("board");
			board.Value = header.BoardInfo.Path;

			// key属性を作成
			XmlAttribute key = doc.CreateAttribute("key");
			key.Value = header.Key;

			// <thread></thread>要素を作成
			XmlNode thread = doc.CreateElement("thread");
			thread.Attributes.Append(serv);
			thread.Attributes.Append(board);
			thread.Attributes.Append(key);

			// タイトル要素を作成
			XmlNode title = doc.CreateElement("title");
			title.AppendChild(doc.CreateCDataSection(header.Subject));
			thread.AppendChild(title);

			// info要素を作成
			XmlAttribute lastmod = doc.CreateAttribute("last-modified");
			lastmod.Value = header.LastModified.ToString("R"); // RFC1123 パターン

			XmlAttribute size = doc.CreateAttribute("size");
			size.Value = header.GotByteCount.ToString();

			XmlNode info = doc.CreateElement("info");
			info.Attributes.Append(lastmod);
			info.Attributes.Append(size);
			thread.AppendChild(info);

			// <res-set></res-set>要素を作成
			XmlNode children = CreateResSetChildren(doc, resCollection);
			thread.AppendChild(children);

			return thread;
		}

		private XmlNode CreateResSetChildren(XmlDocument doc, ResSetCollection resCollection)
		{
			// レスコレクション要素を作成
			XmlNode children = doc.CreateElement("res-set");

			foreach (ResSet res in resCollection)
			{
				// レス番号を表す、num属性を作成
				XmlAttribute num = doc.CreateAttribute("num");
				num.Value = res.Index.ToString();

				// レスの状態を表す、state属性を作成
				XmlAttribute state = doc.CreateAttribute("state");
				state.Value = "normal";

				// レス要素を作成
				XmlNode child = doc.CreateElement("res");
				child.Attributes.Append(num);
				child.Attributes.Append(state);

				// 名前
				XmlNode name = doc.CreateElement("name");
				name.AppendChild(doc.CreateCDataSection(res.Name));
				
				// Email
				XmlNode email = doc.CreateElement("email");
				email.AppendChild(doc.CreateCDataSection(res.Email));

				// 日付
				XmlNode timestamp = doc.CreateElement("timestamp");
				timestamp.AppendChild(doc.CreateCDataSection(res.DateString));

				// メッセージ
				XmlNode message = doc.CreateElement("message");
				message.AppendChild(doc.CreateCDataSection(res.Body));

				child.AppendChild(name);
				child.AppendChild(email);
				child.AppendChild(timestamp);
				child.AppendChild(message);
				children.AppendChild(child);
			}

			return children;
		}
	}
}
