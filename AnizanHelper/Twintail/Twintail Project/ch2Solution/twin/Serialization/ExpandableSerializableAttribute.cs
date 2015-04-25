using System;

namespace Twintail3
{
	/// <summary>
	/// 対象のクラスが ApplicationSettingsSerializer によってシリアライズ可能であることを示す属性です。
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Property)]
	public class ExpandableSerializeAttribute : Attribute { }
}
