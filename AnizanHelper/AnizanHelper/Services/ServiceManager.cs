using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity;

namespace AnizanHelper.Services
{
	public class ServiceManager : IServiceManager
	{
		public IUnityContainer UnityContainer { get; }
		private List<IService> ServiceList { get; } = new List<IService>();

		public IEnumerable<IService> Services => this.ServiceList;

		public ServiceManager(IUnityContainer unityContainer)
		{
			this.UnityContainer = unityContainer ?? throw new ArgumentNullException(nameof(unityContainer));
		}

		public void RegisterServicesFromAssembly(Assembly assembly)
		{
			var services = assembly
				.GetTypes()
				.Where(type => !type.IsAbstract && type.GetInterface(typeof(IService).FullName) != null)
				.Select(type => new
				{
					Type = type,
					Attribute = type.GetCustomAttribute<ServiceAttribute>(),
				})
				.Where(x => x.Attribute?.IsEnabled == true)
				.Select(x =>
				{
					try
					{
						return (IService)this.UnityContainer.Resolve(x.Type);
					}
					catch (Exception ex)
					{
						Console.WriteLine(ex);
						// TOOD: log error
					}

					return null;
				})
				.Where(x => x != null);

			this.ServiceList.AddRange(services);
		}

		public void StartAll()
		{
			foreach (var service in this.ServiceList)
			{
				service.Start();
			}
		}

		public void StopAll()
		{
			foreach (var service in this.ServiceList)
			{
				service.Stop();
			}
		}

		public void Register(IService service)
		{
			if (service == null) { throw new ArgumentNullException(nameof(service)); }
			this.ServiceList.Add(service);
		}

		public void Unregister(IService service)
		{
			if (service == null) { throw new ArgumentNullException(nameof(service)); }
			if (this.ServiceList.Remove(service))
			{
				service.Dispose();
			}
		}

		public void UnregisterAll()
		{
			var services = this.ServiceList.ToArray();
			this.ServiceList.Clear();

			foreach (var service in services)
			{
				service.Dispose();
			}
		}
	}
}
