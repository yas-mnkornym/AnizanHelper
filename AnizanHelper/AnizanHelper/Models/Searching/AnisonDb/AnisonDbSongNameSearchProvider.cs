using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace AnizanHelper.Models.Searching.AnisonDb
{
	public class AnisonDbSongNameSearchProvider : AnisonDbSongSearchProviderBase
	{
		public static int MaxPageNumberToRetreive { get; } = 20;
		public static Uri DatabaseBaseUri { get; } = new Uri("http://anison.info/data/");
		private static Regex LinkParserRegex { get; } = new Regex(@"javascript:link\('(?<Category>.*)','(?<Id>.*)'\)");
		private static Regex SeriesNumberPatternRegex { get; } = new Regex(@"(?<Type>(OP|ED))\s*(?<Number>\d)");
		public static string ShortProviderIdentifier { get; } = "DB";

		private AnizanSongInfoProcessor SongInfoConverter { get; }

		public bool CheckSeries { get; set; }

		public AnisonDbSongNameSearchProvider(
			HttpClient httpClient,
			AnizanSongInfoProcessor songInfoConverter) : base(httpClient)
		{
			this.SongInfoConverter = songInfoConverter ?? throw new ArgumentNullException(nameof(songInfoConverter));
		}

		public override async Task<ZanmaiSongInfo> ConvertToZanmaiSongInfoAsync(
			ISongSearchResult songSearchResult,
			CancellationToken cancellationToken = default)
		{
			if (songSearchResult is AnisonDbNameSongSearchResult result)
			{
				var info = new ZanmaiSongInfo
				{
					Title = result.Title,
					Series = result.Series,
					Genre = result.Genre,
					Artists = result.Artists,
					SongType = this.CheckSeries ? await this.FindActualSongTypeAsync(result, cancellationToken).ConfigureAwait(false) : result.SongType
				};

				return this.SongInfoConverter.Convert(info);
			}
			else
			{
				throw new ArgumentException("unsupported result type.", nameof(songSearchResult));
			}
		}

		private async Task<string> FindActualSongTypeAsync(AnisonDbNameSongSearchResult result, CancellationToken cancellationToken)
		{
			if (result.SeriesUrl == null) { return result.SongType; }

			var match = SeriesNumberPatternRegex.Match(result.SongType);
			if (match == null) { return result.SongType; }

			if (string.IsNullOrWhiteSpace(match.Groups["Number"].Value)) { return result.SongType; }
			if (Convert.ToInt32(match.Groups["Number"].Value) > 1) { return result.SongType; }

			using (var res = await this.HttpClient.GetAsync(result.SeriesUrl, cancellationToken).ConfigureAwait(false))
			{
				res.EnsureSuccessStatusCode();

				using (var stream = await res.Content.ReadAsStreamAsync().ConfigureAwait(false))
				{
					var doc = new HtmlDocument();
					doc.Load(stream, Encoding.UTF8);

					var tbody = doc.DocumentNode.SelectSingleNode(@"//table[contains(.,""OP/ED"")]/tbody");
					if (tbody == null)
					{
						return result.SongType;
					}

					var isActuallyNumbered = tbody.Descendants("tr")
						.Where(tr =>
						{
							var td1 = tr.Descendants("td").FirstOrDefault();
							if (td1 == null) { return false; }

							var dbMatch = SeriesNumberPatternRegex.Match(td1.InnerText);
							if (dbMatch == null) { return false; }

							if (match.Groups["Type"].Value != dbMatch.Groups["Type"].Value) { return false; }
							if (string.IsNullOrWhiteSpace(dbMatch.Groups["Number"].Value)) { return false; }

							var number = Convert.ToInt32(dbMatch.Groups["Number"].Value);
							if (number > 1) { return true; }

							return false;
						})
						.Any();

					if (isActuallyNumbered)
					{
						return result.SongType;
					}
					else
					{
						return match.Groups["Type"].Value;
					}
				}
			}
		}

		public override async IAsyncEnumerable<ISongSearchResult> SearchAsync(
			string searchTerm,
			Dictionary<string, string> options = null,
			[EnumeratorCancellation] CancellationToken cancellationToken = default)
		{
			if (searchTerm == null) { throw new ArgumentNullException(nameof(searchTerm)); }

			var uri = this.CreateQueryUri(searchTerm, "song");

			for (var i = 0; i < MaxPageNumberToRetreive && uri != null; i++)
			{
				var item = await this.SearchPageAsync(searchTerm, uri, cancellationToken).ConfigureAwait(false);
				uri = item.NextPageUri;

				foreach (var song in item.Songs)
				{
					yield return song;
				}
			}
		}

		private async Task<SearchPageResult> SearchPageAsync(string searchTerm, Uri uri, CancellationToken cancellationToken)
		{
			var doc = await this.QueryDocumentAsync(uri, cancellationToken).ConfigureAwait(false);
			var tbody = doc.DocumentNode.SelectSingleNode(@"//table[contains(.,""曲名"")]/tbody");
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
				.Select(tr =>
				{
					var tds = tr.Descendants("td").ToArray();
					var serisA = tds[3].Descendants("a").FirstOrDefault();
					var title = WebUtility.HtmlDecode(tds[0].InnerText);
					return new AnisonDbNameSongSearchResult
					{
						ShortProviderIdentifier = ShortProviderIdentifier,
						ProviderId = this.Id,
						Title = title,
						Artists = tds[1].Descendants("a").Any() ?
							tds[1].Descendants("a").Select(x => WebUtility.HtmlDecode(x.InnerText)).ToArray() :
							new string[] { tds[1].InnerText },
						Genre = WebUtility.HtmlDecode(tds[2].InnerText.Replace(" ", "")),
						Series = WebUtility.HtmlDecode(tds[3].InnerText),
						SeriesUrl = (serisA != null ? this.GetSeriesUri(serisA.GetAttributeValue("href", (string)null)) : null),
						SongType = WebUtility.HtmlDecode(tds[4].InnerText.Replace(" ", "")),
						Score = string.Compare(title, searchTerm, CultureInfo.InvariantCulture, CompareOptions.IgnoreCase | CompareOptions.IgnoreKanaType | CompareOptions.IgnoreNonSpace | CompareOptions.IgnoreSymbols| CompareOptions.IgnoreWidth) == 0
						? 4
						: 3,
					};
				});

			// 次のページを検索
			var seek = doc.DocumentNode.Descendants("a").FirstOrDefault(x => x.GetAttributeValue("class", null) == "seek" && x.InnerText == "次へ");
			var nextPageHref = seek?.GetAttributeValue("href", null);

			var nextPageUri = nextPageHref == null
				? null
				: new Uri(DatabaseBaseUri.AbsoluteUri + nextPageHref);

			return new SearchPageResult
			{
				Songs = songs.ToArray(),
				NextPageUri = nextPageUri,
			};
		}

		private Uri GetSeriesUri(string path)
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

			var ub = new UriBuilder(DatabaseBaseUri);

			ub.Path = string.IsNullOrWhiteSpace(ub.Path)
				? path
				: ub.Path.TrimEnd('/') + "/" + path;

			return ub.Uri;
		}

		private class SearchPageResult
		{
			public SongSearchResult[] Songs { get; set; }
			public Uri NextPageUri { get; set; }
		}
	}
}
