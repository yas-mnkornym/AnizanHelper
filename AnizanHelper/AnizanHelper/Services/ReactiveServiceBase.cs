using System;
using System.Reactive.Disposables;

namespace AnizanHelper.Services
{
	abstract class ReactiveServiceBase : IService
	{
		private CompositeDisposable Disposables { get; set; }

		public bool IsRunning => Disposables != null;

		public virtual void Start()
		{
			if (Disposables == null) {
				Disposables = new CompositeDisposable();

				RegisterDisposables(Disposables);
			}
		}

		public virtual void Stop()
		{
			if (Disposables != null) {
				Disposables.Dispose();
				Disposables = null;
			}
		}

		protected abstract void RegisterDisposables(CompositeDisposable disposables);

		#region IDisposable

		bool isDisposed_ = false;
		virtual protected void Dispose(bool disposing)
		{
			if (isDisposed_) { return; }
			if (disposing) {
				Stop();
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
