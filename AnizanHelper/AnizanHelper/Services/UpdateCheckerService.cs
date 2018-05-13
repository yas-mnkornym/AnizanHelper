using System;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using AnizanHelper.Models;
using AnizanHelper.Models.Updating;
using Reactive.Bindings.Extensions;

namespace AnizanHelper.Services
{
	[Service]
	class UpdateCheckerService : ReactiveServiceBase
	{
		static TimeSpan CheckInterval { get; } = TimeSpan.FromMinutes(10);

		IUpdateManager UpdateManager { get; }
		Settings Settings { get; }

		public UpdateCheckerService(
			IUpdateManager updateManager,
			Settings settings)
		{
			UpdateManager = updateManager ?? throw new ArgumentNullException(nameof(updateManager));
			Settings = settings ?? throw new ArgumentNullException(nameof(settings));
		}

		public async Task CheckForUpdateAndShowDialogIfAvailableAsync()
		{
			await UpdateManager.CheckForUpdateAsync();

			var sb = new StringBuilder();
			sb.AppendLine("アニソンDBぱーさーのアップデートがあります。");
			sb.AppendLine("Webサイトを開きますか？");

			var updateInfo = UpdateManager.UpdateInfo;
			if (updateInfo != null) {
				sb.AppendLine();
				sb.AppendLine("最新バージョン: {0}", updateInfo.Version);
			}

			sb.AppendLine();
			sb.AppendLine("※ アップデートをダウンロードしてファイルを上書きしてください。");

			if(MessageBox.Show(sb.ToString(), "アップデート確認", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.OK) == MessageBoxResult.Yes){
				Process.Start(Constants.AnizanHelperWebSiteUrl);
			}
		}

		protected override void RegisterDisposables(CompositeDisposable disposables)
		{
			Observable.Timer(TimeSpan.Zero, CheckInterval)
				.Where(_ => Settings.CheckForUpdateAutomatically)
				.ObserveOnDispatcher()
				.SelectMany(async _ => {
					try {
						await CheckForUpdateAndShowDialogIfAvailableAsync().ConfigureAwait(false);
					}
					catch (Exception ex) {
						// TODO: Log error
						Console.WriteLine(ex);
					}
					return true;
				})
				.Subscribe()
				.AddTo(disposables);
		}
	}
}
