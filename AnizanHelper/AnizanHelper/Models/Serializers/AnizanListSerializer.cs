using System;
using System.Text;
using System.Text.RegularExpressions;

namespace AnizanHelper.Models.Serializers
{
	/// <summary>
	/// アニ三リストスレ形式へのシリアライザ
	/// </summary>
	internal class AnizanListSerializer : ISongInfoSerializer
	{
		private static readonly string[] replaceList = new string[]{
			// 両側空白
			@"(?<prev>[^\s])&(?<next>[^\s])",
			@"${prev} & ${next}",

			// 前側空白
			@"(?<prev>\s)&(?<next>[^\s])",
			@"${prev}& ${next}",
			
			// 後ろ側空白
			@"(?<prev>[^\s])&(?<next>\s)",
			@"${prev} &${next}"
		};


		public string Serialize(AnizanSongInfo info)
		{
			return this.Serialize(info, true);
		}

		private string Serialize(AnizanSongInfo info, bool appendAll)
		{
			if (info == null) { throw new ArgumentNullException("info"); }

			// フォーマット構築
			StringBuilder sb = new StringBuilder();
			sb.Append(".｢{0}｣");

			var hasArtist = !string.IsNullOrWhiteSpace(info.Artist);

			if (appendAll || hasArtist)
			{
				sb.Append("/{1}");
			}

			var hasSeries = !string.IsNullOrWhiteSpace(info.Series);
			var hasGenre = !string.IsNullOrWhiteSpace(info.Genre);
			var hasSongType = !string.IsNullOrWhiteSpace(info.SongType);

			if (appendAll || hasSeries || hasSongType)
			{
				sb.Append("(");
			}

			// 使用作品名追加
			if (hasSeries)
			{
				// ジャンル追加
				if (hasGenre)
				{
					sb.Append("[{2}]");
				}

				sb.Append("{3}");
			}

			// 曲種追加
			if (hasSongType)
			{
				sb.Append("　{4}");
			}

			if (appendAll || hasSeries || hasSongType)
			{
				sb.Append(")");
			}

			// 補足追加
			if (!string.IsNullOrWhiteSpace(info.Additional))
			{
				sb.Append("※{5}");
			}

			var result = string.Format(
				sb.ToString(),
				info.Title,
				info.Artist,
				info.Genre,
				info.Series,
				info.SongType,
				info.Additional);
			for (int i = 0; i < replaceList.Length - 1; i++)
			{
				result = Regex.Replace(result, replaceList[i], replaceList[i + 1]);
			}
			return result;
		}

		public string SerializeFull(AnizanSongInfo info)
		{
			var sb = new StringBuilder();

			var body = this.Serialize(info, false);
			var isSpecial = !string.IsNullOrWhiteSpace(info.SpecialHeader);

			if (isSpecial)
			{
				sb.Append(info.SpecialHeader);
				sb.Append(info.SpecialItemName);
				body = body.Substring(1);
			}
			else
			{
				sb.AppendFormat("{0:0000}", info.Number);
			}

			sb.Append(body);
			return sb.ToString();
		}
	}
}
