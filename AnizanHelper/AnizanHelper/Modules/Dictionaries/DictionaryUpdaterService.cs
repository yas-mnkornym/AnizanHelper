using System;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using AnizanHelper.Models;
using AnizanHelper.Services;
using Reactive.Bindings.Extensions;

namespace AnizanHelper.Modules.Dictionaries
{
	[Service]
	public class DictionaryUpdaterService : ReactiveServiceBase, IDictionaryUpdaterService
	{
		private static TimeSpan CheckInterval { get; } = TimeSpan.FromMinutes(3);
		private Models.Settings Settings { get; }

		public DictionaryUpdaterService(Models.Settings settings)
		{
			this.Settings = settings ?? throw new ArgumentNullException(nameof(settings));
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
							await this.CheckForUpdateAsync().ConfigureAwait(false);
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

		public async Task CheckForUpdateAsync(CancellationToken cancellationToken = default)
		{
			MessageService.Current.ShowMessage("辞書の更新を確認しています...");
			try
			{
				var updates = await this.CheckForDictionaryUpdateAsync().ConfigureAwait(false);
				if (updates.Length == 0)
				{
					MessageService.Current.ShowMessage("新しい辞書はありませんでした。");
					return;
				}

				StringBuilder sb = new StringBuilder();
				sb.AppendLine("新しい辞書があります。更新しますか？");
				sb.AppendLine();
				sb.AppendLine("*** 更新情報 ***");
				foreach (var info in updates.OrderByDescending(x => x.Version))
				{
					sb.AppendFormat("【Version {0}】- {1}", info.Version, info.Date.ToString("yyyy年MM月dd日 HH時mm分"));
					sb.AppendLine();
					sb.AppendLine(info.Description);
				}
				var ret = MessageBox.Show(App.Current.MainWindow, sb.ToString(), "確認", MessageBoxButton.YesNo, MessageBoxImage.Question);
				if (ret != MessageBoxResult.Yes)
				{
					MessageService.Current.ShowMessage("辞書の更新がキャンセルされました。");
					return;
				}

				MessageService.Current.ShowMessage("最新の辞書を取得しています...");
				var updater = new ReplaceDictionaryUpdater();
				var newDic = updater.DownloadDictionary();

				MessageService.Current.ShowMessage("辞書を更新しています...");
				File.WriteAllText(AppInfo.Current.DictionaryFilePath, newDic, Encoding.UTF8);

				MessageService.Current.ShowMessage("辞書の更新が完了しました！");
			}
			catch (Exception ex)
			{
				MessageService.Current.ShowMessage("更新に失敗しました。(" + ex.Message + ")");
			}
		}

		private async Task<ReplaceDictionaryUpdateInfo[]> CheckForDictionaryUpdateAsync()
		{
			var currentVersion = 0;
			var filePath = AppInfo.Current.DictionaryFilePath;
			if (File.Exists(filePath))
			{
				var reader = new DictionaryFileReader();
				try
				{
					currentVersion = reader.GetVersionNumber(filePath);
				}
				catch { }
			}

			var updater = new ReplaceDictionaryUpdater();
			var latestVersion = updater.GetLatestVersionNumber();
			if (latestVersion <= currentVersion)
			{
				return Array.Empty<ReplaceDictionaryUpdateInfo>();
			}

			var updates = await updater.GetUpdateInfoAsync().ConfigureAwait(false);
			return updates
				.Where(x => x.Version > currentVersion)
				.ToArray();
		}
	}
}
