using System;
using System.Threading;
using System.Windows.Threading;
using Studiotaiha.Toolkit;

namespace AnizanHelper.Models.SettingComponents
{
	class WPFDispatcher : IDispatcher
	{
		Dispatcher dispatcher_;

		public WPFDispatcher(Dispatcher dispatcher)
		{
			if (dispatcher == null) { throw new ArgumentNullException("dispatcher"); }
			dispatcher_ = dispatcher;
		}

		public void Dispatch(Action act)
		{
			if (Thread.CurrentThread.ManagedThreadId != dispatcher_.Thread.ManagedThreadId) {
				dispatcher_.Invoke(act);
			}
			else {
				act();
			}
		}

		public T Dispatch<T>(Func<T> func)
		{
			if (Thread.CurrentThread.ManagedThreadId != dispatcher_.Thread.ManagedThreadId) {
				return (T)dispatcher_.Invoke(func);
			}
			else {
				return func();
			}
		}

		public void BeginDispatch(
			Action act,
			Action onCompleted = null,
			Action onAborted = null)
		{
			var ret = dispatcher_.BeginInvoke(act);
			ret.Completed += (_, __) => {
				onCompleted();
			};
			ret.Aborted += (_, __) => {
				onAborted();
			};
		}

		public void BeginDispatch<T>(
			Func<T> func,
			Action<T> onCompleted = null,
			Action onAborted = null)
		{
			var ret = dispatcher_.BeginInvoke(func);
			ret.Completed += (_, __) => {
				onCompleted((T)ret.Result);
			};
			ret.Aborted += (_, __) => {
				onAborted();
			};
		}
	}
}
