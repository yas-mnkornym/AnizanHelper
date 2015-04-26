using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AnizanHelper.Models.DbSearch
{
	public interface IDbSearcher<TResult>
	{
		IEnumerable<TResult> Search(string searchWord);
	}
}
