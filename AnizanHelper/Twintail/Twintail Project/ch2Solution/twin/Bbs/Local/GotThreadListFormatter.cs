// GotThreadListFormatter.cs

namespace Twin.Text
{
	using System;
	using System.Text;
	using System.IO;
	using System.Collections.Generic;
	using System.Xml;

	/// <summary>
	/// 既得スレッドの情報をインデックス化する
	/// </summary>
	public class GotThreadListFormatter : ThreadListFormatter
	{
		/// <summary>
		/// GotThreadListFormatterクラスのインスタンスを初期化
		/// </summary>
		public GotThreadListFormatter()
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

			/*	headerの簡易情報をxml形式に変換
			 *
			 * <indices>
			 *		<item key="123456">
			 *			<subject>スレッド名</subject>
			 *			<resCount>999</resCount>
			 *		</item>
			 * </indices>
			 */

			XmlDocument document = new XmlDocument();
			XmlElement root = document.CreateElement("indices");
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
