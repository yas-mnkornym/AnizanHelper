using System;
using System.Threading;
using System.Windows.Threading;
using Studiotaiha.Toolkit;

namespace AnizanHelper.Models.SettingComponents
{
	internal class WPFDispatcher : IDispatcher
	{
		private Dispatcher dispatcher_;

		public WPFDispatcher(Dispatcher dispatcher)
		{
			if (dispatcher == null) { throw new ArgumentNullException("dispatcher"); }
			this.dispatcher_ = dispatcher;
		}

		public void Dispatch(Action act)
		{
			if (Thread.CurrentThread.ManagedThreadId != this.dispatcher_.Thread.ManagedThreadId)
			{
				this.dispatcher_.Invoke(act);
			}
			else
			{
				act();
			}
		}

		public T Dispatch<T>(Func<T> func)
		{
			if (Thread.CurrentThread.ManagedThreadId != this.dispatcher_.Thread.ManagedThreadId)
			{
				return (T)this.dispatcher_.Invoke(func);
			}
			else
			{
				return func();
			}
		}

		public void BeginDispatch(
			Action act,
			Action onCompleted = null,
			Action onAborted = null)
		{
			var ret = this.dispatcher_.BeginInvoke(act);
			ret.Completed += (_, __) =>
			{
				onCompleted();
			};
			ret.Aborted += (_, __) =>
			{
				onAborted();
			};
		}

		public void BeginDispatch<T>(
			Func<T> func,
			Action<T> onCompleted = null,
			Action onAborted = null)
		{
			var ret = this.dispatcher_.BeginInvoke(func);
			ret.Completed += (_, __) =>
			{
				onCompleted((T)ret.Result);
			};
			ret.Aborted += (_, __) =>
			{
				onAborted();
			};
		}
	}
}
