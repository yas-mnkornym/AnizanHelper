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
			var files = this.DictionaryManager.DictionaryFilePaths;
			var directories = files
				.Select(x => Path.GetDirectoryName(x))
				.Distinct()
				.ToArray();

			foreach (var directory in directories)
			{
				var watcher = new FileSystemWatcher(directory)
				{
					IncludeSubdirectories = false,
				}
				.AddTo(disposables);

				new[]
				{
					Observable.FromEventPattern<FileSystemEventArgs>(watcher, nameof(watcher.Changed)),
					Observable.FromEventPattern<FileSystemEventArgs>(watcher, nameof(watcher.Created)),
				}
				.Merge()
				.Where(x => files.Contains(x.EventArgs.FullPath))
				//.Select(x => x.EventArgs.FullPath)
				//.Buffer(this.ThrottlingInterval)
				//.SelectMany(x => x.Distinct())
				.Throttle(this.ThrottlingInterval)
				.Select(filePath => Observable.FromAsync(async () =>
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
				.AddTo(disposables);

				watcher.EnableRaisingEvents = true;
			}
		}
	}
}
