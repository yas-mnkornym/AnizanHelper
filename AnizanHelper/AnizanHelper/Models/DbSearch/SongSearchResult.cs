using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using HtmlAgilityPack;

namespace AnizanHelper.Models.DbSearch
{
	public class SongSearchResult
	{
		public string Title { get; set; }

		public IEnumerable<string> Singers { get; set; }

		public string Genre { get; set; }

		public string Series { get; set; }

		public string SongType { get; set; }

		public string SeriesUrl { get; set; }

		public override string ToString()
		{
			return Utils.GeneralObjectToString(this);
		}


		public static GeneralSongInfo ToGeneralInfo(SongSearchResult info, bool checkSeries = true)
		{
			if (info == null) { throw new ArgumentNullException("info"); }

			return new GeneralSongInfo
			{
				Title = info.Title,
				Series = info.Series,
				Genre = info.Genre,
				Singers = info.Singers,
				SongType = checkSeries ? FindActualSongType(info) : info.SongType
			};
		}

		private static string patternNumber = @"(?<Type>(OP|ED))\s*(?<Number>\d)";
		private static string userAgent_ = @"Mozilla/5.0 (Windows NT 6.3; Trident/7.0; rv:11.0) like Gecko";

		private static string FindActualSongType(SongSearchResult info)
		{
			if (string.IsNullOrWhiteSpace(info.SeriesUrl)) { return info.SongType; }

			var match = Regex.Match(info.SongType, patternNumber);
			if (match == null) { return info.SongType; }
			if (string.IsNullOrWhiteSpace(match.Groups["Number"].Value)) { return info.SongType; }
			if (Convert.ToInt32(match.Groups["Number"].Value) > 1) { return info.SongType; }

			using (var client = new HttpClient())
			{
				if (!string.IsNullOrWhiteSpace(userAgent_))
				{
					client.DefaultRequestHeaders.Add("User-Agent", userAgent_);
				}

				using (var stream = client.GetStreamAsync(info.SeriesUrl).Result)
				{
					var doc = new HtmlDocument();
					doc.Load(stream, Encoding.UTF8);

					var tbody = doc.DocumentNode.SelectSingleNode("/html[1]/body[1]/div[2]/table[2]/tbody[1]");
					if (tbody == null)
					{
						return info.SongType;
					}
					var isActuallyNumbered = tbody.Descendants("tr")
						.Where(tr =>
						{
							var td1 = tr.Descendants("td").FirstOrDefault();
							if (td1 == null) { return false; }
							var dbMatch = Regex.Match(td1.InnerText, patternNumber);
							if (dbMatch == null) { return false; }
							if (match.Groups["Type"].Value != dbMatch.Groups["Type"].Value) { return false; }
							if (string.IsNullOrWhiteSpace(dbMatch.Groups["Number"].Value)) { return false; }
							var number = Convert.ToInt32(dbMatch.Groups["Number"].Value);
							if (number > 1) { return true; }
							return false;
						}).Any();

					if (isActuallyNumbered) { return info.SongType; }
					else { return match.Groups["Type"].Value; }
				}
			}
		}
	}
}
