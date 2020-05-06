using System;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using AnizanHelper.Models;
using AnizanHelper.Models.Searching;
using AnizanHelper.Models.Searching.AnisonDb;
using AnizanHelper.ViewModels.Events;
using Prism.Events;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using Studiotaiha.LazyProperty;
using Xceed.Wpf.Toolkit;

namespace AnizanHelper.ViewModels.Pages
{
	internal class SongSearchPageViewModel : ReactiveViewModelBase
	{
		private ISongSearchProvider SongSearchProvider { get; }
		private Settings Settings { get; }
		private IEventAggregator EventAggregator { get; }

		public SongSearchPageViewModel(
			Settings settings,
			ISongSearchProvider songSearchProvider,
			IEventAggregator eventAggregator)
		{
			this.Settings = settings ?? throw new ArgumentNullException(nameof(settings));
			this.SongSearchProvider = songSearchProvider ?? throw new ArgumentNullException(nameof(songSearchProvider));
			this.EventAggregator = eventAggregator ?? throw new ArgumentNullException(nameof(eventAggregator));

			if (this.SongSearchProvider is AnisonDbSongNameSearchProvider anisonDbSearchProvider)
			{
				anisonDbSearchProvider.CheckSeries = this.Settings.CheckSeriesTypeNumberAutomatically;

				this.Settings
					.PropertyChangedAsObservable(nameof(this.Settings.CheckSeriesTypeNumberAutomatically))
					.Subscribe(_ =>
					{
						anisonDbSearchProvider.CheckSeries = this.Settings.CheckSeriesTypeNumberAutomatically;
					})
					.AddTo(this.Disposables);
			}

			this.EventAggregator
				.GetEvent<SearchSongEvent>()
				.Subscribe(
					async condition =>
					{
						this.SearchTerm.Value = condition.SearchTerm;
						await this.SearchAsync();
					},
					ThreadOption.UIThread,
					true)
				.AddTo(this.Disposables);

			this.EventAggregator
				.GetEvent<ClearSearchInputEvent>()
				.Subscribe(
					() =>
					{
						this.ClearInput();
					},
					ThreadOption.UIThread,
					true)
				.AddTo(this.Disposables);
		}

		#region ISearchController

		public void ClearInput()
		{
			this.SearchTerm.Value = null;
		}

		#endregion ISearchController

		private async Task SearchAsync()
		{
			try
			{
				var searchTerm = this.SearchTerm.Value.Trim();
				MessageService.Current.ShowMessage("楽曲情報を検索しています...");
				var sw = new Stopwatch();

				using (var cts = new CancellationTokenSource())
				{
					this.SearchCancellationTokenSource.Value = cts;

					try
					{
						this.Results.Clear();

						await foreach (var item in this.SongSearchProvider.SearchAsync(searchTerm, null, cts.Token))
						{
							this.Results.Add(item);
						}
					}
					finally
					{
						sw.Stop();
						this.SearchCancellationTokenSource.Value = null;
					}
				}

				MessageService.Current.ShowMessage(string.Format("検索完了({0}件, {1:0.000}秒)", this.Results.Count, sw.Elapsed.TotalSeconds));
			}
			catch (Exception ex)
			{
				MessageService.Current.ShowMessage(string.Format("楽曲情報の検索に失敗しました。 ({0})", ex.Message));
			}
		}

		#region Bindings

		public ReactiveProperty<bool> CheckSeriesTypeNumberAutomatically => this.LazyReactiveProperty(() =>
		{
			return this.Settings.ToReactivePropertyAsSynchronized(x => x.CheckSeriesTypeNumberAutomatically);
		});

		public ReadOnlyReactiveProperty<bool> IsSearching => this.LazyReadOnlyReactiveProperty(() =>
		{
			return this.SearchCancellationTokenSource
				.Select(x => x != null)
				.ToReadOnlyReactiveProperty();
		});

		public ReactiveCollection<ISongSearchResult> Results { get; } = new ReactiveCollection<ISongSearchResult>();
		public ReactiveProperty<CancellationTokenSource> SearchCancellationTokenSource { get; } = new ReactiveProperty<CancellationTokenSource>();
		public ReactiveProperty<string> SearchTerm { get; } = new ReactiveProperty<string>();

		#endregion Bindings

		#region Commands

		public ICommand ApplySongCommand => this.LazyAsyncReactiveCommand<SongSearchResult>(
			async result =>
			{
				if (result == null) { return; }

				try
				{
					var songInfo = await this.SongSearchProvider.ConvertToGeneralSongInfoAsync(result);

					this.EventAggregator
						.GetEvent<SongParsedEvent>()
						.Publish(songInfo);
				}
				catch (Exception ex)
				{
					MessageBox.Show(string.Format("曲情報の適用に失敗しました。\n\n【例外情報】\n{0}", ex), "エラー", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Stop);
				}
			});

		public ICommand CancelSearchCommand => this.LazyReactiveCommand(
			this.SearchCancellationTokenSource.Select(x => x != null),
			() =>
			{
				using (var cts = this.SearchCancellationTokenSource.Value)
				{
					this.SearchCancellationTokenSource.Value = null;
					cts.Cancel();
				}
			});

		public ICommand SearchCommand => this.LazyAsyncReactiveCommand(
			new[]{
				this.SearchTerm.Select(x => !string.IsNullOrWhiteSpace(x)),
				this.IsSearching.Select(x => !x),
			}
			.CombineLatestValuesAreAllTrue(),
			async () =>
			{
				try
				{
					await this.SearchAsync();
				}
				catch (Exception ex)
				{
					MessageBox.Show(string.Format("検索に失敗しました。\n\n【例外情報】\n{0}", ex), "検索失敗", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Stop);
				}
			});

		public ICommand SearchOnBrowserCommand => this.LazyReactiveCommand(
			this.SearchTerm.Select(x => !string.IsNullOrWhiteSpace(x)),
			() =>
			{
				try
				{
					var searchTerm = this.SearchTerm.Value;
					if (this.SongSearchProvider is AnisonDbSongNameSearchProvider anisonDbSearchProvider)
					{
						var uri = anisonDbSearchProvider.CreateQueryUri(searchTerm, "song");
						Process.Start(uri.AbsoluteUri);
					}
				}
				catch (Exception ex)
				{
					MessageBox.Show(string.Format("検索ページのオープンに失敗しました。\n\n【例外情報】\n{0}", ex), "検索失敗", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Stop);
				}
			});

		#endregion Commands
	}
}
