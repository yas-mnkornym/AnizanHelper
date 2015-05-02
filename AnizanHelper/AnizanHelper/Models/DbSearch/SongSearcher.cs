using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Web;

namespace AnizanHelper.Models.DbSearch
{
	public class SongSearcher : DbSeacherBase<SongSearchResult>
	{
		public static readonly string SearchType = "song";
		static readonly string BaseUrl = "http://anison.info/data/";

		override public IEnumerable<SongSearchResult> Search(string searchWord, CancellationToken cancellationToken = default(CancellationToken))
		{
			var uri = CreateQueryUrl(searchWord, SearchType);
			while (uri != null) {
				cancellationToken.ThrowIfCancellationRequested();
				var items = SearchPage(ref uri);
				foreach (var item in items) {
					yield return item;
				}
			}
		}

		IEnumerable<SongSearchResult> SearchPage(ref string uri)
		{
			var doc = QueryDocument(uri);
			var tbody = doc.DocumentNode.SelectSingleNode("/html[1]/body[1]/table[2]/tbody[1]");
			if (tbody == null) {
				return Enumerable.Empty<SongSearchResult>();
			}

			// 次のページを検索
			var seek = doc.DocumentNode.Descendants("a").FirstOrDefault(x => x.GetAttributeValue("class", null) == "seek" && x.InnerText == "次へ");
			if (seek == null) { uri = null; }
			else {
				uri = seek.GetAttributeValue("href", null);
				if (uri != null) {
					uri = BaseUrl + uri;
				}
			}

			return tbody
				.Descendants("tr")
				.Select(tr => {
					var tds = tr.Descendants("td").ToArray();
					var serisA = tds[3].Descendants("a").FirstOrDefault();
					return new SongSearchResult {
						Title = HttpUtility.HtmlDecode(tds[0].InnerText),
						Singers = tds[1].Descendants("a").Any() ?
							tds[1].Descendants("a").Select(x => HttpUtility.HtmlDecode(x.InnerText)).ToArray() :
							new string[] { tds[1].InnerText },
						Genre = HttpUtility.HtmlDecode(tds[2].InnerText.Replace(" ", "")),
						Series = HttpUtility.HtmlDecode(tds[3].InnerText),
						SeriesUrl = (serisA != null ? BaseUrl + serisA.GetAttributeValue("href", (string)null) : null),
						SongType = HttpUtility.HtmlDecode(tds[4].InnerText.Replace(" ", ""))
					};
				});

		}
	}
}
