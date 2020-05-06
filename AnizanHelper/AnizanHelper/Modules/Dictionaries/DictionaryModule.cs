using AnizanHelper.Models;
using Prism.Modularity;
using Unity;

namespace AnizanHelper.Modules.Dictionaries
{
	[Module]
	public class DictionaryModule : ModuleBase
	{
		protected override void RegisterTypes(IUnityContainer unityContainer)
		{
			unityContainer.RegisterSingleton<IDictionaryManager, DictionaryManager>();
		}
	}
}
