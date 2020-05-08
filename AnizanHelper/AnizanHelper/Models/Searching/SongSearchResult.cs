using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace AnizanHelper.Models.Searching
{
	public class SongSearchResult : ISongSearchResult
	{
		public string ProviderId { get; set; }
		public string ShortProviderIdentifier { get; set; }

		public int Score { get; set; }
		public string Title { get; set; }
		public string[] Artists { get; set; }
		public string Genre { get; set; }
		public string Series { get; set; }
		public string SongType { get; set; }
		public string Note { get; set; }
	}

	public class CompositeSearchProvider : ISongSearchProvider
	{
		public string Id => nameof(CompositeSearchProvider);
		private Dictionary<string, ISongSearchProvider> Providers { get; }

		public CompositeSearchProvider(params ISongSearchProvider[] providers)
		{
			this.Providers = providers
				?.ToDictionary(x => x.Id, x => x)
				?? throw new ArgumentNullException(nameof(providers));
		}

		public Task<GeneralSongInfo> ConvertToGeneralSongInfoAsync(ISongSearchResult songSearchResult, CancellationToken cancellationToken = default)
		{
			if (songSearchResult == null) { throw new ArgumentNullException(nameof(songSearchResult)); }

			return this.Providers.TryGetValue(songSearchResult.ProviderId, out var provider)
				? provider.ConvertToGeneralSongInfoAsync(songSearchResult, cancellationToken)
				: throw new ArgumentException("Unsupported provider ID.", nameof(songSearchResult));
		}

		public async IAsyncEnumerable<ISongSearchResult> SearchAsync(string searchTerm, Dictionary<string, string> options = null, [EnumeratorCancellation]CancellationToken cancellationToken = default)
		{
			var tasks = this.Providers.Values
				.Select(x => x.SearchAsync(searchTerm, options, cancellationToken).ToArrayAsync().AsTask())
				.ToArray();


			var results = await Task.WhenAll(tasks).ConfigureAwait(false);

			foreach (var item in results.SelectMany(x => x))
			{
				yield return item;
			}
		}
	}
}
