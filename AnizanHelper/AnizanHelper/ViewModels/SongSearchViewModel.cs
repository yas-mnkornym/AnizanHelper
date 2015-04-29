using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
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

		public SongSearchViewModel(IDispatcher dispatcher)
			: base(dispatcher)
		{ }


		void Search()
		{
			var words = SearchWord;
			IsSearching = true;
			MessageService.Current.ShowMessage("楽曲情報を検索しています...");
			Stopwatch sw = new Stopwatch();
			Task.Factory
				.StartNew(() => {
					sw.Start();
					return searcher_.Search(words);
				})
				.ContinueWith(task => {
					try {
						sw.Stop();
						Results = new ObservableCollection<GeneralSongInfo>(task.Result);
						MessageService.Current.ShowMessage(string.Format("検索完了({0}件, {1:0.000}秒)", Results.Count, sw.Elapsed.TotalSeconds));
					}
					catch (Exception ex) {
						MessageService.Current.ShowMessage(string.Format("楽曲情報の検索に失敗しました。 ({0})", ex.Message));
					}
					finally {
						IsSearching = false;
					}
				}, TaskScheduler.Current);
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
				if (SetValue(ref searchWord_, value, GetMemberName(() => SearchWord))) {
					SearchCommand.RaiseCanExecuteChanged();
					SearchOnBrowserCommand.RaiseCanExecuteChanged();
				}
			}
		}
		#endregion

		#region Results
		ObservableCollection<GeneralSongInfo> results_ = new ObservableCollection<GeneralSongInfo>();
		public ObservableCollection<GeneralSongInfo> Results
		{
			get
			{
				return results_;
			}
			set
			{
				SetValue(ref results_, value, GetMemberName(() => Results));
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
				if (SetValue(ref isSearching_, value, GetMemberName(() => IsSearching))) {
					Dispatch(() => {
						SearchCommand.RaiseCanExecuteChanged();
					});
				}
			}
		}
		#endregion
		#endregion // Bindigns

		#region Commands
		#region SearchCommand
		DelegateCommand searchCommand_ = null;
		public DelegateCommand SearchCommand
		{
			get
			{
				return searchCommand_ ?? (searchCommand_ = new DelegateCommand {
					ExecuteHandler = param => {
						try {
							Search();
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
						var searchResult = param as GeneralSongInfo;
						if (searchResult == null) { return; }
						OnSongParsed(new SongParsedEventArgs(searchResult));
					}
				});
			}
		}
		#endregion 
		#endregion
	}
}
