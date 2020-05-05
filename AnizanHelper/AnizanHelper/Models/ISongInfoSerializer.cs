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
		string SerializeFull(AnizanSongInfo info);
	}
}
