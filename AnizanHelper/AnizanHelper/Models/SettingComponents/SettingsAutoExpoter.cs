using System;
using System.IO;
using System.Reactive.Disposables;
using System.Reactive.Linq;

namespace AnizanHelper.Models.SettingComponents
{
	internal class SettingsAutoExpoter : IDisposable
	{
		#region Private Field
		private ISettingsSerializer serializer_;
		private Stream stream_;
		private CompositeDisposable disposables_ = new CompositeDisposable();
		#endregion

		public SettingsAutoExpoter(
			string filePath,
			string tempFilePath,
			SettingsImpl settings,
			ISettingsSerializer serializer,
			int delay = 300)
		{
			if (filePath == null) { throw new ArgumentNullException("filePath"); }
			if (tempFilePath == null) { throw new ArgumentNullException("tempFilePath"); }
			if (settings == null) { throw new ArgumentNullException("settings"); }
			if (delay < 0) { throw new ArgumentOutOfRangeException("dealy must be >= 0)"); }
			if (serializer == null) { throw new ArgumentNullException("serializer"); }
			this.serializer_ = serializer;

			// ファイルを開く
			this.stream_ = new FileStream(tempFilePath, FileMode.Create, FileAccess.Write, FileShare.Read);
			this.disposables_.Add(this.stream_);

			// 設定変更を監視
			var subscription = Observable.FromEventPattern<SettingChangeEventArgs>(settings, "SettingChanged")
				.Throttle(TimeSpan.FromMilliseconds(delay))
				.Subscribe(args =>
				{
					try
					{
						this.Export(filePath, tempFilePath, settings);
						if (Exported != null)
						{
							Exported(this, new EventArgs());
						}
					}
					catch (Exception ex)
					{
						if (Error != null)
						{
							Error(this, new ErrorEventArgs(ex));
						}
					}
				});
			this.disposables_.Add(subscription);
		}

		private void Export(
			string filePath,
			string tempFilePath,
			SettingsImpl settings
			)
		{
			if (filePath == null) { throw new ArgumentNullException("filePath"); }
			if (tempFilePath == null) { throw new ArgumentNullException("tempFilePath"); }
			if (settings == null) { throw new ArgumentNullException("settings"); }

			// ファイルを空にする
			this.stream_.SetLength(0);

			// 設定をシリアラズしてファイルに保存
			this.serializer_.Serialize(this.stream_, settings);

			// 一時ファイルから本来のファイルにコピーする
			File.Copy(tempFilePath, filePath, true);
		}

		public event EventHandler Exported;
		public event EventHandler<ErrorEventArgs> Error;

		#region IDisposable メンバ
		private bool isDisposed_ = false;
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed", MessageId = "stream_")]
		protected virtual void Dispose(bool disposing)
		{
			if (this.isDisposed_) { return; }
			if (disposing)
			{
				this.disposables_.Dispose();
				this.disposables_ = null;
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
