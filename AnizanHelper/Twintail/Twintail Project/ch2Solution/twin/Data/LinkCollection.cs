// LinkCollection.cs

namespace Twin
{
	using System;
	using System.Collections.Specialized;
	using System.Collections;
	using System.IO;

	/// <summary>
	/// リンク文字列をコレクション管理
	/// </summary>
	public class LinkCollection : StringCollection
	{
		/// <summary>
		/// 指定した拡張子のリンクを取得 (OR演算子で複数指定可能)
		/// 例: LinkCollection[".jpg|.gif"]
		/// </summary>
		public string[] this[string extension]
		{
			get
			{
				ArrayList matches = new ArrayList();
				string[] array = extension.Split('|');

				if (array.Length > 0 && Count > 0)
				{
					// 同じ拡張子を持つリンクを検索
					foreach (string link in this)
					{
						foreach (string ext in array)
						{
							if (link.ToLower().EndsWith(ext.ToLower()))
								matches.Add(link);
						}
					}
				}
				return (string[])matches.ToArray(typeof(string));
			}
		}

		/// <summary>
		/// LinkCollectionクラスのインスタンスを初期化
		/// </summary>
		public LinkCollection()
		{
			// 
			// TODO: コンストラクタ ロジックをここに追加してください。
			//
		}
	}
}
