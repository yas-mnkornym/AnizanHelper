using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using AnizanHelper.Models;
using AnizanHelper.Models.DbSearch;
using AnizanHelper.Models.Registries;
using AnizanHelper.Models.SettingComponents;
using AnizanHelper.Services;
using AnizanHelper.ViewModels;
using AnizanHelper.Views;
using Prism.Ioc;
using Prism.Regions;
using Prism.Unity;
using Unity;

namespace AnizanHelper
{
	/// <summary>
	/// App.xaml の相互作用ロジック
	/// </summary>
	public partial class App : PrismApplication
	{
		private CompositeDisposable Disposables { get; } = new CompositeDisposable();
		private Settings settings_;
		private SettingsAutoExpoter settingsAutoExpoter_;
		private AnizanSongInfoConverter converter_;
		private SongPresetRepository songPresetRepository_;

		protected override Window CreateShell()
		{
			var window = this.Container.Resolve<MainWindow>();
			window.Loaded += this.Window_Loaded;
			return window;
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			var regionManager = this.Container.Resolve<IRegionManager>();
			regionManager.RequestNavigate("Region_Search", nameof(SongSearchControl));
		}

		protected override void RegisterTypes(IContainerRegistry containerRegistry)
		{
			var unityContainer = containerRegistry.GetContainer();

			new UnityConfig().RegisterTypes(unityContainer);

			containerRegistry.RegisterSingleton<MainWindow>();
			containerRegistry.RegisterForNavigation<SongSearchControl>();

			// 設定初期化
			var settings = this.InitializeSettings();
			unityContainer.RegisterInstance(this.settings_);

			// Initialize service manager
			var serviceManager = this.Container.Resolve<IServiceManager>();
			(serviceManager as ServiceManager)?.RegisterServicesFromAssembly(this.GetType().Assembly);
			serviceManager.StartAll();

			// コンバータをロード
			this.LoadDictionaries();


			unityContainer.RegisterInstance(this.converter_);
			unityContainer.RegisterInstance(this.songPresetRepository_);
		}

		//protected override void OnInitialized()
		//{
		//	base.OnInitialized();

		//	// 辞書の更新を確認
		//	Task.Factory.StartNew(() =>
		//	{
		//		this.UpdateDictionary();
		//	});
		//}

		protected override void OnExit(ExitEventArgs e)
		{
			// 設定自動保存を停止
			if (this.settingsAutoExpoter_ != null)
			{
				this.settingsAutoExpoter_.Dispose();
				this.settingsAutoExpoter_ = null;
			}

			// Stop all the services
			this.Container.Resolve<IServiceManager>().StopAll();

			this.Disposables.Dispose();

			base.OnExit(e);
		}

