using AnizanHelper.Models;
using Prism.Events;

namespace AnizanHelper.ViewModels.Events
{
	internal class SongParsedEvent : PubSubEvent<GeneralSongInfo>
	{
	}

	internal class ClearSearchInputEvent : PubSubEvent
	{
	}
}
