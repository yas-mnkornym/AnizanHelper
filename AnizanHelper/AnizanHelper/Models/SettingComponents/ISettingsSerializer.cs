namespace AnizanHelper.Models.SettingComponents
{
	internal interface ISettingsSerializer
	{
		/// <summary>
		/// 設定をシリアライズする
		/// </summary>
		/// <param name="stream">シリアライズすしたデータを記録するストリーム</param>
		/// <param name="settings">シリアライズする設定</param>
		void Serialize(System.IO.Stream stream, SettingsContainer settings);

		/// <summary>
		/// 設定をデシリアライズする
		/// </summary>
		/// <param name="stream">デシリアライズするデータを読み込むストリーム</param>
		/// <param name="settings">デシリアライズしたデータを格納する設定</param>
		void Deserialize(System.IO.Stream stream, SettingsContainer settings);
	}
}
