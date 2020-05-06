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
	public class ModuleCleanupService
	{
		private List<IDestructiveModule> Modules { get; } = new List<IDestructiveModule>();

		public void Register(IDestructiveModule module)
		{
			this.Modules.Add(module);
		}

		public void Cleanup()
		{
			foreach (var module in this.Modules)
			{
				module.Cleanup();
			}
		}
	}

	public interface IDestructiveModule : IModule
	{
		void Cleanup();
	}

	public abstract class ModuleBase : IModule, IDestructiveModule
	{
		protected CompositeDisposable Disposables { get; } = new CompositeDisposable();

		public virtual void Cleanup()
		{
			this.Disposables.Dispose();
		}

		public virtual void OnInitialized(IContainerProvider containerProvider)
		{
			var cleanupService = containerProvider.Resolve<ModuleCleanupService>();
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
