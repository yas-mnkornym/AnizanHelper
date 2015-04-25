using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using System.Windows;
using AnizanHelper.Models;
using AnizanHelper.Models.SettingComponents;

namespace AnizanHelper
{

	/// <summary>
	/// App.xaml の相互作用ロジック
	/// </summary>
	public partial class App : Application
	{
		Settings settings_;
		SettingsAutoExpoter settingsAutoExpoter_;

		protected override void OnStartup(StartupEventArgs e)
		{
			base.OnStartup(e);

			// 設定初期化
			InitializeSettinsg();

			// メインウィンドウ作成
			MainWindow = new MainWindow() {
				DataContext = new MainWindowViewModel(settings_, new WPFDispatcher(Dispatcher)) {
				}
			};
			MainWindow.Show();
		}

		protected override void OnExit(ExitEventArgs e)
		{
			// 設定自動保存を停止
			if (settingsAutoExpoter_ != null) {
				settingsAutoExpoter_.Dispose();
				settingsAutoExpoter_ = null;
			}

			base.OnExit(e);
		}

		/// <summary>
		/// 設定を初期化する。
		/// </summary>
		/// <param name="logger"></param>
		void InitializeSettinsg()
		{
			var settingsImpl = new SettingsImpl("AnizanHelper", Settings.KnownTypes);
			settings_ = new Settings(settingsImpl, new WPFDispatcher(App.Current.Dispatcher));
			var serializer = new DataContractSettingsSerializer();

			// 設定を読込
			try {
				var settingsFilePath = Path.Combine(AppInfo.Current.StartupDirectory, Constants.SettingsFileName);
				if (File.Exists(settingsFilePath)) {
					using (var fs = new FileStream(settingsFilePath, FileMode.Open, FileAccess.Read, FileShare.Read)) {
						serializer.Deserialize(fs, settingsImpl);
					}
				}
			}
			catch {
				MessageBox.Show("設定の読込に失敗しました。\nデフォルトの設定を利用します。", "エラー", MessageBoxButton.OK, MessageBoxImage.Exclamation);
			}

			settingsAutoExpoter_ = new SettingsAutoExpoter(
				Path.Combine(AppInfo.Current.StartupDirectory, Constants.SettingsFileName),
				Path.Combine(AppInfo.Current.StartupDirectory, Constants.SettingsTempFileName),
				settingsImpl, serializer, 300);
		}
	}
}
