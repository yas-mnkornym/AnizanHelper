using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace AnizanHelper.Models.Searching.AnisonDb
{
	public abstract class AnisonDbSongSearchProviderBase : ISongSearchProvider
	{
		public static Uri SearchUri { get; } = new Uri("http://anison.info/data/n.php");

		protected HttpClient HttpClient { get; }

		public abstract string Id { get; }

		public object QueryUrlBase { get; private set; }

		public AnisonDbSongSearchProviderBase(HttpClient httpClient)
		{
			this.HttpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
		}

		protected async Task<HtmlDocument> QueryDocumentAsync(Uri queryUrl, CancellationToken cancellationToken = default)
		{
			using (var response = await this.HttpClient.GetAsync(queryUrl, cancellationToken).ConfigureAwait(false))
			{
				response.EnsureSuccessStatusCode();

				using (var stream = await response.Content.ReadAsStreamAsync())
				{
					var doc = new HtmlDocument();
					doc.Load(stream, Encoding.UTF8);
					return doc;
				}
			}
		}

		public Uri CreateQueryUri(string searchTerm, string type)
		{
			if (searchTerm == null) { throw new ArgumentNullException(nameof(searchTerm)); }
			if (type == null) { throw new ArgumentNullException(nameof(type)); }

			var uriBuilder = new UriBuilder(SearchUri);
			uriBuilder.Query =
				string.Format(
					"q={0}&m={1}",
					string.Join(
						"+",
						searchTerm
							.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries)
							.Select(x => WebUtility.UrlEncode(x))),
					WebUtility.UrlEncode(type));

			return uriBuilder.Uri;
		}

		public abstract IAsyncEnumerable<ISongSearchResult> SearchAsync(string searchTerm, Dictionary<string, string> options = null, CancellationToken cancellationToken = default);

		public abstract Task<ZanmaiSongInfo> ConvertToZanmaiSongInfoAsync(ISongSearchResult songSearchResult, CancellationToken cancellationToken = default);
	}
}
