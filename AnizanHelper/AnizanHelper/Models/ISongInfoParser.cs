namespace AnizanHelper.Models
{
	internal interface ISongInfoParser
	{
		/// <summary>
		/// テキストを解析して曲情報を抽出する
		/// </summary>
		/// <param name="inputText">入力文字列</param>
		/// <returns>曲情報</returns>
		ZanmaiSongInfo Parse(string inputText);
	}
}
