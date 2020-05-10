using System;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using AnizanHelper.Models;
using AnizanHelper.Models.Updating;
using Reactive.Bindings.Extensions;

namespace AnizanHelper.Services
{
	[Service]
	internal class UpdateCheckerService : ReactiveServiceBase
	{
		private static TimeSpan CheckInterval { get; } = TimeSpan.FromMinutes(10);

		private IUpdateManager UpdateManager { get; }
		private Settings Settings { get; }
		private Version IgnoreVersion { get; set; } = AppInfo.Current.Version;

		public UpdateCheckerService(
			IUpdateManager updateManager,
			Settings settings)
		{
			this.UpdateManager = updateManager ?? throw new ArgumentNullException(nameof(updateManager));
			this.Settings = settings ?? throw new ArgumentNullException(nameof(settings));
		}

		public async Task<bool?> CheckForUpdateAndShowDialogIfAvailableAsync(bool autoIgnore = true)
		{
			await this.UpdateManager.CheckForUpdateAsync();
			var updateInfo = this.UpdateManager.UpdateInfo;

			if (updateInfo.Version <= AppInfo.Current.Version)
			{
				return false;
			}

			if (autoIgnore && updateInfo.Version <= this.IgnoreVersion)
			{
				return null;
			}

			var sb = new StringBuilder();
			sb.AppendLine("アニソンDBぱーさーのアップデートがあります。");
			sb.AppendLine("Webサイトを開きますか？");

			if (updateInfo != null)
			{
				sb.AppendLine();
				sb.AppendLine("最新バージョン: {0}", updateInfo.Version);

				var releaseNote = updateInfo.ReleaseNotes
					?.FirstOrDefault(x => x.Version == updateInfo.Version);

				if (releaseNote != null)
				{
					sb.AppendLine();
					sb.AppendLine("【更新内容】");

					foreach (var message in releaseNote.Messages)
					{
						sb.AppendLine($" - {message}");
					}
				}
			}

			sb.AppendLine();
			sb.AppendLine("※ アップデートをダウンロードしてファイルを上書きしてください。");
			sb.AppendLine();
			sb.AppendLine("※ キャンセル を選択すると、次回起動時までこのバージョンは通知されなくなります。");

			var ret = MessageBox.Show(App.Current.MainWindow, sb.ToString(), "アップデート確認", MessageBoxButton.YesNoCancel, MessageBoxImage.Question, MessageBoxResult.OK);
			if (ret == MessageBoxResult.Yes)
			{
				Process.Start(Constants.AnizanHelperWebSiteUrl);
			}
			else if (ret == MessageBoxResult.Cancel)
			{
				this.IgnoreVersion = updateInfo.Version;
			}

			return true;
		}

		protected override void RegisterDisposables(CompositeDisposable disposables)
		{
			int checkGate = 0;
			Observable.Timer(TimeSpan.Zero, CheckInterval)
				.Where(_ => this.Settings.CheckForUpdateAutomatically)
				.ObserveOnDispatcher()
				.SelectMany(async _ =>
				{
					if (Interlocked.CompareExchange(ref checkGate, 1, 0) == 0)
					{
						try
						{
							await this.CheckForUpdateAndShowDialogIfAvailableAsync().ConfigureAwait(false);
						}
						catch (Exception ex)
						{
							// TODO: Log error
							Console.WriteLine(ex);
						}
						finally
						{
							Volatile.Write(ref checkGate, 0);
						}
					}

					return Unit.Default;
				})
				.Subscribe()
				.AddTo(disposables);
		}
	}
}
