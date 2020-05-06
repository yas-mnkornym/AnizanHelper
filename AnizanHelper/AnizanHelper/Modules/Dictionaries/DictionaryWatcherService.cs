using System;
using System.IO;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using AnizanHelper.Models;
using AnizanHelper.Services;
using Reactive.Bindings.Extensions;

namespace AnizanHelper.Modules.Dictionaries
{
	[Service]
	public class DictionaryWatcherService : ReactiveServiceBase
	{
		private IDictionaryManager DictionaryManager { get; }
		private TimeSpan ThrottlingInterval { get; } = TimeSpan.FromMilliseconds(250);

		public DictionaryWatcherService(IDictionaryManager dictionaryManager)
		{
			this.DictionaryManager = dictionaryManager ?? throw new ArgumentNullException(nameof(dictionaryManager));
		}

		protected override void RegisterDisposables(CompositeDisposable disposables)
		{
			foreach (var filePath in this.DictionaryManager.DictionaryFilePaths)
			{
				var watcher = new FileSystemWatcher(filePath)
					.AddTo(disposables);

				new[]
				{
					Observable.FromEventPattern<FileSystemEventArgs>(watcher, nameof(watcher.Changed)),
					Observable.FromEventPattern<FileSystemEventArgs>(watcher, nameof(watcher.Created)),
				}
				.Merge()
				.Throttle(this.ThrottlingInterval)
				.Select(x => Observable.FromAsync(async () =>
				{
					try
					{
						await this.DictionaryManager.LoadAsync().ConfigureAwait(false);
					}
					catch (Exception ex)
					{
						Console.WriteLine("Failed to load dictionary: {0}", filePath);
						Console.WriteLine(ex);
						MessageService.Current.ShowMessage("辞書ファイルの読み込みに失敗しました。");
					}
				}))
				.Merge(1)
				.Subscribe()
				.Dispose();
			}
		}
	}
}
