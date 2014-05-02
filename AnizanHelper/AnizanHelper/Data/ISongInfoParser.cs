using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnizanHelper.Data
{
	internal interface ISongInfoParser
	{
		/// <summary>
		/// テキストを解析して曲情報を抽出する
		/// </summary>
		/// <param name="inputText">入力文字列</param>
		/// <returns>曲情報</returns>
		SongInfo Parse(string inputText);
	}
}
