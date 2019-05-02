using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using AnizanHelper.Models;
using AnizanHelper.Models.Registries;
using AnizanHelper.Models.SettingComponents;
using AnizanHelper.Services;
using AnizanHelper.ViewModels;
using AnizanHelper.Views;
using Unity;

namespace AnizanHelper
{


	/// <summary>
	/// App.xaml の相互作用ロジック
	/// </summary>
	public partial class App : Application
	{
		CompositeDisposable Disposables { get; } = new CompositeDisposable();
		UnityContainer UnityContainer { get; } = new UnityContainer();
		Settings settings_;
		SettingsAutoExpoter settingsAutoExpoter_;
		AnizanSongInfoConverter converter_;

		protected override void OnStartup(StartupEventArgs e)
		{
			base.OnStartup(e);

			// レジストリを設定
			var config = new UnityConfig();
			config.RegisterTypes(UnityContainer);

			// 設定初期化
			InitializeSettinsg();
			UnityContainer.RegisterInstance(settings_);
			
			// Initialize service manager
			var serviceManager = UnityContainer.Resolve<IServiceManager>();
			(serviceManager as ServiceManager)?.RegisterServicesFromAssembly(this.GetType().Assembly);
			serviceManager.StartAll();

			// コンバータをロード
			LoadReplaceDictionary();

			// メインウィンドウ作成
			MainWindow = new MainWindow();
			MainWindow.DataContext = new MainWindowViewModel(
				MainWindow,
				settings_,
				converter_,
				serviceManager,
				UnityContainer.Resolve<HttpClient>(),
				new WPFDispatcher(Dispatcher));
			MainWindow.Show();

			// 辞書の更新を確認
			Task.Factory.StartNew(() => {
				UpdateDictionary();
			});
		}

		protected override void OnExit(ExitEventArgs e)
		{
			// 設定自動保存を停止
			if (settingsAutoExpoter_ != null) {
				settingsAutoExpoter_.Dispose();
				settingsAutoExpoter_ = null;
			}

			// Stop all the services
			UnityContainer.Resolve<IServiceManager>().StopAll();

			Disposables.Dispose();

			base.OnExit(e);
		}

		public void LoadReplaceDictionary()
		{
			if (converter_ == null) {
				converter_ = new AnizanSongInfoConverter();
			}

			var replaceList = new List<ReplaceInfo>();

			try {
				if (File.Exists(AppInfo.Current.DictionaryFilePath)) {
					var loader = new ReplaceDictionaryFileReader();
					var replaces = loader.Load(AppInfo.Current.DictionaryFilePath).ToArray();
					if (replaces?.Any() == true) {
						replaceList.AddRange(replaces);
					}
				}
			}
			catch (Exception ex) {
				MessageBox.Show(
					string.Format("置換辞書ファイルの読み込みに失敗しました。\n\n【例外情報】\n{0}", ex)
					, "エラー", MessageBoxButton.OK, MessageBoxImage.Warning);
			}

			// ユーザ辞書の内容を優先
			try {
				if (File.Exists(AppInfo.Current.UserDictionaryfilePath)) {
					var loader = new ReplaceDictionaryFileReader();
					var replaces = loader.Load(AppInfo.Current.UserDictionaryfilePath).ToArray();
					foreach (var replace in replaces) {
						var item = replaceList.FirstOrDefault(x => x.Original == replace.Original);
						if (item != null) {
							replaceList.Remove(item);
						}

						replaceList.Add(replace);
					}
				}
				else {
					try {
						File.WriteAllText(AppInfo.Current.UserDictionaryfilePath, "rem,これはユーザ定義辞書ファイルです。dictionary.txtを参考にUTF-8で記述して下さい。");
					}
					catch { }
				}
			}
			catch (Exception ex) {
				MessageBox.Show(
					string.Format("ユーザ定義置換辞書ファイルの読み込みに失敗しました。\n\n【例外情報】\n{0}", ex)
					, "エラー", MessageBoxButton.OK, MessageBoxImage.Warning);
			}

			converter_.Replaces = replaceList.Where(x => !string.IsNullOrEmpty(x.Original)).ToArray();
		}

		public void UpdateDictionary()
		{
			MessageService.Current.ShowMessage("新しい辞書を確認しています...");
			try {
				var updates = CheckForDictionaryUpdate();
				if (!updates.Any()) {
					MessageService.Current.ShowMessage("新しい辞書はありませんでした。");
					return;
				}

				StringBuilder sb = new StringBuilder();
				sb.AppendLine("新しい辞書があります。更新しますか？");
				sb.AppendLine();
				sb.AppendLine("*** 更新情報 ***");
				foreach (var info in updates.OrderByDescending(x => x.Version)) {
					sb.AppendFormat("【Version {0}】- {1}", info.Version, info.Date.ToString("yyyy年MM月dd日 HH時mm分"));
					sb.AppendLine();
					sb.AppendLine(info.Description);
				}
				var ret = MessageBox.Show(sb.ToString(), "確認", MessageBoxButton.YesNo, MessageBoxImage.Question);
				if (ret != MessageBoxResult.Yes) {
					MessageService.Current.ShowMessage("辞書の更新がキャンセルされました。");
					return;
				}

				MessageService.Current.ShowMessage("最新の辞書を取得しています...");
				var updater = new ReplaceDictionaryUpdater();
				var newDic = updater.DownloadDictionary();

				MessageService.Current.ShowMessage("辞書を更新しています...");
				File.WriteAllText(AppInfo.Current.DictionaryFilePath, newDic, Encoding.UTF8);
				LoadReplaceDictionary();

				MessageService.Current.ShowMessage("辞書の更新が完了しました！");
			}
			catch (Exception ex) {
				MessageService.Current.ShowMessage("更新に失敗しました。(" + ex.Message + ")");
			}
		}

		public IEnumerable<ReplaceDictionaryUpdateInfo> CheckForDictionaryUpdate()
		{
			var currentVersion = 0;
			var filePath = AppInfo.Current.DictionaryFilePath;
			if (File.Exists(filePath)) {
				var reader = new ReplaceDictionaryFileReader();
				try{
					currentVersion = reader.GetVersionNumber(filePath);
				}
				catch{}
			}

			var updater = new ReplaceDictionaryUpdater();
			var latestVersion = updater.GetLatestVersionNumber();
			if (latestVersion <= currentVersion) { return Enumerable.Empty<ReplaceDictionaryUpdateInfo>(); }

			return updater.GetUpdateInfo().Where(x => x.Version > currentVersion);
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

		void ConfigureRegistry()
		{
			var appName = AppInfo.Current.ExecutionFileName;
			/*
			if (System.Diagnostics.Debugger.IsAttached) {
				appName = System.IO.Path.GetFileNameWithoutExtension(appName)
					+ ".vshost"
					+ System.IO.Path.GetExtension(appName);
			}
			 * */
			if (IERegistryManager.ShouldResetRegistry(appName, false)) {
				var manager = new IERegistryManager(appName, false);
				try {
					manager.ManageRegistry();
				}
				catch (Exception ex) {
					MessageService.Current.ShowMessage("レジストリの変更に失敗しました。" + ex.Message);
				}
			}
		}
	}
}
