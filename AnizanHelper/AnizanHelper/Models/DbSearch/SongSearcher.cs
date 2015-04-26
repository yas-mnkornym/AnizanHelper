using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AnizanHelper.Models.DbSearch
{
	public class SongSearcher : DbSeacherBase<GeneralSongInfo>
	{
		static readonly string SearchType = "song";

		override public IEnumerable<GeneralSongInfo> Search(string searchWord)
		{
			var doc = QueryDocument(searchWord, SearchType);

			var tbody = doc.DocumentNode.SelectSingleNode("/html[1]/body[1]/table[2]/tbody[1]");
			if (tbody == null) {
				return Enumerable.Empty<GeneralSongInfo>();
			}
			return tbody
				.Descendants("tr")
				.Select(tr => {
					var tds = tr.Descendants("td").ToArray();
					return new GeneralSongInfo {
						Title = tds[0].InnerText,
						Singers = tds[1].Descendants("a").Any() ?
							tds[1].Descendants("a").Select(x => x.InnerText).ToArray() : 
							new string[] { tds[1].InnerText },
						Genre = tds[2].InnerText.Replace(" ", ""),
						Series = tds[3].InnerText,
						SongType = tds[4].InnerText.Replace(" ", "")
					};
				});

		}
	}
}
