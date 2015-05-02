using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AnizanHelper.Models.DbSearch
{
	public class SongSearcher : DbSeacherBase<SongSearchResult>
	{
		public static readonly string SearchType = "song";
		static readonly string SeriesBaseUrl = "http://anison.info/data/";

		override public IEnumerable<SongSearchResult> Search(string searchWord)
		{
			var doc = QueryDocument(searchWord, SearchType);

			var tbody = doc.DocumentNode.SelectSingleNode("/html[1]/body[1]/table[2]/tbody[1]");
			if (tbody == null) {
				return Enumerable.Empty<SongSearchResult>();
			}
			return tbody
				.Descendants("tr")
				.Select(tr => {
					var tds = tr.Descendants("td").ToArray();
					var serisA = tds[3].Descendants("a").FirstOrDefault();
					return new SongSearchResult {
						Title = tds[0].InnerText,
						Singers = tds[1].Descendants("a").Any() ?
							tds[1].Descendants("a").Select(x => x.InnerText).ToArray() : 
							new string[] { tds[1].InnerText },
						Genre = tds[2].InnerText.Replace(" ", ""),
						Series = tds[3].InnerText,
						SeriesUrl = (serisA != null ? SeriesBaseUrl + serisA.GetAttributeValue("href", (string)null) : null),
						SongType = tds[4].InnerText.Replace(" ", "")
					};
				});

		}
	}
}
