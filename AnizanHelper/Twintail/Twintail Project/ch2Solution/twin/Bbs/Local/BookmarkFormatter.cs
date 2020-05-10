// BookmarkFormatter.cs
// #2.0

namespace Twin.Text
{
	using System;
	using System.Text;
	using System.Collections.Generic;
	using System.IO;
	using System.Xml;

	/// <summary>
	/// お気に入り情報をインデックス化する
	/// </summary>
	public class BookmarkFormatter : ThreadListFormatter
	{
		/// <summary>
		/// BookmarkFormatterクラスのインスタンスを初期化
		/// </summary>
		public BookmarkFormatter()
		{
		}

		/// <summary>
		/// 子ノードを作成
		/// </summary>
		/// <param name="header"></param>
		/// <returns></returns>
		public void AppendChild(XmlDocument doc, XmlElement root, ThreadHeader header)
		{
			XmlAttribute attr = doc.CreateAttribute("key");
			attr.Value = header.Key;

			XmlElement child = doc.CreateElement("item");
			child.Attributes.Append(attr);

			XmlElement subj = doc.CreateElement("subject");
			subj.AppendChild(doc.CreateCDataSection(header.Subject));

			XmlElement resc = doc.CreateElement("resCount");
			resc.InnerText = header.ResCount.ToString();

			child.AppendChild(subj);
			child.AppendChild(resc);

			root.AppendChild(child);
		}

		/// <summary>
		/// 指定したヘッダーを書式化して文字列に変換
		/// </summary>
		public override string Format(ThreadHeader header)
		{
			if (header == null)
			{
				throw new ArgumentNullException("header");
			}

			List<ThreadHeader> temp = new List<ThreadHeader>();
			temp.Add(header);

			return Format(temp);
		}

		/// <summary>
		/// 指定したヘッダーコレクションを書式化して文字列に変換
		/// </summary>
		public override string Format(List<ThreadHeader> headerList)
		{
			if (headerList == null)
			{
				throw new ArgumentNullException("headerList");
			}

			XmlDocument document = new XmlDocument();
			XmlElement root = document.CreateElement("Bookmarks");
			document.AppendChild(root);

			foreach (ThreadHeader header in headerList)
			{
				AppendChild(document, root, header);
			}

			MemoryStream memory = new MemoryStream();
			XmlTextWriter writer = new XmlTextWriter(memory, TwinDll.DefaultEncoding); // UTF8にするとなぜか先頭にゴミが付く…

			writer.Formatting = Formatting.Indented;
			document.Save(writer);
			writer.Close();

			// 文字列に変換
			return TwinDll.DefaultEncoding.GetString(memory.ToArray());
		}
	}
}

