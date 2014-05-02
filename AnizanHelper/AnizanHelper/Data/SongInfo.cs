using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnizanHelper.Data
{
	internal class SongInfo
	{
		/// <summary>
		/// 曲名
		/// </summary>
		public string Title { get; set; }

		/// <summary>
		/// 歌手
		/// </summary>
		public string Singer { get; set; }

		/// <summary>
		/// 作品名
		/// </summary>
		public string Series { get; set; }

		/// <summary>
		/// 曲種
		/// </summary>
		public string SongType { get; set; }

		public override string ToString()
		{
			return Utils.GeneralObjectToString(this);
		}
	}
}
