using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace AnizanHelper.Models.DbSearch
{
	public interface IDbSearcher<TResult>
	{
		IEnumerable<TResult> Search(string searchWord, CancellationToken cancellationToken = default(CancellationToken));
	}
}
