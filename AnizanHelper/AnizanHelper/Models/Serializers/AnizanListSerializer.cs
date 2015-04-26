using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnizanHelper.Models.Serializers
{
	/// <summary>
	/// アニ三リストスレ形式へのシリアライザ
	/// </summary>
	internal class AnizanListSerializer : ISongInfoSerializer
	{
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

			return string.Format(
				sb.ToString(),
				info.Title,
				info.Singer,
				info.Genre,
				info.Series,
				info.SongType);
		}
	}
}
