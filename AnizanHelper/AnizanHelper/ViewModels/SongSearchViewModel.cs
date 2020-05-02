using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AnizanHelper.Models;
using AnizanHelper.Models.DbSearch;
using AnizanHelper.Models.SettingComponents;
using Xceed.Wpf.Toolkit;

namespace AnizanHelper.ViewModels
{
	internal class SongSearchViewModel : SongParserVmBase
	{
		SongSearcher searcher_ = new SongSearcher();

		public SongSearchViewModel(
			Settings settings,
			ISearchManager searchManager,
			Studiotaiha.Toolkit.IDispatcher dispatcher)
			: base(dispatcher)
		{
			if (settings == null) { throw new ArgumentNullException("settings"); }
			Settings = settings;

			if (searchManager is SearchManager castedSearchManager)
			{
				castedSearchManager.SearchTriggered += async (_, searchTerm) =>
				{
					if (string.IsNullOrWhiteSpace(searchTerm))
					{
						return;
					}

					this.SearchWord = searchTerm;
					await this.SearchAsync();
				};
			}
		}


		async Task SearchAsync()
		{
			var words = SearchWord.Trim();

			MessageService.Current.ShowMessage("楽曲情報を検索しています...");
			var sw = new Stopwatch();

			try
			{
				IsSearching = true;
				CancellationTokenSource = new CancellationTokenSource();
				CancelSearchingCommand.RaiseCanExecuteChanged();

				var searchResultItems = await searcher_.SearchAsync(words, CancellationTokenSource.Token);
				sw.Stop();

				Results = new ObservableCollection<SongSearchResult>(searchResultItems);
				MessageService.Current.ShowMessage(string.Format("検索完了({0}件, {1:0.000}秒)", Results.Count, sw.Elapsed.TotalSeconds));
			}
			catch (Exception ex)
			{
				MessageService.Current.ShowMessage(string.Format("楽曲情報の検索に失敗しました。 ({0})", ex.Message));
			}
			finally
			{
				if (CancellationTokenSource != null)
				{
					CancellationTokenSource.Dispose();
					CancellationTokenSource = null;
				}
				IsSearching = false;

				CancelSearchingCommand.RaiseCanExecuteChanged();
			}
		}

		public override void ClearInput()
		{
			SearchWord = null;
		}

		#region Bindings
		#region SearchWord
		string searchWord_ = "";
		public string SearchWord
		{
			get
			{
				return searchWord_;
			}
			set
			{
				if (SetValue(ref searchWord_, value)) {
					SearchCommand.RaiseCanExecuteChanged();
					SearchOnBrowserCommand.RaiseCanExecuteChanged();
				}
			}
		}
		#endregion

		#region Results
		ObservableCollection<SongSearchResult> results_ = new ObservableCollection<SongSearchResult>();
		public ObservableCollection<SongSearchResult> Results
		{
			get
			{
				return results_;
			}
			set
			{
				SetValue(ref results_, value);
			}
		}
		#endregion

		#region IsSearching
		bool isSearching_ = false;
		public bool IsSearching
		{
			get
			{
				return isSearching_;
			}
			set
			{
				if (SetValue(ref isSearching_, value)) {
					Dispatch(() => {
						SearchCommand.RaiseCanExecuteChanged();
					});
				}
			}
		}
		#endregion

		public Settings Settings
		{
			get
			{
				return GetValue<Settings>();
			}
			set
			{
				SetValue(value);
			}
		}

		public CancellationTokenSource CancellationTokenSource
		{
			get
			{
				return GetValue<CancellationTokenSource>();
			}
			set
			{
				SetValue(value);
			}
		}
		
		#endregion // Bindigns

		#region Commands
		#region SearchCommand
		DelegateCommand searchCommand_ = null;
		public DelegateCommand SearchCommand
		{
			get
			{
				return searchCommand_ ?? (searchCommand_ = new DelegateCommand {
					ExecuteHandler = async param => {
						try {
							await SearchAsync();
						}
						catch (Exception ex) {
							MessageBox.Show(string.Format("検索に失敗しました。\n\n【例外情報】\n{0}", ex), "検索失敗", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Stop);
						}
					},
					CanExecuteHandler = param => {
						return (!string.IsNullOrWhiteSpace(SearchWord) && !IsSearching);
					}
				});
			}
		}
		#endregion

		#region SearchOnBrowserCommand
		DelegateCommand searchOnBrowserCommand_ = null;
		public DelegateCommand SearchOnBrowserCommand
		{
			get
			{
				return searchOnBrowserCommand_ ?? (searchOnBrowserCommand_ = new DelegateCommand {
					ExecuteHandler = param => {
						try {
							var url = searcher_.CreateQueryUrl(SearchWord, SongSearcher.SearchType);
							Process.Start(url);
						}
						catch (Exception ex) {
							MessageBox.Show(string.Format("検索ページのオープンに失敗しました。\n\n【例外情報】\n{0}", ex), "検索失敗", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Stop);
						}
					},
					CanExecuteHandler = param => {
						return !string.IsNullOrWhiteSpace(SearchWord);
					}
				});
			}
		}
		#endregion 
		
		#region ApplySongCommand
		DelegateCommand applySongCommand_ = null;
		public DelegateCommand ApplySongCommand
		{
			get
			{
				return applySongCommand_ ?? (applySongCommand_ = new DelegateCommand {
					ExecuteHandler = param => {
						var searchResult = param as SongSearchResult;
						if (searchResult == null) { return; }

						try {
							OnSongParsed(new SongParsedEventArgs(SongSearchResult.ToGeneralInfo(searchResult, Settings.CheckSeriesTypeNumberAutomatically)));
						}
						catch (Exception ex) {
							MessageBox.Show(string.Format("曲情報の適用に失敗しました。\n\n【例外情報】\n{0}", ex), "エラー", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Stop);
						}
					}
				});
			}
		}
		#endregion 

		#region CancelSearchingCommand
		DelegateCommand cancelSearchingCommand_ = null;
		public DelegateCommand CancelSearchingCommand
		{
			get
			{
				return cancelSearchingCommand_ ?? (cancelSearchingCommand_ = new DelegateCommand {
					ExecuteHandler = param => {
						CancellationTokenSource.Cancel();
						CancelSearchingCommand.RaiseCanExecuteChanged();
					},
					CanExecuteHandler = param => {
						return (IsSearching && CancellationTokenSource != null && !CancellationTokenSource.IsCancellationRequested);
					}
				});
			}
		}
		#endregion 
		#endregion
	}
}
