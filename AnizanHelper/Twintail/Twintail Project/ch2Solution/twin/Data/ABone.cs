// ABone.cs

namespace Twin
{
	using System;
	using System.Runtime.Serialization;

	/// <summary>
	/// あぼーん情報を表す
	/// </summary>
	[Serializable]
	[System.ComponentModel.TypeConverter(typeof(TwinExpandableConverter))]
	public class ABone : ISerializable
	{
		public bool Visible;
		public bool Chain;

		/// <summary>
		/// ABoneクラスのインスタンスを初期化
		/// </summary>
		public ABone()
		{
			// 
			// TODO: コンストラクタ ロジックをここに追加してください。
			//
			this.Visible = true;
			this.Chain = false;
		}

		/// <summary>
		/// ABoneクラスのインスタンスを初期化
		/// </summary>
		/// <param name="visible"></param>
		/// <param name="chain"></param>
		public ABone(bool visible, bool chain) : this()
		{
			this.Visible = visible;
			this.Chain = chain;
		}

		public ABone(SerializationInfo info, StreamingContext context)
		{
			try{
			Visible = info.GetBoolean("Visible");
			Chain = info.GetBoolean("Chain");}catch{}
		}

		void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
		{
			try{
			info.AddValue("Visible", Visible);
			info.AddValue("Chain", Chain);}catch{}
		}
	}
}
