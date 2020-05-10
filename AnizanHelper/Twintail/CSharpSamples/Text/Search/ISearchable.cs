// ISearchable.cs

namespace CSharpSamples.Text.Search
{
	/// <summary>
	/// 検索インターフェースを表す
	/// </summary>
	public interface ISearchable
	{
		/// <summary>
		/// 検索パターンを取得
		/// </summary>
		string Pattern
		{
			get;
		}

		/// <summary>
		/// 文字列照合を行う
		/// </summary>
		/// <param name="text">検索文字列</param>
		/// <returns></returns>
		int Search(string text);

		/// <summary>
		/// 指定したインデックスから文字列照合を行う
		/// </summary>
		/// <param name="text">検索文字列</param>
		/// <param name="index">検索開始インデックス</param>
		/// <returns></returns>
		int Search(string text, int index);
	}
}
