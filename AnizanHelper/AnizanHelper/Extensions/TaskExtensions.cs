using System;
using System.Threading.Tasks;

namespace AnizanHelper
{
	public static class TaskExtensions
	{
		public static void FireAndForget(
			this Task task,
			Action<Exception> exceptionHandler = null)
		{
			task
				.ContinueWith(x =>
				{
					exceptionHandler?.Invoke(x.Exception);
				},
				TaskContinuationOptions.OnlyOnFaulted);
		}
	}
}
