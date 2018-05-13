using System.Collections.Generic;

namespace AnizanHelper.Services
{
	public interface IServiceManager
	{
		IEnumerable<IService> Services { get; }

		void StartAll();
		void StopAll();

		void Register(IService service);
		void Unregister(IService service);
		void UnregisterAll();
	}
}