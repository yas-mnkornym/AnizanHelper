using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace AnizanHelper.Models.Searching.Zanmai
{
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
}
