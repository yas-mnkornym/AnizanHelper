using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;

namespace AnizanHelper.Models.SettingComponents
{
	internal class DataContractSettingsSerializer : ISettingsSerializer
	{
		public DataContractSettingsSerializer()
		{ }

		#region ISettingsSerializer メンバー

		public void Serialize(System.IO.Stream stream, SettingsImpl settings)
		{
			var serializationInfoArray = settings.SettingsData.Select(x => SerializeSerializaeInfo(x.Key, x.Value, settings.KnownTypes)).ToArray();
			DataContractSerializer serializer = new DataContractSerializer(typeof(KeyValueSerializationInfo[]));
			serializer.WriteObject(stream, serializationInfoArray);
		}

		public void Deserialize(System.IO.Stream stream, SettingsImpl settings)
		{
			DataContractSerializer serializer = new DataContractSerializer(typeof(KeyValueSerializationInfo[]));
			var serializationInfoArray = (KeyValueSerializationInfo[])serializer.ReadObject(stream);
			var deserialized = serializationInfoArray.Select(x => new { Key = x.Key, Value = DeserializSeserializeInfo(x, settings.KnownTypes) });
			settings.Clear();
			foreach (var des in deserialized) {
				settings.Set(des.Key, des.Value);
			}
		}

		#endregion

		object DeserializSeserializeInfo(KeyValueSerializationInfo info, IEnumerable<Type> knownTypes)
		{
			if (info == null) { throw new ArgumentNullException("info"); }

			// 設定の型を取得
			Type type = Type.GetType(info.Type, false);
			if (type == null) {
				foreach (var knownType in knownTypes) {
					if (knownType.FullName == info.Type) {
						type = knownType;
						break;
					}
				}
			}

			// 型が取得できなかったらnullを返す
			if (type == null) {
				return null;
			}

			// デシリアライズ
			using (var xmlReader = new XmlTextReader(new StringReader(info.Value))) {
				DataContractSerializer serializer = new DataContractSerializer(type, knownTypes);
				return serializer.ReadObject(xmlReader);
			}
		}


		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:オブジェクトを複数回破棄しない")]
		KeyValueSerializationInfo SerializeSerializaeInfo(string key, object value, IEnumerable<Type> knownTypes)
		{
			if (key == null) { throw new ArgumentNullException("key"); }
			if (value == null) { throw new ArgumentNullException("value"); }
			var serializationInfo = new KeyValueSerializationInfo {
				Key = key,
				Type = value.GetType().FullName
			};

			using (var writer = new StringWriter())
			using (var xmlWriter = new XmlTextWriter(writer)) {
				DataContractSerializer serializer = new DataContractSerializer(value.GetType(), knownTypes);
				serializer.WriteObject(xmlWriter, value);

				// シリアライズした文字列を取得
				serializationInfo.Value = writer.ToString();
			}
			return serializationInfo;
		}
	}

	[DataContract]
	class KeyValueSerializationInfo
	{
		[DataMember]
		public string Type { get; set; }

		[DataMember]
		public string Key { get; set; }

		[DataMember]
		public string Value { get; set; }
	}
}
