using System;
using System.Text.RegularExpressions;

namespace AnizanHelper.Models.Parsers
{
	internal class AnizanFormatParser : ISongInfoParser
	{
		private static Regex[] Regexes = new Regex[]{
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

		public ZanmaiSongInfo Parse(string inputText)
		{
			Match match = null;
			foreach (var regex in Regexes)
			{
				match = regex.Match(inputText);
				if (match.Success)
				{
					break;
				}
			}

			var specialItemHeader = match.Groups["SpecialHeader"]?.Value?.Trim();
			return match?.Success == true
				? new ZanmaiSongInfo
				{
					Number = TryParseAsIntOrDefault(match.Groups["Number"]?.Value?.Trim()),
					Title = match.Groups["Title"]?.Value?.Trim(),
					Artists = match.Groups["Artist"]?.Value?.Trim()?.Split(',') ?? Array.Empty<string>(),
					Genre = match.Groups["Genre"]?.Value?.Trim(),
					Series = match.Groups["Series"]?.Value?.Trim(),
					SongType = match.Groups["SongType"]?.Value?.Trim(),
					Additional = match.Groups["Additional"]?.Value?.Trim(),
					SpecialItemName = match.Groups["SpecialItemName"]?.Value?.Trim(),
					SpecialHeader = specialItemHeader,
					IsSpecialItem = !string.IsNullOrWhiteSpace(specialItemHeader),
				}
				: null;
		}

		private static int TryParseAsIntOrDefault(string text)
		{
			if (text == null)
			{
				return 0;
			}

			return int.TryParse(text, out var result) ? result : 0;
		}
	}
}
