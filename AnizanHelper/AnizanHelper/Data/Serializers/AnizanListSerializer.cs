using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnizanHelper.Data.Serializers
{
	/// <summary>
	/// アニ三リストスレ形式へのシリアライザ
	/// </summary>
	internal class AnizanListSerializer : ISongInfoSerializer
	{
		public string Serialize(SongInfo info)
		{
			if (info == null) { throw new ArgumentNullException("info"); }
			string format = ".｢{0}｣/{1}({3}　{4})";
			if (info.IsNotAnison) {
				format = ".｢{0}｣/{1}(一般曲)";
			}
			else if (!string.IsNullOrEmpty(info.Genre)) {
				if (string.IsNullOrWhiteSpace(info.SongType)) {
					format = ".｢{0}｣/{1}([{2}]{3})";
				}
				else {
					format = ".｢{0}｣/{1}([{2}]{3}　{4})";
				}
			}
			return string.Format(
				format,
				info.Title,
				info.Singer,
				info.Genre,
				info.Series,
				info.SongType);
		}
	}
}
