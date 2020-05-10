// LinkInfoCollection.cs

namespace Twin
{
	using System;
	using System.Collections;
	using System.Runtime.Serialization;
	using System.Runtime.Serialization.Formatters.Soap;
	using System.Text;
	using System.IO;
	using System.Xml;
	using CSharpSamples;
	using System.Text.RegularExpressions;

	/// <summary>
	/// LinkInfoクラスをコレクション管理
	/// </summary>
	[Serializable]
	public class LinkInfoCollection : CollectionBase, ISerializable
	{
		private string filePath;

		/// <summary>
		/// 指定したインデックスのアイテムを取得
		/// </summary>
		public LinkInfo this[int index] {
			get {
				return (LinkInfo)List[index];
			}
		}

		/// <summary>
		/// LinkInfoCollectionクラスのインスタンスを初期化
		/// </summary>
		public LinkInfoCollection() 
		{
			filePath = String.Empty;
		}

		/// <summary>
		/// LinkInfoCollectionクラスのインスタンスを初期化
		/// </summary>
		public LinkInfoCollection(string fileName) 
		{
			LoadFromXml(fileName);
		}

		public LinkInfoCollection(SerializationInfo info, StreamingContext context)
		{
			ArrayList arrayList = (ArrayList)info.GetValue("AddressList", typeof(ArrayList));
			foreach (LinkInfo link in arrayList)
			{
				if (link != null)
					Add(link);
			}
		}

		void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue("AddressList", InnerList);
		}

		/// <summary>
		/// コレクションに追加
		/// </summary>
		/// <param name="item">追加するLinkInfoクラス</param>
		/// <returns>追加された位置</returns>
		public int Add(LinkInfo item)
		{
			return List.Add(item);
		}

		/// <summary>
		/// コレクションを追加
		/// </summary>
		/// <param name="items">追加するLinkInfoCollectionクラス</param>
		/// <returns>追加された位置</returns>
		public void AddRange(LinkInfoCollection items)
		{
			InnerList.AddRange(items);
		}

		/// <summary>
		/// 指定したインデックスに挿入
		/// </summary>
		/// <param name="index">挿入するインデックス</param>
		/// <param name="item">挿入するLinkInfoクラス</param>
		public void Insert(int index, LinkInfo item)
		{
			List.Insert(index, item);
		}

		/// <summary>
		/// 指定したLinkInfoを削除
		/// </summary>
		/// <param name="item">削除するLinkInfo</param>
		public void Remove(LinkInfo item)
		{
			List.Remove(item);
		}

		/// <summary>
		/// インデックスを取得
		/// </summary>
		/// <param name="item">検索するアイテム</param>
		/// <returns>存在すればそのインデックス (見つからなければ-1)</returns>
		public int IndexOf(LinkInfo item)
		{
			return List.IndexOf(item);
		}

		/// <summary>
		/// 指定したURIにリンク情報を取得
		/// </summary>
		/// <param name="uri"></param>
		/// <returns></returns>
		public LinkInfo IndexOf(string uri)
		{
			uri = uri.ToLower();

			foreach (LinkInfo link in List)
			{
				if (uri.IndexOf(link.Uri.ToLower()) >= 0)
					return link;
			}
			return null;
		}

		/// <summary>
		/// リンクコレクションを読み込む (旧バージョンのファイル形式)
		/// </summary>
		/// <param name="fileName"></param>
		public void LoadFromSoap(string fileName)
		{
			if (fileName == null) {
				throw new ArgumentNullException("fileName");
			}

			Clear();
			filePath = fileName;

			if (File.Exists(fileName))
			{
				
				using (FileStream fileStream = new FileStream(fileName, FileMode.Open))
				{
					SoapFormatter soap = new SoapFormatter();
					LinkInfo[] linkInfos = soap.Deserialize(fileStream) as LinkInfo[];
					if (linkInfos != null)
					{
						foreach (LinkInfo linkInfo in linkInfos)
							Add(linkInfo);
					}
				}
			}
		}

		public void LoadFromXml(string fileName)
		{
			Clear();
			filePath = fileName;

			if (File.Exists(fileName))
			{
				XmlTextReader xmlIn = new XmlTextReader(fileName);
				
				try
				{
					xmlIn.MoveToContent();

					if (xmlIn.Name == "SOAP-ENV:Envelope")
					{
						// 旧式のファイルの場合
						xmlIn.Close();
						xmlIn = null;

						LoadFromSoap(fileName);
					}
					else if (xmlIn.Name == "LinkInfoCollection")
					{
						string version = xmlIn.GetAttribute("Version");

						if (version != "1.0")
							throw new ArgumentException();

						ReadXml(xmlIn);
					}

				}
				finally
				{
					if (xmlIn != null)
						xmlIn.Close();
				}
			}
		}

		private void ReadXml(XmlTextReader xmlIn)
		{
			while (xmlIn.Read())
			{
				if (xmlIn.NodeType != XmlNodeType.Element)
					continue;

				if (xmlIn.Name == "Item")
				{
					LinkInfo item = new LinkInfo();
					item.Uri = xmlIn.GetAttribute("Uri");
					item.Text = xmlIn.ReadString();

					List.Add(item);
				}
			}
		}

		/// <summary>
		/// リンクコレクションを保存
		/// </summary>
		/// <param name="fileName"></param>
		public void SaveToXml(string fileName)
		{
			if (fileName == null) {
				throw new ArgumentNullException("fileName");
			}

			XmlTextWriter xmlOut = new XmlTextWriter(fileName, Encoding.GetEncoding("shift_jis"));
			xmlOut.Formatting = Formatting.Indented;

			filePath = fileName;

			try
			{
				xmlOut.WriteStartDocument();

				xmlOut.WriteStartElement("LinkInfoCollection");
				xmlOut.WriteAttributeString("Version", "1.0");

				foreach (LinkInfo item in this.List)
				{
					xmlOut.WriteStartElement("Item");
					xmlOut.WriteAttributeString("Uri", item.Uri);

					xmlOut.WriteString(item.Text);

					xmlOut.WriteEndElement();
				}

				xmlOut.WriteEndElement();
				xmlOut.WriteEndDocument();
			}
			finally
			{
				xmlOut.Close();
			}
	
			//using (FileStream fileStream = new FileStream(fileName, FileMode.Create))
			//{
			//    SoapFormatter soap = new SoapFormatter();
			//    soap.Serialize(fileStream, this.InnerList.ToArray(typeof(LinkInfo)));
			//}
			//			ConfigSerializer.Serialize(fileName, typeof(LinkInfoCollection), this);

		}
	}
}
