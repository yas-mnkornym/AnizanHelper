namespace AnizanHelper.Models.DbSearch
{
	public interface ISearchController
	{
		void ClearInput();
		void TriggerSearch(string searchTerm);
	}
}
