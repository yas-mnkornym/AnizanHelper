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
	/// 讒倥・↑蝙九・Public繝励Ο繝代ユ繧｣蛟､縺ｮ繧ｷ繝ｪ繧｢繝ｩ繧､繧ｺ/繝・す繝ｪ繧｢繝ｩ繧､繧ｺ繧定｡後≧繧ｯ繝ｩ繧ｹ縺ｧ縺吶・
	/// </summary>
	public abstract class ApplicationSettingsSerializer
	{
		public const string Version = "1.0";

		protected Encoding encoding = Encoding.UTF8;

		/// <summary>
		/// 險ｭ螳壹・菫晏ｭ伜・繝輔ぃ繧､繝ｫ蜷阪ｒ蜿門ｾ励＠縺ｾ縺吶・
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
		/// 迴ｾ蝨ｨ縺ｮ繝・・繧ｿ縺悟ｾｩ蜈・＆繧後◆迥ｶ諷九〒縺ゅｋ縺九←縺・°繧堤､ｺ縺励∪縺吶・
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
			if (reset) Reset();
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
		/// DefaultValueAttribute 螻樊ｧ縺瑚ｨｭ螳壹＆繧後※縺・ｋ繝励Ο繝代ユ繧｣縺吶∋縺ｦ繧・DefaultValueAttribute 螻樊ｧ縺ｧ謖・ｮ壹＆繧後◆蛟､縺ｫ險ｭ螳壹＠縺ｾ縺吶・
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
		/// 閾ｪ霄ｫ縺ｮ險ｭ螳壹ヵ繧｡繧､繝ｫ繧貞炎髯､縺励ヽeset 繝｡繧ｽ繝・ラ縺ｫ繧医▲縺ｦ繝励Ο繝代ユ繧｣縺ｮ蛟､繧偵Μ繧ｻ繝・ヨ縺励∪縺吶・
		/// </summary>
		public virtual void Remove()
		{
			Reset();

			if (File.Exists(this.FileName))
				File.Delete(this.FileName);
		}

		/// <summary>
		/// 繝輔ぃ繧､繝ｫ縺九ｉ險ｭ螳壹ｒ蠕ｩ蜈・＠縺ｾ縺吶ゅヵ繧｡繧､繝ｫ縺悟ｭ伜惠縺励↑縺・ｴ蜷医・菴輔ｂ縺励∪縺帙ｓ縲・
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
		/// Load 繝｡繧ｽ繝・ラ縺ｫ繧医▲縺ｦ險ｭ螳壹′豁｣蟶ｸ縺ｫ繝・す繝ｪ繧｢繝ｩ繧､繧ｺ縺輔ｌ縺溘→縺阪↓蜻ｼ縺ｰ繧後ｋ繝｡繧ｽ繝・ラ縺ｧ縺吶・
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
							// set 繝｡繧ｽ繝・ラ縺後↑縺・・繝ｭ繝代ユ繧｣縺ｯ蜃ｦ逅・＠縺ｪ縺・
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
		/// 迴ｾ蝨ｨ縺ｮ險ｭ螳壹ｒ繝輔ぃ繧､繝ｫ縺ｫ菫晏ｭ倥＠縺ｾ縺吶・
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
