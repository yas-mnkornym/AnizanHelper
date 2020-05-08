using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using AnizanHelper.Models.Parsers;
using HtmlAgilityPack;

namespace AnizanHelper.Models.Searching.Zanmai
{
	public class ZanmaiWikiSearchIndexGenerator
	{
		public static Uri WikiUrl { get; } = new Uri("https://w.atwiki.jp/anizan_portal");
		public static string ListPagePath { get; } = "list";
		public Regex RegexListPageTitle { get; } = new Regex(@"(?<Year>.*)/放送曲リスト/(?<Date>.*)");

		private HttpClient HttpClient { get; }
		private HtmlHelper HtmlHelper { get; }

		public ZanmaiWikiSearchIndexGenerator(HttpClient httpClient)
		{
			this.HttpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
			this.HtmlHelper = new HtmlHelper(this.HttpClient, WikiUrl);
		}

		public async Task CreateIndexAsync(
			Stream outputStream,
			CancellationToken cancellationToken)
		{
			var programs = await this.RetrieveSongEntries(cancellationToken)
				.ToArrayAsync()
				.ConfigureAwait(false);

			using (var writer = new StreamWriter(outputStream))
			{
				var programId = 0;
				foreach (var program in programs)
				{
					await this.WriteEntriesAsync(
						writer,
						"Program",
						programId,
						program.ProgramTitle,
						program.Songs.Length,
						program.SongListPageLink.Year,
						program.SongListPageLink.Date,
						program.SongListPageLink.Title,
						program.SongListPageLink.Uri.AbsoluteUri)
						.ConfigureAwait(false);

					foreach (var song in program.Songs.Select(x => x.SongInfo))
					{
						await this.WriteEntriesAsync(
							writer,
							"Song",
							programId,
							song.Number,
							song.Title,
							song.Artist,
							song.Genre,
							song.Series,
							song.SongType,
							song.IsSpecialItem,
							song.SpecialHeader,
							song.SpecialItemName,
							song.Additional,
							song.ShortDescription)
							.ConfigureAwait(false);
					}

					programId++;
				}

				await writer.FlushAsync().ConfigureAwait(false);
			}
		}


		private Task WriteEntriesAsync(
			TextWriter writer,
			string recordName,
			params object[] values)
		{
			var line = string.Join(
				"\t",
				new string[] { recordName }
					.Concat(values.Select(x => x?.ToString() ?? string.Empty))
					.Select(x => x.Replace("\\", "\\\\").Replace("\t", "\\\t")));

			return writer.WriteLineAsync(line);
		}


		private async IAsyncEnumerable<ZanmaiProgram> RetrieveSongEntries([EnumeratorCancellation]CancellationToken cancellationToken = default)
		{
			await foreach (var page in this.GetListPagesAsync(cancellationToken).ConfigureAwait(false))
			{
				var doc = await this.HtmlHelper.GetDocumentAsync(page.Uri, cancellationToken).ConfigureAwait(false);

				var programs = this.ParseListPage(doc)
					.GroupBy(x => x.ProgramTitle)
					.Select(x => new ZanmaiProgram
					{
						SongListPageLink = page,
						ProgramTitle = x.Key,
						Songs = x.ToArray(),
					});

				foreach (var program in programs) {
					yield return program;
				}
			}
		}


		private IEnumerable<ZanmaiSongListItem> ParseListPage(HtmlDocument doc)
		{
			var lines = doc.DocumentNode
				.SelectSingleNode(@"//*[@id=""wikibody""]")
				.InnerText
				.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
				.Select(x => WebUtility.HtmlDecode(x.Trim()))
				.ToArray();

			var regexProgramTitle = new Regex(@"第(?<Number>\d+)部.*\[.+\]");
			var songInfoParser = new AnizanFormatParser();

			string currentProgram = null;
			foreach (var line in lines)
			{
				var programRegexMatch = regexProgramTitle.Match(line);
				if (programRegexMatch.Success)
				{
					currentProgram = line;
					continue;
				}

				var songInfo = songInfoParser.ParseAsAnizanInfo(line);
				if (songInfo != null)
				{
					yield return new ZanmaiSongListItem
					{
						ProgramTitle = currentProgram,
						SongInfo = songInfo,
					};
				}
			}
		}

