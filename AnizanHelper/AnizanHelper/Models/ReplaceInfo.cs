namespace AnizanHelper.Models
{
	public class ReplaceInfo
	{
		public ReplaceInfo(string original, string replaced, bool exact = false)
		{
			this.Original = original;
			this.Replaced = replaced;
			this.Exact = exact;
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
		/// 曲名制約
		/// </summary>
		public string SongTitleConstraint { get; set; }

		/// <summary>
		/// trueなら完全一致の場合にのみ置換する
		/// </summary>
		public bool Exact { get; set; }
	}
}
