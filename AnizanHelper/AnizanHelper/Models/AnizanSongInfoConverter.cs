using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AnizanHelper.Models
{
	internal class AnizanSongInfoConverter
	{
		public ReplaceInfo[] Replaces { get; set; }

		public AnizanSongInfoConverter(params ReplaceInfo[] replaces)
		{
			Replaces = replaces;
		}

		public AnizanSongInfo Convert(GeneralSongInfo gInfo)
		{
			var replacedTitle = Replace(gInfo.Title);
			var result = new AnizanSongInfo {
				Title = replacedTitle,
				Singer = Replace(string.Join(",", gInfo.Singers.Select(x => Replace(x, replacedTitle)).Where(x => !string.IsNullOrWhiteSpace(x))), replacedTitle),
				Genre = Replace(gInfo.Genre, replacedTitle),
				Series = Replace(gInfo.Series, replacedTitle),
				SongType = Replace(gInfo.SongType, replacedTitle)
			};

			// ジャンルを置換
			if (gInfo.Genre != null) {
				switch (gInfo.Genre) {
					case "GM":
						result.Genre = "GM";
						break;

					case "RD":
					case "WR":
						result.Genre = "RD";
						break;

					case "DK":
						result.Genre = "同人";
						break;

					case "KK":
						result.Genre = "企画";
						break;

					case "SF":
					case "SS":
					case "SV":
					case "SM":
					case "WS":
					case "NG":
						result.Genre = "特撮";
						break;

					case "TV":
					case "TS":
					case "VD":
					case "MV":
					case "WA":
						result.Genre = null;
						break;

					default: // 一般曲扱い
						result.Genre = null;
						result.SongType = null;
						result.Series = "一般曲";
						break;
				}
			}

			// 曲種に基づいてジャンルを置換
			if (gInfo.SongType != null) {
				switch (gInfo.SongType) {
					case "KK":
						result.Genre = "企画";
						result.SongType = null;
						break;
				}
			}

			return result;
		}

		string Replace(string text, string songName = null)
		{
			if (text == null) { throw new ArgumentNullException("text"); }
			var str = text;
			foreach (var rep in Replaces) {
				if (!string.IsNullOrWhiteSpace(rep.SongTitleConstraint) && songName != rep.SongTitleConstraint) {
					continue;
				}

				if (rep.Exact && rep.Original != str) {
					continue;
				}

				str = str.Replace(rep.Original, rep.Replaced);
			}
			
			// 前後の空白削除して返す
			return str.Trim();
		}
	}
}
