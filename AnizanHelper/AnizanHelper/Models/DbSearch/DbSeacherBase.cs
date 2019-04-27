using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace AnizanHelper.Models.DbSearch
{
	public abstract class DbSeacherBase<TResult> : IDbSearcher<TResult>
	{
		string QueryUrlBase { get; } = "http://anison.info/data/n.php";
		static HttpClient HttpClient { get; } = new HttpClient();

		public string UserAgent { get; set; } = @"Mozilla/5.0 (Windows NT 6.3; Trident/7.0; rv:11.0) like Gecko";

		public DbSeacherBase()
		{ }

		public DbSeacherBase(string queryUrl)
		{
			QueryUrlBase = queryUrl ?? throw new ArgumentNullException(nameof(queryUrl));
		}

		public abstract Task<IEnumerable<SongSearchResult>> SearchAsync(string searchWord, CancellationToken cancellationToken = default);

		public string CreateQueryUrl(string word, string type)
		{
			if (word == null) { throw new ArgumentNullException(nameof(word)); }
			if (type == null) { throw new ArgumentNullException(nameof(type)); }

			return string.Format("{0}?q={1}&m={2}",
				QueryUrlBase,
				string.Join("+", word.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries)
					.Select(x => HttpUtility.UrlEncode(x))),
				HttpUtility.UrlEncode(type));
		}

		protected HtmlDocument QueryDocument(string word, string type)
		{
			var queryUrl = CreateQueryUrl(word, type);
			using (var client = new HttpClient()) {
				if (!string.IsNullOrWhiteSpace(UserAgent)) {
					client.DefaultRequestHeaders.Add("User-Agent", UserAgent);
				}

				using (var stream = client.GetStreamAsync(queryUrl).Result) {
					var doc = new HtmlDocument();
					doc.Load(stream, Encoding.UTF8);
					return doc;
				}
			}
		}

		protected async Task<HtmlDocument> QueryDocumentAsync(string queryUrl, CancellationToken cancellationToken = default)
		{
			if (!string.IsNullOrWhiteSpace(UserAgent))
			{
				HttpClient.DefaultRequestHeaders.Add("User-Agent", UserAgent);
			}

			using (var response = await HttpClient.GetAsync(queryUrl, cancellationToken).ConfigureAwait(false))
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
	}
}
