using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
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

			await ZanmaiWikiSearchDataHelper.SerializeProgramsAsync(
				outputStream,
				programs,
				cancellationToken)
				.ConfigureAwait(false);
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

				foreach (var program in programs)
				{
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

				var songInfo = songInfoParser.Parse(line);
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
	}
}
