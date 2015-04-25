using System;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace Twintail3
{
	/// <summary>
	/// 様々な型のPublicプロパティ値のシリアライズ/デシリアライズを行うクラスです。
	/// </summary>
	public abstract class ApplicationSettingsSerializer
	{
		public const string Version = "1.0";

		protected Encoding encoding = Encoding.UTF8;

		/// <summary>
		/// 設定の保存先ファイル名を取得します。
		/// </summary>
		private string fileName;
		[XmlIgnore]
		public string FileName
		{
			get
			{
				return fileName;
			}
			protected set
			{
				fileName = value;
			}
		}
//		{ get; protected set; }

		/// <summary>
		/// 現在のデータが復元された状態であるかどうかを示します。
		/// </summary>
		private bool deserialized;
		[XmlIgnore]
		[Browsable(false)]
		public bool IsDeserialized
		{
			get
			{
				return deserialized;
			}
			protected set
			{
				deserialized = value;
			}
		}
		//		{ get; protected set; }
	
		public ApplicationSettingsSerializer(string fileName)
		{
			if (fileName == null)
				throw new ArgumentNullException("fileName");

			this.FileName = fileName;
			this.IsDeserialized = false;
		}

		public ApplicationSettingsSerializer(string fileName, bool reset)
			: this(fileName)
		{
			if (reset)　Reset();
		}

		private Attribute GetCustomAttribute(PropertyInfo property, Type type)
		{
			object[] attributes = property.GetCustomAttributes(type, true);
			return attributes.Length > 0 ? (Attribute)attributes[0] : null;
		}

		private bool GetDefaultValue(PropertyInfo property, out object result)
		{
			result = null;

			DefaultValueAttribute attr = GetCustomAttribute(
				property, typeof(DefaultValueAttribute)) as DefaultValueAttribute;

			if (attr == null)
				return false;

			if (attr.Value.GetType() != property.PropertyType)
			{
				TypeConverter converter = TypeDescriptor.GetConverter(property.PropertyType);

				if (converter == null)
					return false;

				result = converter.ConvertFromString(attr.Value.ToString());
			}
			else
			{
				result = attr.Value;
			}

			return true;
		}

		private bool IsCallConstructor(PropertyInfo property, out object[] arguments)
		{
			CallConstructorAttribute attr = 
				GetCustomAttribute(property, typeof(CallConstructorAttribute)) as CallConstructorAttribute;
			if (attr != null)
			{
				arguments = attr.Arguments;
				return true;
			}
			else
			{
				arguments = null;
				return false;
			}
		}

		/// <summary>
		/// DefaultValueAttribute 属性が設定されているプロパティすべてを DefaultValueAttribute 属性で指定された値に設定します。
		/// </summary>
		public virtual void Reset()
		{
			foreach (PropertyInfo property in GetType().GetProperties())
			{
				object value;
				object[] arguments;

				if (property.PropertyType.IsArray)
				{
					DefaultArraySetAttribute attr = 
						GetCustomAttribute(property, typeof(DefaultArraySetAttribute)) as DefaultArraySetAttribute;
					
					if (attr != null && attr.Length >= 0)
					{
						Array newArrayObj = (Array)Activator.CreateInstance(property.PropertyType, new object[]{attr.Length});

						if (attr.DefaultValue != null)
						{
							for (int i = 0; i < newArrayObj.Length; i++)
								newArrayObj.SetValue(attr.DefaultValue, i);
						}

						property.SetValue(this, newArrayObj, null);
					}
				}
				else if (GetDefaultValue(property, out value))
				{
					property.SetValue(this, value, null);
				}
				else if (IsCallConstructor(property, out arguments))
				{
					object newObject = Activator.CreateInstance(property.PropertyType, arguments);
					property.SetValue(this, newObject, null);
				}
			}
			this.IsDeserialized = false;
		}

		/// <summary>
		/// 自身の設定ファイルを削除し、Reset メソッドによってプロパティの値をリセットします。
		/// </summary>
		public virtual void Remove()
		{
			Reset();

			if (File.Exists(this.FileName))
				File.Delete(this.FileName);
		}

		/// <summary>
		/// ファイルから設定を復元します。ファイルが存在しない場合は何もしません。
		/// </summary>
		public virtual void Load()
		{
			if (File.Exists(this.FileName))
			{
				using (XmlTextReader r = new XmlTextReader(this.FileName))
					ReadFromXml(r, this);

				OnDeserialized();
			}
		}

		public static void ReadFromXml(XmlTextReader xmlIn, object instanceObj)
		{
			XmlDocument document = new XmlDocument();
			document.Load(xmlIn);

			XmlNode root = document.DocumentElement;

			if (root == null || root.Name != instanceObj.GetType().Name)
				throw new ArgumentException();

			if (root.Attributes["Version"].Value != Version)
				throw new ArgumentException();

			ReadFromXmlInternal(root, instanceObj);
		}

		/// <summary>
		/// Load メソッドによって設定が正常にデシリアライズされたときに呼ばれるメソッドです。
		/// </summary>
		protected virtual void OnDeserialized()
		{
			this.IsDeserialized = true;
		}

		private static bool IsExpandableSerialize(MemberInfo property)
		{
			object[] attributes = property.GetCustomAttributes(typeof(ExpandableSerializeAttribute), true);
			return attributes.Length > 0;
		}

		private static void ReadFromXmlInternal(XmlNode parent, object instanceObj)
		{
			Type type = instanceObj.GetType();

			foreach (XmlElement node in parent.ChildNodes)
			{
				PropertyInfo property = type.GetProperty(node.Name);

				if (property != null)
				{
					TypeConverter customConv = GetCosutomTypeConverter(property);
					if (customConv != null)
					{
						object newObj = customConv.ConvertFromString(node.InnerText);
						property.SetValue(instanceObj, newObj, null);
					}
					else if (property.PropertyType.IsArray)
					{
						int arrayLength = node.ChildNodes.Count;
						int arrayIndex = 0;

						Array newArrayObj = (Array)Activator.CreateInstance(property.PropertyType, new object[] { arrayLength });

						foreach (XmlNode child in node.ChildNodes)
						{
							Type elementType = property.PropertyType.GetElementType();
							object newValueObj;
							if (elementType.IsArray || IsExpandableSerialize(property))
							{
								newValueObj = Activator.CreateInstance(elementType);
								ReadFromXmlInternal(child, newValueObj);
							}
							else
							{
								TypeConverter conv = GetDefaultTypeConverter(null, elementType);
								newValueObj = conv.ConvertFromString(child.InnerText);
							}
							newArrayObj.SetValue(newValueObj, arrayIndex++);
						}

						property.SetValue(instanceObj, newArrayObj, null);
					}
					else
					{
						object objectVal = property.GetValue(instanceObj, null);

						if (IsExpandableSerialize(property))
						{
							ReadFromXmlInternal(node, objectVal);
						}
						else
						{
							// set メソッドがないプロパティは処理しない
							if (property.GetSetMethod() == null)
								continue;

							TypeConverter converter = GetDefaultTypeConverter(objectVal, property.PropertyType);
							object setValue = converter.ConvertFromString(node.InnerText);
							property.SetValue(instanceObj, setValue, null);
						}
					}
				}
			}
		}

		/// <summary>
		/// 現在の設定をファイルに保存します。
		/// </summary>
		public virtual void Save()
		{
			FileInfo fi = new FileInfo(this.FileName);
			if (!fi.Directory.Exists)
				fi.Directory.Create();

			using (XmlTextWriter w = new XmlTextWriter(this.FileName, this.encoding))
				WriteToXml(w, this);
		}

		public static void WriteToXml(XmlTextWriter xmlOut, object instanceObj)
		{
			Type rootType = instanceObj.GetType();

			xmlOut.Formatting = Formatting.Indented;

			xmlOut.WriteStartDocument();
			xmlOut.WriteStartElement(rootType.Name);
			xmlOut.WriteAttributeString("Version", Version);

			WriteToXml(xmlOut, rootType.GetProperties(), instanceObj);

			xmlOut.WriteEndElement();
			xmlOut.WriteEndDocument();
		}

		private static void WriteToXml(XmlTextWriter xmlOut, PropertyInfo[] properties, object instanceObj)
		{
			foreach (PropertyInfo propertyInfo in properties)
			{
				if (!propertyInfo.IsDefined(typeof(XmlIgnoreAttribute), true))
				{
					xmlOut.WriteStartElement(propertyInfo.Name);

					if (instanceObj != null)
					{
						object value = propertyInfo.GetValue(instanceObj, null);
						
						TypeConverter customConv = GetCosutomTypeConverter(propertyInfo);
						if (customConv != null)
						{
							xmlOut.WriteString(customConv.ConvertToString(value));
						}
						else if (propertyInfo.PropertyType.IsArray)
						{
							Array array = (Array)value;

							foreach (object obj in array)
							{
								Type objType = obj.GetType();
								xmlOut.WriteStartElement(objType.Name);

								if (objType.IsArray || IsExpandableSerialize(propertyInfo))
								{
									WriteToXml(xmlOut, objType.GetProperties(), obj);
								}
								else
								{
									TypeConverter conv = GetDefaultTypeConverter(obj, propertyInfo.PropertyType);
									xmlOut.WriteString(conv.ConvertToString(obj));
								}

								xmlOut.WriteEndElement();
							}
						}
						else if (IsExpandableSerialize(propertyInfo))
						{
							WriteToXml(xmlOut, propertyInfo.PropertyType.GetProperties(), value);
						}
						else
						{
							TypeConverter converter = GetDefaultTypeConverter(value, propertyInfo.PropertyType);
							xmlOut.WriteString(converter.ConvertToString(value));
						}
					}
					xmlOut.WriteEndElement();
				}
			}
		}

		private static TypeConverter GetDefaultTypeConverter(object objectVal, Type type)
		{
			if (objectVal == null)
			{
				return TypeDescriptor.GetConverter(type);
			}
			else
			{
				return TypeDescriptor.GetConverter(objectVal);
			}
		}

		private static TypeConverter GetCosutomTypeConverter(PropertyInfo property)
		{
			object[] attributes = property.GetCustomAttributes(typeof(TypeConverterAttribute), true);
			if (attributes.Length > 0)
			{
				string converterTypeName =
					(attributes[0] as TypeConverterAttribute).ConverterTypeName;

				return (TypeConverter)Activator.CreateInstance(Type.GetType(converterTypeName));
			}
			else return null;
		}
	}
}
