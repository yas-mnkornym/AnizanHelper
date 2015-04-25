// CategoryCollection.cs

namespace Twin
{
	using System;
	using System.Collections;
	using System.Runtime.Serialization;

	/// <summary>
	/// Categoryクラスをコレクション管理
	/// </summary>
	public class CategoryCollection : CollectionBase
	{
		/// <summary>
		/// 指定したインデックスのボードアイテムを取得
		/// </summary>
		public Category this[int index] {
			get {
				return (Category)List[index];
			}
		}

		/// <summary>
		/// CategoryCollectionクラスのインスタンスを初期化
		/// </summary>
		public CategoryCollection() 
		{
		}

		/// <summary>
		/// コレクションにカテゴリを追加
		/// </summary>
		/// <param name="item">追加するCategoryクラス</param>
		/// <returns>追加された位置</returns>
		public int Add(Category item)
		{
			return List.Add(item);
		}

		/// <summary>
		/// 複数のカテゴリを追加
		/// </summary>
		/// <param name="items"></param>
		public void AddRange(CategoryCollection items)
		{
			InnerList.AddRange(items);
		}

		/// <summary>
		/// 指定したインデックスにカテゴリを挿入
		/// </summary>
		/// <param name="index">挿入するインデックス</param>
		/// <param name="item">挿入するCategoryクラス</param>
		public void Insert(int index, Category item)
		{
			List.Insert(index, item);
		}

		/// <summary>
		/// 指定したカテゴリを削除
		/// </summary>
		/// <param name="item">削除するカテゴリ</param>
		public void Remove(Category item)
		{
			List.Remove(item);
		}
	}
}
