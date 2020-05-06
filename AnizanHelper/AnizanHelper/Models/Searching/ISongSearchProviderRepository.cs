using System.Collections.Generic;

namespace AnizanHelper.Models.Searching
{
	public interface ISongSearchProviderRepository
	{
		public IEnumerable<ISongSearchProvider> GetProviders();
	}
}
