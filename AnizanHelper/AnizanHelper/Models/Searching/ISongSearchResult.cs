namespace AnizanHelper.Models.Searching
{
	public interface ISongSearchResult
	{
		string ProviderId { get; }
		string ShortProviderIdentifier { get; }

		string[] Artists { get; }
		string Genre { get; }
		string Series { get; }
		string SongType { get; }
		string Title { get; }
	}
}
