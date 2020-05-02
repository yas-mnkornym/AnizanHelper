using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
using Studiotaiha.LazyProperty;
using Studiotaiha.Toolkit;

namespace AnizanHelper.ViewModels
{
	public class ReactiveViewModelBase : ViewModelBase, IDisposable, IReactiveLazyPropertyHolder
	{
		protected CompositeDisposable Disposables { get; } = new CompositeDisposable();

		ICollection<IDisposable> IReactiveLazyPropertyHolder.Disposables => Disposables;

		#region IDisposable

		bool isDisposed_ = false;

		public ReactiveViewModelBase(IDispatcher dispatcher = null) : base(dispatcher)
		{
		}

		public ReactiveViewModelBase(IDispatcher dispatcher, bool enableAutoDispatch) : base(dispatcher, enableAutoDispatch)
		{
		}

		virtual protected void Dispose(bool disposing)
		{
			if (isDisposed_) { return; }
			if (disposing) {
				Disposables.Dispose();
			}
			isDisposed_ = true;
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		#endregion
	}
}
