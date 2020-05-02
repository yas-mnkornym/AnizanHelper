using System;

namespace AnizanHelper.Models.DbSearch
{
	public class SearchManager : ISearchManager
	{
		public event EventHandler<string> SearchTriggered;

		public void TriggerSearch(string format)
		{
			if (format == null) { throw new ArgumentNullException(nameof(format)); }

			this.SearchTriggered?.Invoke(this, format);
		}
	}
}
