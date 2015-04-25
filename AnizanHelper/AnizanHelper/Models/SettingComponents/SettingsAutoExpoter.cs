using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;

namespace AnizanHelper.Models.SettingComponents
{
	internal class SettingsAutoExpoter : IDisposable
	{
		#region Private Field
		ISettingsSerializer serializer_;
		Stream stream_;
		CompositeDisposable disposables_ = new CompositeDisposable();
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
			serializer_ = serializer;

			// ファイルを開く
			stream_ = new FileStream(tempFilePath, FileMode.Create, FileAccess.Write, FileShare.Read);
			disposables_.Add(stream_);

			// 設定変更を監視
			var subscription = Observable.FromEventPattern<SettingChangeEventArgs>(settings, "SettingChanged")
				.Throttle(TimeSpan.FromMilliseconds(delay))
				.Subscribe(args => {
					try {
						Export(filePath, tempFilePath, settings);
						if (Exported != null) {
							Exported(this, new EventArgs());
						}
					}
					catch (Exception ex) {
						if (Error != null) {
							Error(this, new ErrorEventArgs(ex));
						}
					}
				});
			disposables_.Add(subscription);
		}

		void Export(
			string filePath,
			string tempFilePath,
			SettingsImpl settings
			)
		{
			if (filePath == null) { throw new ArgumentNullException("filePath"); }
			if (tempFilePath == null) { throw new ArgumentNullException("tempFilePath"); }
			if (settings == null) { throw new ArgumentNullException("settings"); }

			// ファイルを空にする
			stream_.SetLength(0);

			// 設定をシリアラズしてファイルに保存
			serializer_.Serialize(stream_, settings);

			// 一時ファイルから本来のファイルにコピーする
			File.Copy(tempFilePath, filePath, true);
		}

		public event EventHandler Exported;
		public event EventHandler<ErrorEventArgs> Error;

		#region IDisposable メンバ
		bool isDisposed_ = false;
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed", MessageId = "stream_")]
		virtual protected void Dispose(bool disposing)
		{
			if (isDisposed_) { return; }
			if (disposing) {
				disposables_.Dispose();
				disposables_ = null;
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
