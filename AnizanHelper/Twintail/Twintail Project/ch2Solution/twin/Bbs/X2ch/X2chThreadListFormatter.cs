// X2chThreadListFormatter.cs
// #2.0

namespace Twin.Bbs
{
	using System;
	using System.Text;
	using System.Collections.Generic;
	using Twin.Text;

	/// <summary>
	/// ThreadHeaderをsubject.txt形式に変換する機能を提供
	/// </summary>
	public class X2chThreadListFormatter : ThreadListFormatter
	{
		/// <summary>
		/// 指定したヘッダーを書式化して文字列に変換
		/// </summary>
		public override string Format(ThreadHeader header)
		{
			if (header == null)
			{
				throw new ArgumentNullException("header");
			}

			StringBuilder sb =
				new StringBuilder(128);

			// 書式: key.dat<>subject (rescount)
			sb.Append(header.Key);
			sb.Append(".dat");
			sb.Append("<>");
			sb.Append(header.Subject);
			sb.Append(" (");
			sb.Append(header.ResCount);
			sb.Append(")");

			return sb.ToString();
		}

		/// <summary>
		/// 指定したヘッダーコレクションを書式化して文字列に変換
		/// </summary>
		public override string Format(List<ThreadHeader> items)
		{
			if (items == null)
			{
				throw new ArgumentNullException("items");
			}

			StringBuilder sb =
				new StringBuilder(128 * items.Count);

			foreach (ThreadHeader header in items)
			{
				sb.Append(Format(header));
				sb.Append('\n');
			}

			return sb.ToString();
		}
	}
}
