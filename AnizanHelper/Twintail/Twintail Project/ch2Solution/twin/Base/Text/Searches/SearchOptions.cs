// SearchOptions.cs

namespace Twin.Text
{
	using System;

	/// <summary>
	/// 検索オプションを表す
	/// </summary>
	[Flags]
	public enum SearchOptions
	{
		None = 0x000,
		/// <summary>
		/// ドキュメントの終わりから検索を開始する
		/// </summary>
		RightToLeft = 0x001,
		/// <summary>
		/// 単語単位で検索
		/// </summary>
		WholeWordsOnly = 0x002,
		/// <summary>
		/// 大文字と小文字の区別
		/// </summary>
		MatchCase = 0x004,
		/// <summary>
		/// 正規表現検索
		/// </summary>
		Regex = 0x008,
	}
}
