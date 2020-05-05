using System;
using System.Reactive.Disposables;

namespace AnizanHelper.Services
{
	internal abstract class ReactiveServiceBase : IService
	{
		private CompositeDisposable Disposables { get; set; }

		public bool IsRunning => this.Disposables != null;

		public virtual void Start()
		{
			if (this.Disposables == null)
			{
				this.Disposables = new CompositeDisposable();

				this.RegisterDisposables(this.Disposables);
			}
		}

		public virtual void Stop()
		{
			if (this.Disposables != null)
			{
				this.Disposables.Dispose();
				this.Disposables = null;
			}
		}

		protected abstract void RegisterDisposables(CompositeDisposable disposables);

		#region IDisposable

		private bool isDisposed_ = false;
		protected virtual void Dispose(bool disposing)
		{
			if (this.isDisposed_) { return; }
			if (disposing)
			{
				this.Stop();
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
