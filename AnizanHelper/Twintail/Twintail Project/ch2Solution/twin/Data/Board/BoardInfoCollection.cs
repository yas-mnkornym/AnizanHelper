// BoardInfoCollection.cs
// #2.0

namespace Twin
{
	using System;
	using System.Text;
	using System.Collections.Generic;
	using System.Runtime.Serialization;
	using System.ComponentModel;

	/// <summary>
	/// BoardInfo クラスをコレクション管理します。
	/// </summary>
	[Serializable]
	[TypeConverter(typeof(BoardInfoCollectionConverter))]
	public class BoardInfoCollection : List<BoardInfo>, ISerializable
	{
		/// <summary>
		/// BoardInfoCollectionクラスのインスタンスを初期化
		/// </summary>
		public BoardInfoCollection() 
		{
		}

		public BoardInfoCollection(SerializationInfo info, StreamingContext context)
		{
			BoardInfoCollection items =
				(BoardInfoCollection)info.GetValue("BoardInfoCollection", typeof(BoardInfoCollection));

			AddRange(items);
		}

		void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue("BoardInfoCollection", this);
		}

		/// <summary>
		/// 板名が boardName と一致した板のコレクション内インデックスを取得します。
		/// </summary>
		/// <param name="boardName">検索する板名</param>
		/// <returns>存在すればそのインデックスを返し、見つからなければ -1 を返します。</returns>
		public int IndexOfName(string boardName)
		{
			return FindIndex(new Predicate<BoardInfo>(delegate (BoardInfo bi)
			{
				return bi.Name.Equals(boardName);
			}));
		}

		/// <summary>
		/// 指定した url に一致する板のコレクション内インデックスを取得します。
		/// </summary>
		/// <param name="url">検索するURL</param>
		/// <returns>存在すればそのインデックスを返し、見つからなければ -1 を返します。</returns>
		public int IndexOfUrl(string url)
		{
			return FindIndex(new Predicate<BoardInfo>(delegate(BoardInfo bi)
			{
				return bi.Url.Equals(url);
			}));
		}

		/// <summary>
		/// 格納されている板名を連結した文字列に変換します。
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			StringBuilder sb = new StringBuilder(16 * Count);
			for (int i = 0; i < Count; i++)
			{
				sb.Append(this[i].Name);
				if (i+1 < Count) sb.Append("<>");
			}
			return sb.ToString();
		}
	}
}
