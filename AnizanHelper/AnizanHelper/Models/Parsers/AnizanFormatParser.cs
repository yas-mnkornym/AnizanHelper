using System.Text.RegularExpressions;

namespace AnizanHelper.Models.Parsers
{
	internal class AnizanFormatParser : ISongInfoParser
	{
		static Regex[] Regexes = new Regex[]{
			// Special 
			new Regex(@"(?<SpecialHeader>[★▼])(?<SpecialItemName>.*)[「｢](?<Title>.*)[｣」]\s*[/／]?(?<Artist>.*)?[\(（](\[(?<Genre>.*)\])?(?<Series>.*)　(?<SongType>.*)?[\)）]\s*(※(?<Additional>.*))?"),
			
			// Special w/o SongType
			new Regex(@"(?<SpecialHeader>[★▼])(?<SpecialItemName>.*)[「｢](?<Title>.*)[｣」]\s*[/／]?(?<Artist>.*)?[\(（](\[(?<Genre>.*)\])?(?<Series>.*)[\)）]\s*(※(?<Additional>.*))?"),
			
			// Special Lack
			new Regex(@"(?<SpecialHeader>[★▼])(?<SpecialItemName>.*)[「｢](?<Title>.*)[｣」]\s*[/／]?(?<Artist>.*)?\s*(※(?<Additional>.*))?"),

			// Song
			new Regex(@"(?<Number>\d{1,4})?[\.．]?[「｢](?<Title>.*)[｣」]\s*[/／]?(?<Artist>.*)?[\(（](\[(?<Genre>.*)\])?(?<Series>.*)　(?<SongType>.*)?[\)）]\s*(※(?<Additional>.*))?"),

			// Song w/o SongType
			new Regex(@"(?<Number>\d{1,4})?[\.．]?[「｢](?<Title>.*)[｣」]\s*[/／]?(?<Artist>.*)?[\(（](\[(?<Genre>.*)\])?(?<Series>[^\)]*)[\)）]\s*(※(?<Additional>.*))?"),

			// Song Lack
			new Regex(@"(?<Number>\d{1,4})?[\.．]?[「｢](?<Title>.*)[｣」]\s*[/／]?(?<Artist>.*)?\s*(※(?<Additional>.*))?")
		};

		public GeneralSongInfo Parse(string inputText)
		{
			var anizanSongInfo = ParseAsAnizanInfo(inputText);
			if (anizanSongInfo == null) {
				return null;
			}
			else {
				return new GeneralSongInfo {
					Title = anizanSongInfo.Title,
					Singers = anizanSongInfo.Singer.Split(','),
					Genre = anizanSongInfo.Genre,
					Series = anizanSongInfo.Series,
					SongType = anizanSongInfo.SongType,
				};
			}
		}

		public AnizanSongInfo ParseAsAnizanInfo(string inputText)
		{
			Match match = null;
			foreach (var regex in Regexes) {
				match = regex.Match(inputText);
				if (match.Success) {
					break;
				}
			}

			return match?.Success == true
				? new AnizanSongInfo {
					Number = TryParseAsIntOrDefault(match.Groups["Number"]?.Value?.Trim()),
					Title = match.Groups["Title"]?.Value?.Trim(),
					Singer = match.Groups["Artist"]?.Value?.Trim(),
					Genre = match.Groups["Genre"]?.Value?.Trim(),
					Series = match.Groups["Series"]?.Value?.Trim(),
					SongType = match.Groups["SongType"]?.Value?.Trim(),
					Additional = match.Groups["Additional"]?.Value?.Trim(),
					SpecialItemName = match.Groups["SpecialItemName"]?.Value?.Trim(),
					SpecialHeader = match.Groups["SpecialHeader"]?.Value?.Trim(),
				}
				: null;
		}

		static int TryParseAsIntOrDefault(string text)
		{
			if (text == null) {
				return 0;
			}

			return int.TryParse(text, out var result) ? result : 0;
		}
	}
}
