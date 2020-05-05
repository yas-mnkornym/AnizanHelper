using System;

namespace AnizanHelper.Models.DbSearch
{
	public class ProxySearchController : ISearchController
	{
		public ISearchController Target { get; set; }

		public void TriggerSearch(string searchTerm)
		{
			this.Target?.TriggerSearch(searchTerm);
		}

		public void ClearInput()
		{
			this.Target?.ClearInput();
		}
	}
}
