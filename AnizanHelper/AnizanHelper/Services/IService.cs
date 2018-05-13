using System;

namespace AnizanHelper.Services
{
	public interface IService : IDisposable
	{
		bool IsRunning { get; }
		void Start();
		void Stop();
	}
}
