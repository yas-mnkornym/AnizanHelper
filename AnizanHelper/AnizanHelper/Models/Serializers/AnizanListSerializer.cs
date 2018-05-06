using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AnizanHelper.Models.Serializers
{
	/// <summary>
	/// アニ三リストスレ形式へのシリアライザ
	/// </summary>
	internal class AnizanListSerializer : ISongInfoSerializer
	{
		static readonly string[] replaceList = new string[]{
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
			if (info == null) { throw new ArgumentNullException("info"); }

			// フォーマット構築
			StringBuilder sb = new StringBuilder();
			sb.Append(".｢{0}｣/{1}(");

			// 使用作品名追加
			if (!string.IsNullOrWhiteSpace(info.Series)) {
				// ジャンル追加
				if (!string.IsNullOrWhiteSpace(info.Genre)) {
					sb.Append("[{2}]");
				}
				sb.Append("{3}");
			}

			// 曲種追加
			if (!string.IsNullOrWhiteSpace(info.SongType)) {
				sb.Append("　{4}");
			}

			sb.Append(")");

			// 補足追加
			if (!string.IsNullOrWhiteSpace(info.Additional)) {
				sb.Append("※{5}");
			}

			var result = string.Format(
				sb.ToString(),
				info.Title,
				info.Singer,
				info.Genre,
				info.Series,
				info.SongType,
				info.Additional);
			for (int i = 0; i < replaceList.Length-1; i++) {
				result = Regex.Replace(result, replaceList[i], replaceList[i + 1]);
			}
			return result;
		}

		public string SerializeFull(AnizanSongInfo info)
		{
			var sb = new StringBuilder();

			var body = Serialize(info);
			var isSpecial = !string.IsNullOrWhiteSpace(info.SpecialHeader);

			if(isSpecial) {
				sb.Append(info.SpecialHeader);
				sb.Append(info.SpecialItemName);
				body = body.Substring(1);
			}
			else {
				sb.AppendFormat("{0:0000}", info.Number);
			}
			
			sb.Append(body);
			return sb.ToString();
		}
	}
}