		public void LoadDictionaries()
		{
			if (this.converter_ == null)
			{
				this.converter_ = new AnizanSongInfoConverter();
			}

			if (this.songPresetRepository_ == null)
			{
				this.songPresetRepository_ = new SongPresetRepository();
			}

			var replaceList = new List<ReplaceInfo>();
			var presetList = new List<AnizanSongInfo>();

			try
			{
				if (File.Exists(AppInfo.Current.DictionaryFilePath))
				{
					var loader = new DictionaryFileReader();
					loader.Load(AppInfo.Current.DictionaryFilePath);

					replaceList.AddRange(loader.Replaces);
					presetList.AddRange(loader.SongPresets);
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(
					string.Format("置換辞書ファイルの読み込みに失敗しました。\n\n【例外情報】\n{0}", ex)
					, "エラー", MessageBoxButton.OK, MessageBoxImage.Warning);
			}

			// ユーザ辞書の内容を優先
			try
			{
				if (File.Exists(AppInfo.Current.UserDictionaryfilePath))
				{
					var loader = new DictionaryFileReader();
					loader.Load(AppInfo.Current.UserDictionaryfilePath);

					foreach (var replace in loader.Replaces)
					{
						var item = replaceList.FirstOrDefault(x => x.Original == replace.Original);
						if (item != null)
						{
							replaceList.Remove(item);
						}

						replaceList.Add(replace);
					}

					presetList.AddRange(loader.SongPresets);
				}
				else
				{
					try
					{
						File.WriteAllText(AppInfo.Current.UserDictionaryfilePath, "rem,これはユーザ定義辞書ファイルです。dictionary.txtを参考にUTF-8で記述して下さい。");
					}
					catch { }
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(
					App.Current.MainWindow,
					string.Format("ユーザ定義置換辞書ファイルの読み込みに失敗しました。\n\n【例外情報】\n{0}", ex),
					"エラー", MessageBoxButton.OK, MessageBoxImage.Warning);
			}

			this.converter_.Replaces = replaceList.Where(x => !string.IsNullOrEmpty(x.Original)).ToArray();
			this.songPresetRepository_.Presets = presetList.ToArray();
		}

		public void UpdateDictionary()
		{
			MessageService.Current.ShowMessage("新しい辞書を確認しています...");
			try
			{
				var updates = this.CheckForDictionaryUpdate();
				if (!updates.Any())
				{
					MessageService.Current.ShowMessage("新しい辞書はありませんでした。");
					return;
				}

				StringBuilder sb = new StringBuilder();
				sb.AppendLine("新しい辞書があります。更新しますか？");
				sb.AppendLine();
				sb.AppendLine("*** 更新情報 ***");
				foreach (var info in updates.OrderByDescending(x => x.Version))
				{
					sb.AppendFormat("【Version {0}】- {1}", info.Version, info.Date.ToString("yyyy年MM月dd日 HH時mm分"));
					sb.AppendLine();
					sb.AppendLine(info.Description);
				}
				var ret = MessageBox.Show(App.Current.MainWindow, sb.ToString(), "確認", MessageBoxButton.YesNo, MessageBoxImage.Question);
				if (ret != MessageBoxResult.Yes)
				{
					MessageService.Current.ShowMessage("辞書の更新がキャンセルされました。");
					return;
				}

				MessageService.Current.ShowMessage("最新の辞書を取得しています...");
				var updater = new ReplaceDictionaryUpdater();
				var newDic = updater.DownloadDictionary();

				MessageService.Current.ShowMessage("辞書を更新しています...");
				File.WriteAllText(AppInfo.Current.DictionaryFilePath, newDic, Encoding.UTF8);
				this.LoadDictionaries();

				MessageService.Current.ShowMessage("辞書の更新が完了しました！");
			}
			catch (Exception ex)
			{
				MessageService.Current.ShowMessage("更新に失敗しました。(" + ex.Message + ")");
			}
		}

		public IEnumerable<ReplaceDictionaryUpdateInfo> CheckForDictionaryUpdate()
		{
			var currentVersion = 0;
			var filePath = AppInfo.Current.DictionaryFilePath;
			if (File.Exists(filePath))
			{
				var reader = new DictionaryFileReader();
				try
				{
					currentVersion = reader.GetVersionNumber(filePath);
				}
				catch { }
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
		private Settings InitializeSettings()
		{
			var settingsImpl = new SettingsImpl("AnizanHelper", Settings.KnownTypes);
			this.settings_ = new Settings(settingsImpl, new WPFDispatcher(App.Current.Dispatcher));
			var serializer = new DataContractSettingsSerializer();

			// 設定を読込
			try
			{
				var settingsFilePath = Path.Combine(AppInfo.Current.StartupDirectory, Constants.SettingsFileName);
				if (File.Exists(settingsFilePath))
				{
					using (var fs = new FileStream(settingsFilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
					{
						serializer.Deserialize(fs, settingsImpl);
					}
				}
			}
			catch
			{
				MessageBox.Show(App.Current.MainWindow, "設定の読込に失敗しました。\nデフォルトの設定を利用します。", "エラー", MessageBoxButton.OK, MessageBoxImage.Exclamation);
			}

			this.settingsAutoExpoter_ = new SettingsAutoExpoter(
				Path.Combine(AppInfo.Current.StartupDirectory, Constants.SettingsFileName),
				Path.Combine(AppInfo.Current.StartupDirectory, Constants.SettingsTempFileName),
				settingsImpl, serializer, 300);

			return this.settings_;
		}
	}
}
