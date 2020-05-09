using AnizanHelper.Models;
using AnizanHelper.Modules.Settings;
using Prism.Ioc;
using Prism.Modularity;
using Unity;

namespace AnizanHelper.Modules.Dictionaries
{
	[Module]
	[ModuleDependency(nameof(SettingsModule))]
	public class DictionaryModule : ModuleBase
	{
		protected override void RegisterTypes(IUnityContainer unityContainer)
		{
			unityContainer.RegisterSingleton<IDictionaryManager, DictionaryManager>();
		}

		public override void OnAppInitialized(IContainerProvider containerProvider)
		{
			base.OnAppInitialized(containerProvider);

			var manager = containerProvider.Resolve<IDictionaryManager>();
			manager.LoadAsync().FireAndForget();
		}
	}
}
