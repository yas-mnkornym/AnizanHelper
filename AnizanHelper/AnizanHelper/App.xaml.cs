using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Reactive.Disposables;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using AnizanHelper.Models;
using AnizanHelper.Models.SettingComponents;
using AnizanHelper.ViewModels;
using AnizanHelper.Views;
using ComiketSystem.Csv;

namespace AnizanHelper
{

	/// <summary>
	/// App.xaml の相互作用ロジック
	/// </summary>
	public partial class App : Application
	{
		Settings settings_;
		SettingsAutoExpoter settingsAutoExpoter_;
		AnizanSongInfoConverter converter_;

		protected override void OnStartup(StartupEventArgs e)
		{
			base.OnStartup(e);

			// 設定初期化
			InitializeSettinsg();

			// コンバータをロード
			LoadReplaceDictionary();

			// メインウィンドウ作成
			MainWindow = new MainWindow() {
				DataContext = new MainWindowViewModel(settings_, converter_, new WPFDispatcher(Dispatcher)) {
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

		void LoadReplaceDictionary()
		{
			if (converter_ == null) {
				converter_ = new AnizanSongInfoConverter();
			}

			try {
				if (File.Exists(AppInfo.Current.DictionaryFilePath)) {
					var loader = new ReplaceDictionaryFileReader();
					converter_.Replaces = loader.Load(AppInfo.Current.DictionaryFilePath).ToArray();
				}
			}
			catch (Exception ex) {
				MessageBox.Show(
					string.Format("置換辞書ファイルの読み込みに失敗しました。\n\n【例外情報】\n{0}", ex)
					, "エラー", MessageBoxButton.OK, MessageBoxImage.Warning);
			}
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
