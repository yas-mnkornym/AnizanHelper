using System;
using System.Linq;
using System.Net.Http;
using System.Reactive.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using AnizanHelper.Models;
using AnizanHelper.Models.DbSearch;
using AnizanHelper.Models.Updating;
using AnizanHelper.Modules;
using AnizanHelper.ViewModels;
using AnizanHelper.Views;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;
using Prism.Unity;
using Unity;
using Unity.Injection;

namespace AnizanHelper
{
	/// <summary>
	/// App.xaml の相互作用ロジック
	/// </summary>
	public partial class App : PrismApplication
	{
		protected override Window CreateShell()
		{
			var window = this.Container.Resolve<MainWindow>();
			window.Loaded += this.Window_Loaded;
			return window;
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			var regionManager = this.Container.Resolve<IRegionManager>();
			regionManager.RequestNavigate("Region_Search", "SongSearchPage");
			regionManager.RequestNavigate("Region_StreamMetadata", "StreamMetadataViewerPage");
			regionManager.RequestNavigate("Region_SongParser", "SongParserPage");
		}

		protected override void OnInitialized()
		{
			this.MainWindow.DataContext = this.Container.Resolve<MainWindowViewModel>();

			base.OnInitialized();
		}

		protected override void OnExit(ExitEventArgs e)
		{
			var moduleCleanupService = this.Container.Resolve<ModuleCleanupService>();
			moduleCleanupService.Cleanup();

			base.OnExit(e);
		}

		protected override void ConfigureModuleCatalog(IModuleCatalog moduleCatalog)
		{
			base.ConfigureModuleCatalog(moduleCatalog);

			this.GetType().Assembly.GetTypes()
				.Where(x => !x.IsAbstract && !x.IsInterface)
				.Where(x => x.GetTypeInfo().ImplementedInterfaces.Contains(typeof(IModule)))
				.Select(x => new
				{
					Type = x,
					ModuleAttribute = x.GetCustomAttribute<ModuleAttribute>(),
					ModuleDependencyAttributes = x.GetCustomAttributes<ModuleDependencyAttribute>(),
				})
				.Where(x => x.ModuleAttribute != null)
				.ForEach(x =>
				{
					moduleCatalog.AddModule(
						x.ModuleAttribute.ModuleName ?? x.Type.Name,
						x.Type.AssemblyQualifiedName,
						x.ModuleAttribute.OnDemand ? InitializationMode.OnDemand : InitializationMode.WhenAvailable,
						x.ModuleDependencyAttributes.Select(y => y.ModuleName).ToArray());
				});
		}

		protected override void RegisterTypes(IContainerRegistry containerRegistry)
		{
			var unityContainer = containerRegistry.GetContainer();

			unityContainer.RegisterSingleton<ModuleCleanupService>();

			unityContainer.RegisterFactory<HttpClient>(_ => new HttpClient());

			// ------------------------------------------
			// Updating
			// ------------------------------------------
			unityContainer.RegisterType<IUpdateInfoRetreiver, WebUpdateInfoRetreiver>(
				new InjectionConstructor(
					new ResolvedParameter<HttpClient>(),
					new InjectionParameter(new Uri(Constants.UpdateInfoUrl))));

			unityContainer.RegisterSingleton<IUpdateManager, UpdateManager>();

			unityContainer.RegisterSingleton<ProxySearchController>();
			unityContainer.RegisterSingleton<ISearchController, ProxySearchController>();

			unityContainer.RegisterSingleton<AnizanSongInfoConverter>();
			unityContainer.RegisterSingleton<SongPresetRepository>();

			containerRegistry.RegisterSingleton<MainWindow>();

			// Register UI pages
			this.GetType().Assembly.GetTypes()
				.Where(x => x.Name.EndsWith("Page"))
				.Where(x => x.IsSubclassOf(typeof(UserControl)))
				.Select(x => new
				{
					Name = x.Name,
					Type = x,
				})
				.ForEach(x =>
				{
					unityContainer.RegisterTypeForNavigation(x.Type, x.Name);
				});
		}
	}
}
