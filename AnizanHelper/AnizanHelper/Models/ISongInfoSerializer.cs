
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnizanHelper.Models
{
	internal interface ISongInfoSerializer
	{
		/// <summary>
		/// 曲情報を単一の文字列にする
		/// </summary>
		/// <param name="info">曲情報</param>
		/// <returns>シリアライズされた曲情報</returns>
		string Serialize(AnizanSongInfo info);
	}
}
