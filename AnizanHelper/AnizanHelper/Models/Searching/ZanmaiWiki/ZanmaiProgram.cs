using System.Collections.Generic;

namespace AnizanHelper.Models.Searching.Zanmai
{
	internal class ZanmaiProgram
	{
		public WikiSongListPageLink SongListPageLink { get; set; }
		public string ProgramTitle { get; set; }
		public IList<ZanmaiSongListItem> Songs { get; set; }
	}
}
