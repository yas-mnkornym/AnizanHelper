using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AnizanHelper
{
	internal static class Constants
	{
		/// <summary>
		/// 設定ファイル名
		/// </summary>
		public static readonly string SettingsFileName = "AnizanHelper.xml";

		/// <summary>
		/// 設定一時ファイル名
		/// </summary>
		public static readonly string SettingsTempFileName = "AnzanHelper.xml.tmp";

		/// <summary>
		/// 初期設定鯖名
		/// </summary>
		public static readonly string DefaultServerName = "anizanv.ddo.jp";

		/// <summary>
		/// 初期設定板パス
		/// </summary>
		public static readonly string DefaultBoardPath = "operate";

		/// <summary>
		/// 置換辞書ファイル名
		/// </summary>
		public static readonly string DictionaryFileName = "dictionary.txt";

		/// <summary>
		/// 置換辞書URL
		/// </summary>
		public static readonly string ReplaceDictionaryUrl = @"http://www.studio-taiha.net/anizan/anizanhelper/dictionary/update.php";

		/// <summary>
		/// 置換辞書バージョン情報URL
		/// </summary>
		public static readonly string ReplaceDictionaryVersionUrl = @"http://www.studio-taiha.net/anizan/anizanhelper/dictionary/update.php?t=v";

		/// <summary>
		/// 更新情報URL
		/// </summary>
		public static readonly string ReplaceDictionaryUpdateInfoUrl = @"http://www.studio-taiha.net/anizan/anizanhelper/dictionary/update.php?t=u";

	}
}
