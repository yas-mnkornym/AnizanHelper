using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
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
			var words = SearchWord
				.Replace(" ", "+")
				.Replace("\t", "+");
			Results= new ObservableCollection<GeneralSongInfo>(searcher_.Search(words));
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
