using System;
using System.Collections.Generic;

namespace AnizanHelper.Models.SongList
{
	public interface ISongMetadata : IReadOnlyDictionary<string, string>
	{
		DateTimeOffset Timestamp { get; }
		string StreamTitle { get; }
	}
}
