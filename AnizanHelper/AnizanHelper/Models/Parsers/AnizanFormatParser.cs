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

			// Song w/ note
			new Regex(@"(?<Number>\d{1,4})?[\.．]?[「｢](?<Title>.+)[｣」]\s*[/／]?(?<Artist>[^(]+)[\(（](\[(?<Genre>.*)\])\s*(?<Series>[^　]+)　(?<SongType>.+)[\)）]\s*※(?<Additional>.+)"),

			// Song
			new Regex(@"(?<Number>\d{1,4})?[\.．]?[「｢](?<Title>.+)[｣」]\s*[/／]?(?<Artist>[^(]+)[\(（](\[(?<Genre>.*)\])\s*(?<Series>[^　]+)　(?<SongType>.+)[\)）]"),

			// Song w/o Genre w/ note
			new Regex(@"(?<Number>\d{1,4})?[\.．]?[「｢](?<Title>.+)[｣」]\s*[/／]?(?<Artist>[^(]+)[\(（](?<Series>[^　]+)　(?<SongType>.+)[\)）]\s*※(?<Additional>.+)"),

			// Song w/o Genre
			new Regex(@"(?<Number>\d{1,4})?[\.．]?[「｢](?<Title>.+)[｣」][/／](?<Artist>[^(]+)[\(（](?<Series>[^　]+)　(?<SongType>[^　]+)[\)）]"),
			
			// Song w/o SongType w/ Genre and note
			new Regex(@"(?<Number>\d{1,4})?[\.．]?[「｢](?<Title>.+)[｣」][/／](?<Artist>[^(]+)[\(（](\[(?<Genre>.*)\])\s*(?<Series>[^　]+)[\)）]\s*※(?<Additional>.+)"),

			// Song w/o SongType w/ Genre
			new Regex(@"(?<Number>\d{1,4})?[\.．]?[「｢](?<Title>.+)[｣」][/／](?<Artist>[^(]+)[\(（](\[(?<Genre>.*)\])\s*(?<Series>[^　]+)[\)）]"),
			
			// Song w/o SongType w/ note
			new Regex(@"(?<Number>\d{1,4})?[\.．]?[「｢](?<Title>.+)[｣」][/／](?<Artist>[^(]+)[\(（](?<Series>[^　]+)[\)）]\s*※(?<Additional>.+)"),

			// Song w/o SongType
			new Regex(@"(?<Number>\d{1,4})?[\.．]?[「｢](?<Title>.+)[｣」][/／](?<Artist>[^(]+)[\(（](?<Series>[^　]+)[\)）]"),

			// Song w/o Series Info w/ note
			new Regex(@"(?<Number>\d{1,4})?[\.．]?[「｢](?<Title>.+)[｣」][/／](?<Artist>[^※]+)\s*※(?<Additional>.+)"),
			
			// Song w/o Series Info
			new Regex(@"(?<Number>\d{1,4})?[\.．]?[「｢](?<Title>.+)[｣」][/／](?<Artist>[^※]+)?"),
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
