using System;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using AnizanHelper.Models;
using AnizanHelper.Models.DbSearch;
using AnizanHelper.ViewModels.Events;
using Prism.Events;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using Studiotaiha.LazyProperty;
using Xceed.Wpf.Toolkit;

namespace AnizanHelper.ViewModels.Pages
{
	internal class SongSearchPageViewModel : ReactiveViewModelBase, ISearchController
	{
		public SongSearcher Searcher { get; } = new SongSearcher();
		private Settings Settings { get; }
		private IEventAggregator EventAggregator { get; }

		public SongSearchPageViewModel(
			Settings settings,
			ProxySearchController proxySearchController,
			IEventAggregator eventAggregator)
		{
			this.Settings = settings ?? throw new ArgumentNullException(nameof(settings));
			if (proxySearchController == null) { throw new ArgumentNullException(nameof(proxySearchController)); }
			this.EventAggregator = eventAggregator ?? throw new ArgumentNullException(nameof(eventAggregator));

			proxySearchController.Target = this;

			this.EventAggregator
				.GetEvent<ClearSearchInputEvent>()
				.Subscribe(() =>
				{
					this.ClearInput();
				})
				.AddTo(this.Disposables);
		}

		#region ISearchController

		public void ClearInput()
		{
			this.SearchTerm.Value = null;
		}

		public async void TriggerSearch(string searchTerm)
		{
			this.SearchTerm.Value = searchTerm;
			await this.SearchAsync();
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
						var searchResultItems = await this.Searcher.SearchAsync(searchTerm, cts.Token);
						sw.Stop();

						this.Results.Clear();
						this.Results.AddRangeOnScheduler(searchResultItems);
					}
					finally
					{
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

		public ReactiveCollection<SongSearchResult> Results { get; } = new ReactiveCollection<SongSearchResult>();
		public ReactiveProperty<CancellationTokenSource> SearchCancellationTokenSource { get; } = new ReactiveProperty<CancellationTokenSource>();
		public ReactiveProperty<string> SearchTerm { get; } = new ReactiveProperty<string>();

		#endregion Bindings

		#region Commands

		public ICommand ApplySongCommand => this.LazyReactiveCommand<SongSearchResult>(
			result =>
			{
				if (result == null) { return; }

				try
				{
					var songInfo = SongSearchResult.ToGeneralInfo(result, this.Settings.CheckSeriesTypeNumberAutomatically);
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
					var url = this.Searcher.CreateQueryUrl(searchTerm, SongSearcher.SearchType);
					Process.Start(url);
				}
				catch (Exception ex)
				{
					MessageBox.Show(string.Format("検索ページのオープンに失敗しました。\n\n【例外情報】\n{0}", ex), "検索失敗", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Stop);
				}
			});

		#endregion Commands
	}
}
