using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnizanHelper.Models
{
	public class ReplaceInfo
	{
		public ReplaceInfo(string original, string replaced, bool exact = false)
		{
			Original = original;
			Replaced = replaced;
			Exact = exact;
		}

		/// <summary>
		/// 置換対象の文字列
		/// </summary>
		public string Original { get; set; }
		
		/// <summary>
		/// 置換語の文字列
		/// </summary>
		public string Replaced { get; set; }

		/// <summary>
		/// trueなら完全一致の場合にのみ置換する
		/// </summary>
		public bool Exact { get; set; }
	}
}
