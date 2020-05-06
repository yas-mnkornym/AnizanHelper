using System.Reactive.Disposables;
using AnizanHelper.Services;
using Prism.Ioc;
using Prism.Modularity;
using Reactive.Bindings.Extensions;
using Unity;

namespace AnizanHelper.Modules.Services
{
	[Module]
	public class ServicesModule : ModuleBase
	{
		private ServiceManager serviceManager;

		public ServicesModule()
		{

		}

		protected override void RegisterTypes(IUnityContainer unityContainer)
		{
			this.serviceManager = new ServiceManager(unityContainer);
			unityContainer.RegisterInstance((IServiceManager)this.serviceManager);
		}

		public override void OnInitialized(IContainerProvider containerProvider)
		{
			base.OnInitialized(containerProvider);

			this.serviceManager.RegisterServicesFromAssembly(typeof(App).Assembly);
			this.serviceManager.StartAll();

			Disposable
				.Create(() =>
				{
					this.serviceManager.StopAll();
				})
				.AddTo(this.Disposables);
		}
	}
}
