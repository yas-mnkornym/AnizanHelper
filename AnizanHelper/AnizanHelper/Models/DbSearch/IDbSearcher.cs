using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AnizanHelper.Models.DbSearch
{
	public interface IDbSearcher<TResult>
	{
		Task<IEnumerable<SongSearchResult>> SearchAsync(string searchWord, CancellationToken cancellationToken = default);
	}
}
