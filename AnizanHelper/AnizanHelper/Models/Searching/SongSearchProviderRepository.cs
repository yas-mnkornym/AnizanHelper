using System;
using System.Collections.Generic;
using System.Linq;

namespace AnizanHelper.Models.Searching
{
	// TODO: Support dynamic adding/removing of the providers
	public class SongSearchProviderRepository : ISongSearchProviderRepository
	{
		private Dictionary<string, ISongSearchProvider> Providers { get; }

		public SongSearchProviderRepository(params ISongSearchProvider[] providers)
		{
			this.Providers = providers
				?.ToDictionary(x => x.Id, x => x)
				?? throw new ArgumentNullException(nameof(providers));
		}

		public IEnumerable<ISongSearchProvider> GetProviders()
		{
			return this.Providers.Values;
		}

		public bool TryGetProvider(string id, out ISongSearchProvider provider)
		{
			return this.Providers.TryGetValue(id, out provider);
		}
	}
}
