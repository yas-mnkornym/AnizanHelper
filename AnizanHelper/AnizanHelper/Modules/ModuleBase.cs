using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reactive.Disposables;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Unity;
using Unity;

namespace AnizanHelper.Modules
{
	public class AppLifetimeNotifier
	{
		private List<IAppLifetimeAware> Modules { get; } = new List<IAppLifetimeAware>();

		public void Register(IAppLifetimeAware module)
		{
			this.Modules.Add(module);
		}

		public void OnExit(IContainerProvider containerProvider)
		{
			foreach (var module in this.Modules)
			{
				module.OnAppExit(containerProvider);
			}
		}

		public void OnInitialized(IContainerProvider containerProvider)
		{
			foreach (var module in this.Modules)
			{
				module.OnAppInitialized(containerProvider);
			}
		}
	}

	public interface IAppLifetimeAware
	{
		void OnAppInitialized(IContainerProvider containerProvider);
		void OnAppExit(IContainerProvider containerProvider);
	}

	public abstract class ModuleBase : IModule, IAppLifetimeAware
	{
		protected CompositeDisposable Disposables { get; } = new CompositeDisposable();

		public virtual void OnAppExit(IContainerProvider containerProvider)
		{
			this.Disposables.Dispose();
		}

		public virtual void OnAppInitialized(IContainerProvider containerProvider)
		{ }

		public virtual void OnInitialized(IContainerProvider containerProvider)
		{
			var cleanupService = containerProvider.Resolve<AppLifetimeNotifier>();
			cleanupService.Register(this);
		}

		public void RegisterTypes(IContainerRegistry containerRegistry)
		{
			var unityContainer = containerRegistry.GetContainer();
			this.RegisterTypes(unityContainer);
		}

		protected virtual void RegisterTypes(IUnityContainer unityContainer)
		{
		}
	}
}
