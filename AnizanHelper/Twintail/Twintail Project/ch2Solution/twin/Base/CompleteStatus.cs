// CompleteStatus.cs

namespace Twin
{
	using System;

	/// <summary>
	/// 完了時の状態を表す
	/// </summary>
	public enum CompleteStatus
	{
		/// <summary>
		/// 成功
		/// </summary>
		Success,
		/// <summary>
		/// エラーが発生
		/// </summary>
		Error,
		/// <summary>
		/// 中止された
		/// </summary>
		Abort,
	}
}
