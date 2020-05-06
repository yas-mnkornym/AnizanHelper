using System;
using System.Reactive.Disposables;
using System.Threading;
using System.Threading.Tasks;
using Reactive.Bindings.Extensions;

namespace AnizanHelper.Services
{
	internal abstract class SingleRunServiceBase : IService
	{
		public bool IsRunning => Volatile.Read(ref this.state) == StateValues.Running;
		private CancellationTokenSource CancellationTokenSource { get; }
		private CompositeDisposable Disposables { get; set; }

		private Task serviceTask;
		private int state = StateValues.NotStarted;

		public SingleRunServiceBase()
		{
			this.CancellationTokenSource = new CancellationTokenSource().AddTo(this.Disposables);
		}

		public void Start()
		{
			if (Interlocked.CompareExchange(ref this.state, StateValues.Running, StateValues.NotStarted) == 0)
			{
				try
				{
					this.serviceTask = Task.Run(() => this.RunAsync(this.CancellationTokenSource.Token));
				}
				catch
				{
					Volatile.Write(ref this.state, StateValues.Broken);
				}
			}
		}

		public void Stop()
		{
			if (Interlocked.CompareExchange(ref this.state, StateValues.Finished, StateValues.Running) == 1)
			{
				this.CancellationTokenSource.Cancel();
				this.serviceTask.Wait();
			}
		}

		protected abstract Task RunAsync(CancellationToken cancellationToken);

		private static class StateValues
		{
			public static int Broken { get; } = 2;
			public static int Finished { get; } = 2;
			public static int NotStarted { get; } = 0;
			public static int Running { get; } = 1;
		}

		#region IDisposable

		private bool isDisposed = false;

		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (this.isDisposed) { return; }

			if (disposing)
			{
				this.Disposables.Dispose();
			}

			this.isDisposed = true;
		}

		protected virtual string GetObjectName()
		{
			return this.ToString();
		}

		protected void ThrowIfDisposed()
		{
			if (this.isDisposed)
			{
				throw new ObjectDisposedException(this.GetObjectName());
			}
		}

		#endregion IDisposable
	}
}
