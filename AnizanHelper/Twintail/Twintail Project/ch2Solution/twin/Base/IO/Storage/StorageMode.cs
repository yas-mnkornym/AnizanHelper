// StorageMode.cs

namespace Twin.IO
{
	using System;

	/// <summary>
	/// ストレージを開く際の動作を表す
	/// </summary>
	public enum StorageMode
	{
		/// <summary>
		/// 読み込み用に開く
		/// </summary>
		Read,
		/// <summary>
		/// 書き込みように開く
		/// </summary>
		Write,
	}
}
