// ResSetElement.cs

namespace Twin
{
	using System;

	/// <summary>
	/// レスの要素を表す
	/// </summary>
	public enum ResSetElement
	{
		Unknown = -1,

		/// <summary>すべて対象</summary>
		All = 0,
		/// <summary>Nameが対象</summary>
		Name,
		/// <summary>Emailが対象</summary>
		Email,
		/// <summary>IDが対象</summary>
		ID,
		/// <summary>本分が対象</summary>
		Body,

		/// <summary>日付文字列が対象</summary>
		DateString,

	}
}
