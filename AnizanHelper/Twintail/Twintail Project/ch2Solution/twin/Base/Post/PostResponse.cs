// PostResponse.cs

namespace Twin
{
	using System;

	/// <summary>
	/// 投稿時のサーバーからの応対を表す
	/// </summary>
	public enum PostResponse
	{
		/// <summary>指定なし</summary>
		None,
		/// <summary>投稿に成功</summary>
		Success,
		/// <summary>クッキー確認</summary>
		Cookie,
		/// <summary>タイムアウトで投稿できない</summary>
		Timeout,
		/// <summary>書き込みは出来たが注意付き</summary>
		Attention,
		/// <summary>何らかの警告が発生</summary>
		Warning,
		/// <summary>何らかのエラーが発生</summary>
		Error,
		/// <summary>Sambaエラー</summary>
		Samba,
	}
}
