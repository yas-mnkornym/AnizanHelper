using System;
using System.Collections.Generic;

namespace AnizanHelper.Models.SongList
{
	public interface ISongMetadata : IReadOnlyDictionary<string, string>
	{
		Guid Id { get; }
		DateTimeOffset Timestamp { get; }
		string StreamTitle { get; }
	}
}
