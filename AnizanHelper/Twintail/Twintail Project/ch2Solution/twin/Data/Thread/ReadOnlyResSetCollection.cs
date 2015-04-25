// ReadOnlyResSetCollection.cs

namespace Twin
{
	using System;
	using System.Collections;

	/// <summary>
	/// 読み取り専用のコレクションを表す
	/// </summary>
	public class ReadOnlyResSetCollection
	{
		private ResSetCollection collection;

		public object SyncRoot {
			get {
				return collection.SyncRoot;
			}
		}

		public int Count {
			get {
				return collection.Count;
			}
		}

		public ResSet this[int index] {
			get {
				return (ResSet)collection[index];
			}
		}

		/// <summary>
		/// ReadOnlyResSetCollectionクラスのインスタンスを初期化
		/// </summary>
		/// <param name="items"></param>
		public ReadOnlyResSetCollection(ResSetCollection items)
		{
			// 
			// TODO: コンストラクタ ロジックをここに追加してください。
			//
			collection = items;
			//list = ArrayList.ReadOnly(list);
		}

		public IEnumerator GetEnumerator()
		{
			return collection.GetEnumerator();
		}


	}
}
