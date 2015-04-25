// ProgramSettingscs

namespace CSharpSamples
{
	using System;
	using System.IO;
	using System.Xml;
	using System.Diagnostics;
	using System.ComponentModel;

	/// <summary>
	/// プログラム設定などの情報を Xml 形式で管理する機能を提供します。
	/// </summary>
	public class ProgramSettings
	{
		private string fileName;
		private XmlDocument document;

		/// <summary>
		/// ProgramSettingsクラスのインスタンスを初期化。
		/// </summary>
		/// <param name="fileName">設定情報のファイル名。</param>
		public ProgramSettings(string fileName)
		{
			if (fileName == null) {
				throw new ArgumentNullException("fileName");
			}
			// 
			// TODO: コンストラクタ ロジックをここに追加してください。
			//
			this.fileName = fileName;
			this.document = new XmlDocument();

			try {
				if (File.Exists(fileName))
				{
					document.Load(fileName);
				}
				else {
					CreateRoot();
				}
			}
			catch (Exception ex) {
				Debug.WriteLine(ex.ToString());
				CreateRoot();
			}
		}

		/// <summary>
		/// ルートを作成します。
		/// </summary>
		private void CreateRoot()
		{
			XmlNode root = document.CreateElement("Settings");
			document.AppendChild(root);
		}

		/// <summary>
		/// 指定したキーの値を取得します。
		/// </summary>
		/// <param name="key"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		public object Get(string key, Type type)
		{
			if (key == null) {
				throw new ArgumentNullException("key");
			}

			XmlElement element = document.DocumentElement;
			XmlNode node = element.SelectSingleNode(key);
			
			if (node != null)
			{
				TypeConverter converter = TypeDescriptor.GetConverter(type);
				
				if (converter == null)
					throw new ArgumentException(type + "型に対応する TypeConverter が見つかりません。", "type");

				return converter.ConvertFromString(node.InnerText);
			}

			return null;
		}

		/// <summary>
		/// 指定したセクションとキーに設定されている値を取得します。
		/// </summary>
		/// <param name="section"></param>
		/// <param name="key"></param>
		/// <returns></returns>
		public object Get(string section, string key, Type type)
		{
			if (section == null) {
				throw new ArgumentNullException("section");
			}
			if (key == null) {
				throw new ArgumentNullException("key");
			}

			XmlElement element = document.DocumentElement;
			XmlNode node = element.SelectSingleNode(section + "/" + key);
			
			if (node != null)
			{
				TypeConverter converter = TypeDescriptor.GetConverter(type);
				
				if (converter == null)
					throw new ArgumentException(type + "型に対応する TypeConverter が見つかりません。", "type");

				return converter.ConvertFromString(node.InnerText);
			}

			return null;
		}

		/// <summary>
		/// 指定したキーに値を設定します。
		/// </summary>
		/// <param name="key"></param>
		/// <param name="value"></param>
		public void Set(string key, object value)
		{
			if (key == null) {
				throw new ArgumentNullException("key");
			}
			if (value == null) {
				throw new ArgumentNullException("value");
			}

			TypeConverter converter = TypeDescriptor.GetConverter(value.GetType());
			
			if (converter == null)
				throw new ArgumentException("指定したオブジェクトに対応する TypeConverter が見つかりません。", "value");

			// ルート要素を取得
			XmlElement root = document.DocumentElement;

			// セクションが存在するノードを取得
			XmlNode node = root.SelectSingleNode(key);
			if (node == null)
			{
				node = root.AppendChild(
					document.CreateElement(key));
			}

			node.InnerText = converter.ConvertToString(value);
		}

		/// <summary>
		/// 指定したセクションの指定したキーに値を設定します。
		/// </summary>
		/// <param name="section">セクション名</param>
		/// <param name="key">キー名</param>
		/// <param name="value">設定する値</param>
		public void Set(string section, string key, object value)
		{
			if (section == null) {
				throw new ArgumentNullException("section");
			}
			if (key == null) {
				throw new ArgumentNullException("key");
			}
			if (value == null) {
				throw new ArgumentNullException("value");
			}

			TypeConverter converter = TypeDescriptor.GetConverter(value.GetType());
			
			if (converter == null)
				throw new ArgumentException("指定したオブジェクトに対応する TypeConverter が見つかりません。", "value");

			// ルート要素を取得
			XmlElement root = document.DocumentElement;

			// セクションが存在するノードを取得
			XmlNode parent = root.SelectSingleNode(section);
			if (parent == null)
			{
				parent = root.AppendChild(
					document.CreateElement(section));
			}

			// 値を格納するノードを取得
			XmlNode child = parent.SelectSingleNode(key);
			if (child == null)
			{
				child = parent.AppendChild(
					document.CreateElement(key));
			}

			child.InnerText = converter.ConvertToString(value);
		}

		/// <summary>
		/// 設定情報をファイルに保存します。
		/// </summary>
		public void Save()
		{
			document.Save(fileName);
		}
	}
}
