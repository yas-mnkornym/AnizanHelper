// BbsType.cs

namespace Twin
{
	using System;

	/// <summary>
	/// 対応している掲示板の種類を表す
	/// </summary>
	public enum BbsType
	{
		/// <summary>
		/// 指定なし
		/// </summary>
		None,
		/// <summary>
		/// DAT形式の掲示板を表す (基本的に2ch互換)
		/// </summary>
		Dat,
		/// <summary>
		/// ２ちゃんねる (www.2ch.net)
		/// </summary>
		X2ch,
		/// <summary>
		/// ２ちゃんねる過去ログ(要認証) (www.2ch.net)
		/// </summary>
		X2chAuthenticate,
		/// <summary>
		/// ２ちゃんねる過去ログ (www.2ch.net)
		/// </summary>
		X2chKako,
		/// <summary>
		/// Zeta掲示板 (www.zeta.org)
		/// </summary>
		Zeta,
		/// <summary>
		/// まちBBS (www.machibbs.com)
		/// </summary>
		Machi,
		/// <summary>
		/// したらば掲示板 (www.shitaraba.com)
		/// </summary>
		Shita,
		/// <summary>
		/// JBBS＠したらば (jbbs.shitaraba.com)
		/// </summary>
		Jbbs,
		/// <summary>
		/// Be2ch (be.2ch.net)
		/// </summary>
		Be2ch,
		/// <summary>
		/// ミルクカフェ (www.milkcafe.net)
		/// </summary>
		MilkCafe,
	}
}
