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
			var singerStr = (tokens.Length > 1 ? tokens[1] : "");
			var stokens = singerStr.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
			string genre = "";
			var singers = stokens.ToList();
			if (stokens.Length > 1 && stokens.Last().Length == 2)
			{
				genre = stokens.Last();
				singers.RemoveAt(singers.Count - 1);
			}
			var songType = (tokens.Length > 3 ? tokens[3] : "");
			if (songType != null) { songType = songType.Replace(" ", ""); }

			return new GeneralSongInfo
			{
				Title = (tokens.Length > 0 ? tokens[0] : ""),
				Singers = singers,
				Genre = genre,
				Series = (tokens.Length > 2 ? tokens[2] : ""),
				SongType = songType
			};
		}
	}
}
