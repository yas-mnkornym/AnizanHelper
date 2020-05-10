// SortedValueCollection.cs

namespace Twin
{
	using System;
	using System.Collections;
	using System.Text;
	using System.Collections.Generic;

	/// <summary>
	/// ソートされた数値型を管理するコレクション
	/// </summary>
	public class SortedValueCollection<T> : IEnumerable<T>
	{
		private List<T> values;

		/// <summary>
		/// 要素数を取得
		/// </summary>
		public int Count
		{
			get
			{
				return values.Count;
			}
		}

		/// <summary>
		/// SortedValueCollectionクラスのインスタンスを初期化
		/// </summary>
		public SortedValueCollection()
		{
			// 
			// TODO: コンストラクタ ロジックをここに追加してください。
			//
			values = new List<T>();
		}

		/// <summary>
		/// インデックスindexをコレクションの末尾に追加
		/// </summary>
		/// <param name="index"></param>
		public void Add(T index)
		{
			values.Add(index);
			values.Sort();
		}

		/// <summary>
		/// インデックス配列arrayをコレクションの末尾に追加
		/// </summary>
		/// <param name="array"></param>
		public void AddRange(T[] array)
		{
			values.AddRange(array);
			values.Sort();
		}

		public void SetRange(T[] newArray)
		{
			values.Clear();
			AddRange(newArray);
		}

		/// <summary>
		/// indexがコレクションに含まれているかどうかを判断
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		public bool Contains(T index)
		{
			//return indices.Contains(index);
			return values.BinarySearch(index) >= 0;
		}

		/// <summary>
		/// arrayの中のいずれか1つがコレクションに含まれているかどうかを判断
		/// </summary>
		/// <param name="array"></param>
		/// <returns></returns>
		public bool ContainsAny(T[] array)
		{
			foreach (T index in array)
				if (Contains(index)) return true;

			return false;
		}

		/// <summary>
		/// val をコレクション内から削除
		/// </summary>
		/// <param name="val"></param>
		public void Remove(T val)
		{
			values.Remove(val);
		}

		/// <summary>
		/// コレクションをすべて削除
		/// </summary>
		public void Clear()
		{
			values.Clear();
		}

		public void Copy(SortedValueCollection<T> dest)
		{
			dest.Clear();
			dest.AddRange(values.ToArray());
		}

		/// <summary>
		/// コレクションに登録されている番号を文字列形式に変換
		/// </summary>
		/// <returns></returns>
		public string ToArrayString()
		{
			StringBuilder sb = new StringBuilder();

			foreach (T val in values)
				sb.Append(val).Append(',');

			return sb.ToString();
		}

		/// <summary>
		/// ToArrayString で文字列に変換された SortedValueCollection の値を復元
		/// </summary>
		/// <param name="arrayString"></param>
		public void FromArrayString(string arrayString)
		{
			string[] indices = arrayString.Split(',');
			Clear();

			foreach (string index in indices)
			{
				if (!String.IsNullOrEmpty(index))
				{
					try
					{
						T value = (T)Convert.ChangeType(index, typeof(T));
						Add(value);
					}
					catch (InvalidCastException) {}
				}
			}
		}

		IEnumerator<T> IEnumerable<T>.GetEnumerator()
		{
			return values.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return values.GetEnumerator();
		}
	}
}
