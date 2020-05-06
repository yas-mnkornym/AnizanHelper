using System;
using System.IO;
using System.Windows;
using AnizanHelper.Models.SettingComponents;
using AnizanHelper.Modules.Services;
using AnizanHelper.Services;
using Prism.Ioc;
using Prism.Modularity;
using Unity;

namespace AnizanHelper.Modules.Settings
{
	[Module]
	[ModuleDependency(nameof(ServicesModule))]
	public sealed class SettingsModule : ModuleBase
	{
		private IServiceManager ServiceManager { get; }

		public SettingsModule(IServiceManager serviceManager)
		{
			this.ServiceManager = serviceManager ?? throw new ArgumentNullException(nameof(serviceManager));
		}

		protected override void RegisterTypes(IUnityContainer unityContainer)
		{
			var container = new SettingsContainer("AnizanHelper", Models.Settings.KnownTypes);

			var settings = new Models.Settings(container);
			unityContainer.RegisterInstance(settings);

			var serializer = new DataContractSettingsSerializer();

			try
			{
				var settingsFilePath = Path.Combine(AppInfo.Current.StartupDirectory, Constants.SettingsFileName);
				if (File.Exists(settingsFilePath))
				{
					using (var fs = new FileStream(settingsFilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
					{
						serializer.Deserialize(fs, container);
					}
				}
			}
			catch
			{
				MessageBox.Show(App.Current.MainWindow, "設定の読込に失敗しました。\nデフォルトの設定を利用します。", "エラー", MessageBoxButton.OK, MessageBoxImage.Exclamation);
			}

			var autoSaverService = new SettingsAutoSaverService(
				container,
				serializer,
				TimeSpan.FromMilliseconds(250));

			this.ServiceManager.Register(autoSaverService);
		}


	}
}
