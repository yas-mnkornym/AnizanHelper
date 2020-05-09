using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AnizanHelper.Models.Searching
{
	public interface ISongSearchProvider
	{
		string Id { get; }

		Task<ZanmaiSongInfo> ConvertToZanmaiSongInfoAsync(
			ISongSearchResult songSearchResult,
			CancellationToken cancellationToken = default);

		IAsyncEnumerable<ISongSearchResult> SearchAsync(
			string searchTerm,
			Dictionary<string, string> options = null,
			CancellationToken cancellationToken = default);
	}
}