		private async IAsyncEnumerable<WikiSongListPageLink> GetListPages(HtmlDocument doc, [EnumeratorCancellation]CancellationToken cancellationToken)
		{
			var linkNodes = doc.DocumentNode.SelectNodes(@"//table[contains(.,""ページ名"")]//tr/td/a");
			foreach (var node in linkNodes)
			{
				var title = node.InnerText.Trim();
				var url = node.GetAttributeValue<string>("href", null);

				if (title == null || url == null)
				{
					continue;
				}

				if (!title.Contains("放送曲リスト"))
				{
					continue;
				}

				var match = this.RegexListPageTitle.Match(title);
				if (!match.Success)
				{
					continue;
				}

				var year = match.Groups["Year"].Value;
				var date = match.Groups["Date"].Value;
				if (string.IsNullOrWhiteSpace(year) || string.IsNullOrWhiteSpace(date))
				{
					continue;
				}

				yield return new WikiSongListPageLink
				{
					Year = year,
					Date = date,
					Title = title,
					Uri = url.StartsWith("//")
						? new Uri(WikiUrl.Scheme + ":" + url)
						: url.StartsWith("/")
							? new Uri(WikiUrl.AbsoluteUri + url)
							: new Uri(url),
				};
			}

			var nextPageLinkNode = doc.DocumentNode.SelectSingleNode(@"//*[@id=""wikibody""]//ul/li[@class=""next""]/span/a");
			if (nextPageLinkNode != null)
			{
				var link = nextPageLinkNode.GetAttributeValue<string>("href", null);
				if (!string.IsNullOrWhiteSpace(link))
				{
					var tokens = link.Split('?');
					var path = tokens.ElementAtOrDefault(0)?.Trim();
					if (string.IsNullOrWhiteSpace(path))
					{
						path = ListPagePath;
					}

					var nextPageDoc = await this.HtmlHelper.GetDocumentAsync(
						path,
						tokens.ElementAtOrDefault(1)?.Trim(),
						cancellationToken);

					await Task.Delay(TimeSpan.FromMilliseconds(300)).ConfigureAwait(false);
					await foreach (var item in this.GetListPages(nextPageDoc, cancellationToken).ConfigureAwait(false))
					{
						yield return item;
					}
				}
			}
		}

		private async IAsyncEnumerable<WikiSongListPageLink> GetListPagesAsync([EnumeratorCancellation]CancellationToken cancellationToken)
		{
			var doc = await this.HtmlHelper.GetDocumentAsync(ListPagePath, cancellationToken).ConfigureAwait(false);

			await foreach (var item in this.GetListPages(doc, cancellationToken).ConfigureAwait(false))
			{
				yield return item;
			}
		}

		#region Internal classes

		private class WikiSongListPageLink
		{
			public string Year { get; set; }
			public string Date { get; set; }
			public string Title { get; set; }
			public Uri Uri { get; set; }
		}
		private class ZanmaiSongListItem
		{
			public string ProgramTitle { get; set; }
			public AnizanSongInfo SongInfo { get; set; }
		}

		private class ZanmaiProgram 
		{
			public WikiSongListPageLink SongListPageLink { get; set; }
			public string ProgramTitle { get; set; }
			public ZanmaiSongListItem[] Songs { get; set; }
		}

		#endregion Internal classes
	}

	public class HtmlHelper
	{
		public HtmlHelper(HttpClient httpClient, Uri baseUri)
		{
			this.HttpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
			this.BaseUri = baseUri ?? throw new ArgumentNullException(nameof(baseUri));
		}

		public Uri BaseUri { get; }
		public HttpClient HttpClient { get; }

		public async Task<HtmlDocument> GetDocumentAsync(
			Uri uri,
			CancellationToken cancellationToken = default)
		{
			using (var res = await this.HttpClient.GetAsync(uri, cancellationToken).ConfigureAwait(false))
			{
				res.EnsureSuccessStatusCode();
				var text = await res.Content.ReadAsStringAsync().ConfigureAwait(false);
				var doc = new HtmlDocument();
				doc.LoadHtml(text);
				return doc;
				//using (var stream = await res.Content.ReadAsStreamAsync().ConfigureAwait(false))
				//{
				//	var doc = new HtmlDocument();
				//	doc.Load(stream);

				//	return doc;
				//}
			}
		}

		public Task<HtmlDocument> GetDocumentAsync(
			string path,
			CancellationToken cancellationToken = default)
		{
			return this.GetDocumentAsync(path, null, cancellationToken);
		}

		public Task<HtmlDocument> GetDocumentAsync(
			string path,
			string query = null,
			CancellationToken cancellationToken = default)
		{
			if (path == null) { throw new ArgumentNullException(nameof(path)); }

			var ub = new UriBuilder(this.BaseUri);
			ub.Path = string.IsNullOrWhiteSpace(ub.Path)
				? path
				: ub.Path.TrimEnd('/') + "/" + path.TrimStart('/');

			if (!string.IsNullOrEmpty(query))
			{
				ub.Query = query;
			}

			return this.GetDocumentAsync(ub.Uri, cancellationToken);
		}
	}

	public class ZanmaiWikiSearchProvider
	{
	}
}
