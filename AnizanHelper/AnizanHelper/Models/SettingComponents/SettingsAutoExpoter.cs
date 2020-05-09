using System;
using System.IO;
using System.Reactive.Disposables;
using System.Reactive.Linq;

namespace AnizanHelper.Models.SettingComponents
{
	internal sealed class SettingsAutoExpoter : IDisposable
	{
		private ISettingsSerializer Serializer { get; }
		private Stream Stream { get; }
		private CompositeDisposable Disposables { get; } = new CompositeDisposable();

		public SettingsAutoExpoter(
			string filePath,
			string tempFilePath,
			SettingsContainer settings,
			ISettingsSerializer serializer,
			TimeSpan throttlingInterval)
		{
			if (filePath == null) { throw new ArgumentNullException(nameof(filePath)); }
			if (tempFilePath == null) { throw new ArgumentNullException(nameof(tempFilePath)); }
			if (settings == null) { throw new ArgumentNullException(nameof(settings)); }
			this.Serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));

			// ファイルを開く
			this.Stream = new FileStream(tempFilePath, FileMode.Create, FileAccess.Write, FileShare.Read);
			this.Disposables.Add(this.Stream);

			// 設定変更を監視
			var subscription = Observable.FromEventPattern<SettingChangeEventArgs>(settings, "SettingChanged")
				.Throttle(throttlingInterval)
				.Subscribe(args =>
				{
					try
					{
						this.Export(filePath, tempFilePath, settings);
						this.Exported?.Invoke(this, new EventArgs());
					}
					catch (Exception ex)
					{
						this.Error?.Invoke(this, new ErrorEventArgs(ex));
					}
				});

			this.Disposables.Add(subscription);
		}

		private void Export(
			string filePath,
			string tempFilePath,
			SettingsContainer settings)
		{
			if (filePath == null) { throw new ArgumentNullException(nameof(filePath)); }
			if (tempFilePath == null) { throw new ArgumentNullException(nameof(tempFilePath)); }
			if (settings == null) { throw new ArgumentNullException(nameof(settings)); }

			// ファイルを空にする
			this.Stream.SetLength(0);

			// 設定をシリアラズしてファイルに保存
			this.Serializer.Serialize(this.Stream, settings);

			// 一時ファイルから本来のファイルにコピーする
			File.Copy(tempFilePath, filePath, true);
		}

		public event EventHandler Exported;

		public event EventHandler<ErrorEventArgs> Error;

		#region IDisposable

		private bool isDisposed = false;

		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		private void Dispose(bool disposing)
		{
			if (this.isDisposed) { return; }

			if (disposing)
			{
				this.Disposables.Dispose();
			}

			this.isDisposed = true;
		}

		private void ThrowIfDisposed()
		{
			if (this.isDisposed)
			{
				throw new ObjectDisposedException(this.ToString());
			}
		}

		#endregion IDisposable
	}
}
