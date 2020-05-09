using AnizanHelper.Models;
using Prism.Events;

namespace AnizanHelper.ViewModels.Events
{
	internal class SongParsedEvent : PubSubEvent<ZanmaiSongInfo>
	{
	}

	internal class ClearSearchInputEvent : PubSubEvent
	{
	}

	internal class SearchSongEvent : PubSubEvent<SearchCondition>
	{
	}

	internal class SearchCondition
	{
		public string SearchTerm { get; set; }
		public string[] Artists { get; set; }
	}
}
