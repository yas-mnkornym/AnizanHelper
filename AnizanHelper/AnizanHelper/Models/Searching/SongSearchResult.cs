namespace AnizanHelper.Models.Searching
{
	public class SongSearchResult : ISongSearchResult
	{
		public string ProviderId { get; set; }

		public string Title { get; set; }
		public string[] Artists { get; set; }
		public string Genre { get; set; }
		public string Series { get; set; }
		public string SongType { get; set; }
	}
}
