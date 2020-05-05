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

		ICollection<IDisposable> IReactiveLazyPropertyHolder.Disposables => this.Disposables;

		#region IDisposable

		private bool isDisposed_ = false;

		public ReactiveViewModelBase(IDispatcher dispatcher = null) : base(dispatcher)
		{
		}

		public ReactiveViewModelBase(IDispatcher dispatcher, bool enableAutoDispatch) : base(dispatcher, enableAutoDispatch)
		{
		}

		protected virtual void Dispose(bool disposing)
		{
			if (this.isDisposed_) { return; }
			if (disposing)
			{
				this.Disposables.Dispose();
			}
			this.isDisposed_ = true;
		}

		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		#endregion
	}
}
