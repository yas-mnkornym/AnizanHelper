// LinkInfo.cs

namespace Twin
{
	using System;
	using System.Runtime.Serialization;

	/// <summary>
	/// リンクとその説明を表す
	/// </summary>
	[Serializable]
	public class LinkInfo : ISerializable
	{
		private string uri;
		private string text;

		/// <summary>
		/// リンクのURIを取得または設定
		/// </summary>
		public string Uri {
			set {
				if (value == null) {
					throw new ArgumentNullException("Uri");
				}
				uri = value;
			}
			get { return uri; }
		}

		/// <summary>
		/// リンクに関しての説明を取得または設定
		/// </summary>
		public string Text {
			set {
				if (value == null) {
					throw new ArgumentNullException("Text");
				}
				text = value;
			}
			get { return text; }
		}

		/// <summary>
		/// LinkInfoクラスのインスタンスを初期化
		/// </summary>
		/// <param name="uri"></param>
		/// <param name="text"></param>
		public LinkInfo()
		{
			// 
			// TODO: コンストラクタ ロジックをここに追加してください。
			//
			this.uri = String.Empty;
			this.text = String.Empty;
		}

		/// <summary>
		/// LinkInfoクラスのインスタンスを初期化
		/// </summary>
		/// <param name="uri"></param>
		/// <param name="text"></param>
		public LinkInfo(string uri, string text)
		{
			// 
			// TODO: コンストラクタ ロジックをここに追加してください。
			//
			this.uri = uri;
			this.text = text;
		}

		public LinkInfo(SerializationInfo info, StreamingContext context)
		{
			uri = info.GetString("Uri");
			text = info.GetString("Text");
		}

		void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
		{			
			info.AddValue("Uri", uri);
			info.AddValue("Text", text);
		}
	}
}
