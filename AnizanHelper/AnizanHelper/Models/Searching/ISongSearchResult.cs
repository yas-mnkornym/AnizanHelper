namespace AnizanHelper.Models.Searching
{
	public interface ISongSearchResult
	{
		string[] Artists { get; }
		string Genre { get; }
		string ProviderId { get; }
		string Series { get; }
		string SongType { get; }
		string Title { get; }
	}
}
