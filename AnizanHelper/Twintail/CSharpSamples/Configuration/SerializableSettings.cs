// SerializableSettings.cs

namespace CSharpSamples
{
	using System;
	using System.Runtime.Serialization;

	/// <summary>
	/// シリアライズ可能な設定の基本クラス
	/// </summary>
	[Serializable]
	public abstract class SerializableSettings : ISerializable
	{
		protected SerializableSettings()
		{
		}

		protected SerializableSettings(SerializationInfo info, StreamingContext context)
		{
			Serializer.Deserialize(this, info);
		}

		void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
		{
			Serializer.Serialize(this, info);
		}
	}
}
