using System;
using System.Linq;

namespace AnizanHelper.Models
{
	public class AnizanSongInfoConverter
	{
		public ReplaceInfo[] Replaces { get; set; }

		public AnizanSongInfoConverter(params ReplaceInfo[] replaces)
		{
			this.Replaces = replaces;
		}

		public ZanmaiSongInfo Convert(ZanmaiSongInfo gInfo)
		{
			var replacedTitle = this.Replace(gInfo.Title);
			var result = new ZanmaiSongInfo
			{
				Title = replacedTitle,
				Artists = this.Replace(string.Join(",", gInfo.Artists.Select(x => this.Replace(x, replacedTitle)).Where(x => !string.IsNullOrWhiteSpace(x))), replacedTitle).Split(','),
				Genre = this.Replace(gInfo.Genre, replacedTitle),
				Series = this.Replace(gInfo.Series, replacedTitle),
				SongType = this.Replace(gInfo.SongType, replacedTitle)
			};

			// ジャンルを置換
			if (gInfo.Genre != null)
			{
				switch (gInfo.Genre)
				{
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
			if (gInfo.SongType != null)
			{
				switch (gInfo.SongType)
				{
					case "KK":
						result.Genre = "企画";
						result.SongType = null;
						break;
				}
			}

			return result;
		}

		private string Replace(string text, string songName = null)
		{
			if (text == null) { throw new ArgumentNullException("text"); }
			var str = text;
			foreach (var rep in this.Replaces)
			{
				if (!string.IsNullOrWhiteSpace(rep.SongTitleConstraint) && songName != rep.SongTitleConstraint)
				{
					continue;
				}

				if (rep.Exact && rep.Original != str)
				{
					continue;
				}

				str = str.Replace(rep.Original, rep.Replaced);
			}

			// 前後の空白削除して返す
			return str.Trim();
		}
	}
}
