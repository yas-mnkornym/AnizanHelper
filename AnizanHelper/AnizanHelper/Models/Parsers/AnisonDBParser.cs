using System;
using System.Linq;

namespace AnizanHelper.Models.Parsers
{

	internal class AnisonDBParser : ISongInfoParser
	{

		public AnisonDBParser()
		{ }

		public GeneralSongInfo Parse(string inputText)
		{
			// トークン分割
			var tokens = inputText
				.Replace("\r\n", "\n")
				.Split(new char[] { '\t' }, StringSplitOptions.RemoveEmptyEntries)
				.ToArray();

			// 歌手名とジャンルをパース
			var artistText = tokens.ElementAtOrDefault(1) ?? string.Empty;
			var artistTokens = artistText.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);

			string genre = "";
			var artists = artistTokens.ToList();
			if (artistTokens.Length > 1 && artistTokens.Last().Length == 2)
			{
				genre = artistTokens.Last();
				artists.RemoveAt(artists.Count - 1);
			}

			var songType = tokens
				.ElementAtOrDefault(3)
				?.Replace(" ", string.Empty)
				?? string.Empty;

			return new GeneralSongInfo
			{
				Title = tokens.ElementAtOrDefault(0) ?? string.Empty,
				Artists = artists.ToArray(),
				Genre = genre,
				Series = tokens.ElementAtOrDefault(2) ?? string.Empty,
				SongType = songType
			};
		}
	}
}
