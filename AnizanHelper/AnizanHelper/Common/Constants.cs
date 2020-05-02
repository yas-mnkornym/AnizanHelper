namespace AnizanHelper
{
	internal static class Constants
	{
		/// <summary>
		/// 設定ファイル名
		/// </summary>
		public static string SettingsFileName { get; } = "AnizanHelper.xml";

		/// <summary>
		/// 設定一時ファイル名
		/// </summary>
		public static string SettingsTempFileName { get; } = "AnzanHelper.xml.tmp";

		/// <summary>
		/// 初期設定鯖名
		/// </summary>
		public static string DefaultServerName { get; } = "anizanv.com";

		/// <summary>
		/// アニソン三昧さーちURL
		/// </summary>
		public static string ZanmaiSearchUrl { get; } = "http://anizanv.ddo.jp/archive/list/";

		/// <summary>
		/// 初期設定板パス
		/// </summary>
		public static string DefaultBoardPath { get; } = "anizanv2017";

		/// <summary>
		/// 置換辞書ファイル名
		/// </summary>
		public static string DictionaryFileName { get; } = "dictionary.txt";

		/// <summary>
		/// ユーザ定義置換辞書ファイル名
		/// </summary>
		public static string UserDictionaryFileName { get; } = "dictionary.user.txt";

		/// <summary>
		/// 置換辞書URL
		/// </summary>
		public static string ReplaceDictionaryUrl { get; } = @"https://www.studio-taiha.net/anizan/anizanhelper/dictionary/update.php";

		/// <summary>
		/// 置換辞書バージョン情報URL
		/// </summary>
		public static string ReplaceDictionaryVersionUrl { get; } = @"https://www.studio-taiha.net/anizan/anizanhelper/dictionary/update.php?t=v";

		/// <summary>
		/// 辞書更新情報URL
		/// </summary>
		public static string ReplaceDictionaryUpdateInfoUrl { get; } = @"https://www.studio-taiha.net/anizan/anizanhelper/dictionary/update.php?t=u";

		/// <summary>
		/// 本体更新情報URL
		/// </summary>
		public static string UpdateInfoUrl { get; } = @"https://www.studio-taiha.net/anizan/anizanhelper/updates.json";

		/// <summary>
		/// アニソンDBぱーさー サポートページURL
		/// </summary>
		public static string AnizanHelperWebSiteUrl { get; } = @"https://www.studio-taiha.net/anizan/anizanhelper";

	}
}
