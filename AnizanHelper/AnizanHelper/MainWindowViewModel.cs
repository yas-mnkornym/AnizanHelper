using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace AnizanHelper
{
	internal class MainWindowViewModel : BaseNotifyPropertyChanged
	{
		#region コンストラクタ
		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="dispatcher">ディスパッチャ</param>
		public MainWindowViewModel(Dispatcher dispatcher)
			: base(dispatcher)
		{ }
		#endregion

		#region Bindings

		#region AlwaysOnTop
		bool alwaysOnTop_ = true;
		public bool AlwaysOnTop
		{
			get
			{
				return alwaysOnTop_;
			}
			set
			{
				alwaysOnTop_ = value;
				RaisePropertyChanged();
			}
		}
		#endregion

		#region IncrementSongNumberWhenCopied
		bool incrementSongNumberWhenCopied_ = true;
		public bool IncrementSongNumberWhenCopied
		{
			get
			{
				return incrementSongNumberWhenCopied_;
			}
			set
			{
				incrementSongNumberWhenCopied_ = value;
				RaisePropertyChanged();
			}
		}
		#endregion

		#region SongName
		string songName_ = "";
		public string SongName
		{
			get
			{
				return songName_;
			}
			set
			{
				songName_ = value;
				RaisePropertyChanged();
			}
		}
		#endregion

		#region Singer
		string singer_ = "";
		public string Signer
		{
			get
			{
				return singer_;
			}
			set
			{
				singer_ = value;
				RaisePropertyChanged();
			}
		}
		#endregion

		#region Series
		string series_ = "";
		public string Series
		{
			get
			{
				return series_;
			}
			set
			{
				series_ = value;
				RaisePropertyChanged();
			}
		}
		#endregion

		#region SongType
		string songType_ = "";
		public string SongType
		{
			get
			{
				return songType_;
			}
			set
			{
				songType_ = value;
				RaisePropertyChanged();
			}
		}
		#endregion

		#endregion // Bindings


		#region Commands

		#region AnalyzeInputCommand
		DelegateCommand analyzeInputCommand_ = null;
		public DelegateCommand AnalyzeInputCommand
		{
			get
			{
				if (analyzeInputCommand_ == null) {
					analyzeInputCommand_ = new DelegateCommand {
						ExecuteHandler = param => {
						}
					};
				}
				return analyzeInputCommand_;
			}
		}
		#endregion

		#region ApplyDetailsCommand
		DelegateCommand applyDetailsCommand_ = null;
		public DelegateCommand ApplyDetailsCommand
		{
			get
			{
				if (applyDetailsCommand_ == null) {
					applyDetailsCommand_ = new DelegateCommand {
						ExecuteHandler = param => {
						}
					};
				}
				return applyDetailsCommand_;
			}
		}
		#endregion

		#region CopyResultCommand
		DelegateCommand copyResultCommand_ = null;
		public DelegateCommand CopyResultCommand
		{
			get
			{
				if (copyResultCommand_ == null) {
					copyResultCommand_ = new DelegateCommand {
						ExecuteHandler = param => {
						}
					};
				}
				return copyResultCommand_;
			}
		}
		#endregion

		#region CopyResultAndSongNumberCommand
		DelegateCommand copyResultAndSongNumberCommand = null;
		public DelegateCommand CopyResultAndSongNumberCommand
		{
			get
			{
				if (copyResultAndSongNumberCommand == null) {
					copyResultAndSongNumberCommand = new DelegateCommand {
						ExecuteHandler = param => {
						}
					};
				}
				return copyResultAndSongNumberCommand;
			}
		}
		#endregion

		#endregion // Commands
	}
}
