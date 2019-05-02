using System;
using System.Threading;
using System.Threading.Tasks;

namespace AnizanHelper.Models.SongList
{
	public interface ISongMetadataRetreiver
	{
		Task<SongListFeedStopStatus> RunAsync(CancellationToken cancellationToken = default);

		event EventHandler<ISongMetadata> SongMetadataReceived;
	}
}
