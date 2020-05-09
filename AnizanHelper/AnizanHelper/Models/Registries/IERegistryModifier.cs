using Microsoft.Win32;

namespace AnizanHelper.Models.Registries
{
	internal class IERegistryManager
	{
		private static readonly string RegKeyEmulation =
			@"Software\Microsoft\Internet Explorer\Main\FeatureControl\FEATURE_BROWSER_EMULATION";
		private static readonly string[] RegKeyIEs = new string[]{
			@"Software\Microsoft\Internet Explorer\Main\FeatureControl\FEATURE_GPU_RENDERING",
			@"Software\Microsoft\Internet Explorer\Main\FeatureControl\FEATURE_AJAX_CONNECTIONEVENTS",
			@"Software\Microsoft\Internet Explorer\Main\FeatureControl\FEATURE_WEBSOCKET",
			@"Software\Microsoft\Internet Explorer\Main\FeatureControl\FEATURE_XMLHTTP"
		};

		public string ApplicationName { get; set; }
		public bool ToMachine { get; set; }

		public IERegistryManager(
			string appName,
			bool toMachine)
		{
			this.ApplicationName = appName;
			this.ToMachine = toMachine;
		}

		#region レジストリ操作
		/// <summary>
		/// レジストリに必要情報を追加する。
		/// 必要に応じてUACの通過を行う。
		/// </summary>
		/// <param name="appName"></param>
		public void ManageRegistry(bool delete = false)
		{

			if (delete)
			{
				this.DeleteRegistry(this.ApplicationName, this.ToMachine);
			}
			else
			{
				if (ShouldResetRegistry(this.ApplicationName, this.ToMachine))
				{ // キーがない場合、追加しないと！
					this.AddRegistry(this.ApplicationName, this.ToMachine);
				}
			}
		}

		private static int RegValueIE
		{
			get
			{
				try
				{
					var key = Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\Internet Explorer", false);
					var version = (string)key.GetValue("svcVersion");
					int majorVersion = int.Parse(version.Substring(0, version.IndexOf('.')));
					switch (majorVersion)
					{
						case 11:
							return 11001;

						case 10:
							return 10001;

						case 9:
							return 9999;

						case 8:
							return 8888;

						case 7:
							return 7000;

						default:
							return majorVersion * 1000;
					}
				}
				catch
				{
					return 8000;
				}
			}
		}

		/// <summary>
		/// レジストリを追加する
		/// </summary>
		/// <param name="appName">艦ぶら実行ファイル名</param>
		private void AddRegistry(string appName, bool toMachine)
		{
			using (var editor = new RegistryEditor(appName, toMachine))
			{
				editor.SetValue(RegKeyEmulation, RegValueIE, RegistryValueKind.DWord);
				foreach (var key in RegKeyIEs)
				{
					editor.SetValue(key, 1, RegistryValueKind.DWord);
				}
			}
		}

		/// <summary>
		/// レジストリを削除する
		/// </summary>
		/// <param name="appName">艦ぶら実行ファイル名</param>
		private void DeleteRegistry(string appName, bool forOldVersion = false)
		{
			// レジストリキーを削除
			using (var editor = new RegistryEditor(appName, forOldVersion))
			{
				editor.DeleteKey(RegKeyEmulation);
				foreach (var key in RegKeyIEs)
				{
					editor.DeleteKey(key);
				}
			}
		}

		/// <summary>
		/// レジストリの更新が必要かどうかを確認する
		/// </summary>
		/// <param name="appName"></param>
		/// <returns></returns>
		public static bool ShouldResetRegistry(string appName, bool toMachine)
		{
			// キーの存在を確認
			using (var editor = new RegistryEditor(appName, toMachine))
			{
				if (!editor.IsKeyExists(RegKeyEmulation))
				{
					return true;
				}
			}

			// 値を確認
			using (var editor = new RegistryEditor(appName, toMachine))
			{
				var ieVal = editor.GetValue(RegKeyEmulation, (int)0);
				if (ieVal != RegValueIE)
				{
					return true;
				}
			}

			return false;
		}

		#endregion

	}
}
