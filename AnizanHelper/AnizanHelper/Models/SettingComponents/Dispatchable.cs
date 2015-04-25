using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AnizanHelper.Models.SettingComponents
{
	public class Dispatchable
	{
		public IDispatcher Dispatcher { get; protected set; }

		public Dispatchable(IDispatcher dispatcher = null)
		{
			Dispatcher = dispatcher;
		}

		protected void Dispatch(Action act)
		{
			if (act == null) { throw new ArgumentNullException("act"); }
			if (Dispatcher != null) {
				Dispatcher.Dispatch(act);
			}
			else {
				act();
			}
		}

		protected T Dispatch<T>(Func<T> func)
		{
			if (func == null) { throw new ArgumentNullException("func"); }
			if (Dispatcher != null) {
				return Dispatcher.Dispatch(func);
			}
			else {
				return func();
			}
		}

		protected void BeginDispatch(
			Action act,
			Action onCompleted = null,
			Action onAborted = null)
		{
			if (act == null) { throw new ArgumentNullException("act"); }
			if (Dispatcher != null) {
				Dispatcher.BeginDispatch(act, onCompleted, onAborted);
			}
			else {
				act();
			}
		}
		void BeginDispatch<T>(
			Func<T> func,
			Action<T> onCompleted = null,
			Action onAborted = null)
		{
			if (func == null) { throw new ArgumentNullException("func"); }
			if (Dispatcher != null) {
				Dispatcher.BeginDispatch(func, onCompleted, onAborted);
			}
			else {
				var ret = func();
				onCompleted(ret);
			}
		}
	}
}
