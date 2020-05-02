using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace AnizanHelper.Models.DbSearch
{
	public class SongSearcher : DbSeacherBase<SongSearchResult>
	{
		public static string SearchType { get; } = "song";
		static string BaseUrl { get; } = "http://anison.info/data/";
		static Regex LinkParserRegex { get; } = new Regex(@"javascript:link\('(?<Category>.*)','(?<Id>.*)'\)");

		public override async Task<IEnumerable<SongSearchResult>> SearchAsync(string searchWord, CancellationToken cancellationToken = default)
		{
			if (searchWord == null) { throw new ArgumentNullException(nameof(searchWord)); }

			var uri = CreateQueryUrl(searchWord, SearchType);
			var list = new List<SongSearchResult>();


			var limit = 20;
			for (var i = 0; i < limit && uri != null; i++)
			{
				cancellationToken.ThrowIfCancellationRequested();
				var items = await SearchPageAsync(uri, cancellationToken).ConfigureAwait(false);
				list.AddRange(items.Songs);
				uri = items.NextPageUri;
			}

			return list;
		}

		class SearchPageResult
		{
			public SongSearchResult[] Songs { get; set; }
			public string NextPageUri { get; set; }
		}

		async Task<SearchPageResult> SearchPageAsync(string uri, CancellationToken cancellationToken)
		{
			var doc = await QueryDocumentAsync(uri, cancellationToken).ConfigureAwait(false);
			var tbody = doc.DocumentNode.SelectSingleNode("/html[1]/body[1]/table[1]/tbody[1]");
			if (tbody == null)
			{
				return new SearchPageResult
				{
					Songs = Array.Empty<SongSearchResult>(),
					NextPageUri = null,
				};
			}

			var songs = tbody
				.Descendants("tr")
				.Select(tr => {
					var tds = tr.Descendants("td").ToArray();
					var serisA = tds[3].Descendants("a").FirstOrDefault();
					return new SongSearchResult {
						Title = WebUtility.HtmlDecode(tds[0].InnerText),
						Singers = tds[1].Descendants("a").Any() ?
							tds[1].Descendants("a").Select(x => WebUtility.HtmlDecode(x.InnerText)).ToArray() :
							new string[] { tds[1].InnerText },
						Genre = WebUtility.HtmlDecode(tds[2].InnerText.Replace(" ", "")),
						Series = WebUtility.HtmlDecode(tds[3].InnerText),
						SeriesUrl = (serisA != null ? GetActualLink(BaseUrl, serisA.GetAttributeValue("href", (string)null)) : null),
						SongType = WebUtility.HtmlDecode(tds[4].InnerText.Replace(" ", ""))
					};
				});


			// 次のページを検索
			var seek = doc.DocumentNode.Descendants("a").FirstOrDefault(x => x.GetAttributeValue("class", null) == "seek" && x.InnerText == "次へ");
			var nextPageHref = seek?.GetAttributeValue("href", null);
			var nextPageUri = nextPageHref == null
				? null
				: BaseUrl + nextPageHref;

			return new SearchPageResult
			{
				Songs = songs.ToArray(),
				NextPageUri = nextPageUri,
			};
		}

		string GetActualLink(string baseUrl, string path)
		{
			if (path == null) { throw new ArgumentNullException(nameof(path)); }

			if (path.StartsWith("javascript:link"))
			{
				var match = LinkParserRegex.Match(path);
				if (match.Success)
				{
					var category = match.Groups["Category"];
					var id = match.Groups["Id"];

					path = $"{category}/{id}.html";
				}
			}

			return baseUrl + path;
		}
	}
}
