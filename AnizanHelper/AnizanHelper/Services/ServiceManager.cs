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

		public IEnumerable<IService> Services => ServiceList;

		public ServiceManager(IUnityContainer unityContainer)
		{
			UnityContainer = unityContainer ?? throw new ArgumentNullException(nameof(unityContainer));
		}

		public void RegisterServicesFromAssembly(Assembly assembly)
		{
			var services = assembly
				.GetTypes()
				.Where(type => !type.IsAbstract && type.GetInterface(typeof(IService).FullName) != null)
				.Select(type => new {
					Type = type,
					Attribute = type.GetCustomAttribute<ServiceAttribute>(),
				})
				.Where(x => x.Attribute?.IsEnabled == true)
				.Select(x => {
					try {
						return (IService)UnityContainer.Resolve(x.Type);
					}
					catch (Exception ex){
						Console.WriteLine(ex);
						// TOOD: log error
					}

					return null;
				})
				.Where(x => x != null);

			ServiceList.AddRange(services);
		}

		public void StartAll()
		{
			foreach (var service in ServiceList) {
				service.Start();
			}
		}

		public void StopAll()
		{
			foreach (var service in ServiceList) {
				service.Stop();
			}
		}

		public void Register(IService service)
		{
			if (service == null) { throw new ArgumentNullException(nameof(service)); }
			ServiceList.Add(service);
		}

		public void Unregister(IService service)
		{
			if (service == null) { throw new ArgumentNullException(nameof(service)); }
			if (ServiceList.Remove(service)) {
				service.Dispose();
			}
		}

		public void UnregisterAll()
		{
			var services = ServiceList.ToArray();
			ServiceList.Clear();

			foreach (var service in services) {
				service.Dispose();
			}
		}
	}
}
