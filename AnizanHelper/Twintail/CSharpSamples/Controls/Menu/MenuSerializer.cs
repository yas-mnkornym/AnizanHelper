// MenuSerializer.cs

namespace CSharpSamples
{
	using System;
	using System.Reflection;
	using System.Text;
	using System.Windows.Forms;
	using System.Xml;

	/// <summary>
	/// メニューの情報をファイルにシリアル化または逆シリアル化を行うクラス
	/// </summary>
	public class MenuSerializer
	{
		/// <summary>
		/// obj内に含まれるMenuItemフィールドの情報をXMLにシリアル化
		/// </summary>
		/// <param name="filePath">保存先ファイルパス</param>
		/// <param name="obj">シリアル化対象のオブジェクト</param>
		public static void Serialize(string filePath, object obj)
		{
			XmlDocument doc = new XmlDocument();

			// menuの型情報を属性に
			XmlAttribute attrType = doc.CreateAttribute("Type");
			attrType.Value = obj.GetType().FullName;

			// メニューのルート情報
			XmlElement root = doc.CreateElement("Menu");
			root.Attributes.Append(attrType);

			// objに存在するすべてのMenuItemをxmlに書き出す
			FieldInfo[] fields = obj.GetType().GetFields(BindingFlags.Instance |
				BindingFlags.NonPublic | BindingFlags.Public);

			foreach (FieldInfo field in fields)
			{
				if (field.FieldType != typeof(ToolStripMenuItem))
				{
					continue;
				}

				ToolStripMenuItem menu = (ToolStripMenuItem)field.GetValue(obj);

				if (menu != null)
				{
					// Text属性
					XmlAttribute attrText = doc.CreateAttribute("Text");
					attrText.Value = menu.Text;

					// Visible属性
					//XmlAttribute attrVisible = doc.CreateAttribute("Visible");
					//attrVisible.Value = menu.Visible.ToString();

					// Shortcut属性
					XmlAttribute attrShortcut = doc.CreateAttribute("Shortcut");
					attrShortcut.Value = menu.ShortcutKeys.ToString();

					XmlNode node = doc.CreateElement("MenuItem");
					node.Attributes.Append(attrText);
					//node.Attributes.Append(attrVisible);
					node.Attributes.Append(attrShortcut);
					node.InnerText = field.Name;

					root.AppendChild(node);
				}
			}

			doc.AppendChild(root);

			XmlTextWriter writer = null;
			try
			{
				writer = new XmlTextWriter(filePath, Encoding.UTF8);
				writer.Indentation = 1;
				writer.IndentChar = '\t';
				writer.Formatting = Formatting.Indented;
				doc.Save(writer);
			}
			finally
			{
				if (writer != null)
				{
					writer.Close();
				}
			}
		}

		/// <summary>
		/// 指定したファイルに保存されているXMLをデシリアル化しobjに読み込む
		/// </summary>
		/// <param name="filePath"></param>
		/// <param name="obj"></param>
		public static void Deserialize(string filePath, object obj)
		{
			XmlDocument doc = new XmlDocument();
			doc.Load(filePath);

			XmlNode root = doc.DocumentElement;

			XmlAttribute attrType = root.Attributes["Type"];
			if (attrType == null)
			{
				throw new ApplicationException("ルートに型情報が存在しません");
			}

			string typeName = attrType.Value;
			if (typeName.Contains("Version"))
			{
				int token = typeName.IndexOf(",");
				if (token >= 0)
				{
					typeName = typeName.Substring(0, token);
				}
			}

			Type type = Type.GetType(typeName);
			XmlNodeList menuItems = root.SelectNodes("MenuItem");

			foreach (XmlNode node in menuItems)
			{
				XmlAttribute attrText = node.Attributes["Text"];
				XmlAttribute attrVisible = node.Attributes["Visible"];
				XmlAttribute attrShortcut = node.Attributes["Shortcut"];

				FieldInfo field = type.GetField(node.InnerText,
					BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

				if (field != null)
				{
					ToolStripMenuItem menu = (ToolStripMenuItem)field.GetValue(obj);
					menu.Text = attrText.Value;
					menu.ShortcutKeys = (Keys)Enum.Parse(typeof(Keys), attrShortcut.Value);

					if (attrVisible != null)
					{
						menu.Visible = bool.Parse(attrVisible.Value);
					}
				}
			}
		}
	}
}
