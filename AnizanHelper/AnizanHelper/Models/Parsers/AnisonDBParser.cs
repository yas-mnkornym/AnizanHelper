using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnizanHelper.Models.Parsers
{
	internal class AnisonDBParser : ISongInfoParser
	{
		public IEnumerable<ReplaceInfo> Replaces { get; set; }

		public AnisonDBParser(params ReplaceInfo[] replaces)
		{
			Replaces = replaces;
		}

		public SongInfo Parse(string inputText)
		{
			var tokens = inputText.Replace("\r\n", "\n").Split(new char[] { '\t' }, StringSplitOptions.RemoveEmptyEntries)
				.Select(x => {
					var str = x;
					if (Replaces != null) {
						foreach (var rep in Replaces) {
							if (rep.Exact) {
								if (str == rep.Original) {
									str = rep.Replaced;
								}
							}
							else {
								str = str.Replace(rep.Original, rep.Replaced);
							}
						}
					}
					return str;
				}).ToArray();

			var singerStr = (tokens.Length > 1 ? tokens[1] : "");
			var stokens = singerStr.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
			string singer = "";
			string genre = "";
			if (stokens.Length > 1 && stokens.Last().Length == 2) {
				genre = stokens.Last();
				bool first = true;
				StringBuilder sb = new StringBuilder();
				for (int i = 0; i < stokens.Length - 1; i++) {
					if (first) { first = false; }
					else { sb.Append(","); }
					sb.Append(stokens[i]);
				}
				singer = sb.ToString();
			}
			else {
				bool first = true;
				StringBuilder sb = new StringBuilder();
				for (int i = 0; i < stokens.Length; i++) {
					if (first) { first = false; }
					else { sb.Append(","); }
					sb.Append(stokens[i]);
				}
				singer = sb.ToString();
			}

			var songType = (tokens.Length > 3 ? tokens[3] : "");
			if(songType != null){ songType = songType.Replace(" ", ""); }

			SongInfo info = new SongInfo {
				Title = (tokens.Length > 0 ? tokens[0] : ""),
				Singer = singer,
				Series = (tokens.Length > 2 ? tokens[2] : ""),
				SongType = songType
			};

			// ジャンルを置換
			if (genre != null) {
				switch (genre) {
					case "GM":
						info.Genre = "GM";
						break;

					case "RD":
					case "WR":
						info.Genre = "RD";
						break;

					case "DK":
						info.Genre = "同人";
						break;

					case "KK":
						info.Genre = "企画";
						break;

					case "SF":
					case "SS":
					case "SV":
					case "SM":
					case "WS":
					case "NG":
						info.Genre = "特撮";
						break;

					case "TV":
					case "TS":
					case "VD":
					case "MV":
					case "WA":
						info.Genre = null;
						break;

					default:
						info.Genre = null;
						info.IsNotAnison = true;
						break;
				}
			}

			if (songType != null) {
				switch (songType) {
					case "KK":
						info.Genre = "企画";
						info.SongType = null;
						break;
				}
			}


			return info;
		}
	}
}
