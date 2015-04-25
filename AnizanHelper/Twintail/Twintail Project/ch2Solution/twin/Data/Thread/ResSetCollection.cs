// ResSetCollection.cs

namespace Twin
{
	using System;
	using System.Collections;
	using System.Collections.Generic;

	/// <summary>
	/// ResSet構造体をコレクション管理
	/// </summary>
	public class ResSetCollection : CollectionBase, IEnumerable<ResSet>
	{
		/// <summary>
		/// コレクションを同期させるためのオブジェクトを取得
		/// </summary>
		public object SyncRoot {
			get {
				return List.SyncRoot;
			}
		}

		/// <summary>
		/// 指定したインデックスのレスを取得または設定
		/// </summary>
		public ResSet this[int index] {
			set {
				List[index] = value;
			}
			get {
				return (ResSet)List[index];
			}
		}

		/// <summary>
		/// すべてのVisibleフラグを設定
		/// </summary>
		public bool Visible {
			set {
				for (int i = 0; i < Count; i++)
				{
					ResSet res = this[i];
					res.Visible = value;

					this[i] = res;
				}
			}
		}

		/// <summary>
		/// すべての新着フラグを設定
		/// </summary>
		public bool IsNew {
			set {
				for (int i = 0; i < Count; i++)
				{
					ResSet res = this[i];
					res.IsNew = value;

					this[i] = res;
				}
			}
		}

		/// <summary>
		/// ResSetCollectionクラスのインスタンスを初期化
		/// </summary>
		public ResSetCollection()
		{
			InnerList.Capacity = 1000;
		}

		public ResSetCollection(ResSetCollection coll)
		{
			InnerList.Capacity = coll.Count;
			AddRange(coll);
		}

		/// <summary>
		/// レスをコレクションに追加
		/// </summary>
		/// <param name="resSet">追加するResSet構造体</param>
		/// <returns>追加したインデックス値</returns>
		public int Add(ResSet resSet)
		{
			return List.Add(resSet);
		}

		/// <summary>
		/// ResSet構造体の配列をコレクションに追加
		/// </summary>
		public void AddRange(ResSet[] items)
		{
			InnerList.AddRange(items);
		}

		/// <summary>
		/// ResSetコレクションをコレクションに追加
		/// </summary>
		public void AddRange(ResSetCollection resCollection)
		{
			InnerList.AddRange(resCollection);
		}

		/// <summary>
		/// すべてのレスの逆参照回数 (BackReferenceCount プロパティ) をリセットします。
		/// </summary>
		public void ResetBackReferenceCount()
		{
			for (int i = 0; i < Count; i++)
			{
				ResSet r = this[i];
				r.BackReferencedList.Clear();

				this[i] = r;
			}
		}

		/// <summary>
		/// 指定した 1 から始まるレス番号を指定してレスを取得。
		/// このメソッドは、コレクションの順序とレス番号の順序が一致している場合のみに使用。
		/// そうでない場合は、indices で指定しても、返されるコレクションのレス番号と一致している保証は無い。
		/// 指定したレス番号のデータが欲しい場合は、GetRangeOfNumber メソッドを使用する。
		/// </summary>
		/// <param name="indices"></param>
		/// <returns></returns>
		public ResSetCollection GetRange(int[] indices)
		{
			if (indices == null) {
				throw new ArgumentNullException("indices");
			}

			ResSetCollection result = 
				new ResSetCollection();

			foreach (int index in indices)
			{
				if (index > 0 && index <= Count)
				{
					result.Add(this[index - 1]);
				}
			}

			return result;
		}

		/// <summary>
		/// 1 から始まるレス番号の指定した範囲を取得。
		/// このメソッドは、コレクションの順序とレス番号の順序が一致している場合のみに使用。
		/// そうでない場合は、beginIndex と endIndex で指定しても、返されるコレクションのレス番号と一致している保証は無い。
		/// 指定したレス番号のデータが欲しい場合は、GetRangeOfNumber メソッドを使用する。
		/// </summary>
		/// <param name="beginIndex"></param>
		/// <param name="endIndex"></param>
		/// <returns></returns>
		public ResSetCollection GetRange(int beginIndex, int endIndex)
		{
			if (beginIndex < 1 || endIndex < 1 || beginIndex > endIndex)
				throw new ArgumentOutOfRangeException();

			var result = new ResSetCollection();
			for (int i = beginIndex; i <= endIndex; i++)
				result.Add(this[i - 1]);

			return result;
		}

		public ResSetCollection GetRangeOfNumber(int beginResNumber, int endResNumber)
		{
			if (beginResNumber < 1 || endResNumber < 1 || beginResNumber > endResNumber)
				throw new ArgumentOutOfRangeException();

			var num = new List<int>();
			for (int i = beginResNumber; i <= endResNumber; i++) num.Add(i);
			return GetRangeOfNumber(num.ToArray());
		}

		public ResSetCollection GetRangeOfNumber(int[] resNumbers)
		{
			var result = new ResSetCollection();

			foreach (ResSet res in List)
			{
				foreach (int n in resNumbers)
				{
					if (res.Index == n)
						result.Add(res);
				}
			}

			return result;
		}

		/// <summary>
		/// 指定したインデックスをあぼーん
		/// </summary>
		/// <param name="index">削除するレス番号</param>
		/// <param name="visible">可視あぼーんならtrue</param>
		public ResSet ABone(int index, bool visible, ABoneType type, string description)
		{
			int st = 0;
			int ed = Count-1;

			while (st <= ed)
			{
				int mid = (st + ed) / 2;
				ResSet res = this[mid];

				if (res.Index > index)
					ed = mid - 1;
				else if (res.Index < index)
					st = mid + 1;
				else {
					st = mid;
					break;
				}
			}

			// あぼーん
			this[st] = ResSet.ABone(this[st], visible, type, description);

			return this[st];
		}

		public int[] GetBackReferences(int index)
		{
			List<int> indices = new List<int>();

			foreach (ResSet res in this)
			{
				if (res.RefIndices.Length >= 50)
					continue;

				foreach (int n in res.RefIndices)
				{
					if (n == index && !indices.Contains(res.Index))
					{
						indices.Add(res.Index);
					}
				}
			}
			return indices.ToArray();
		}

		IEnumerator<ResSet> IEnumerable<ResSet>.GetEnumerator()
		{
			for (int i = 0; i < List.Count; i++)
				yield return this[i];
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return List.GetEnumerator();
		}

		public int[] IndicesFromID(string idstr)
		{
			var indices = new List<int>();
			foreach (ResSet res in this)
				if (res.ID == idstr) indices.Add(res.Index);
			return indices.ToArray();
		}
	}
}
