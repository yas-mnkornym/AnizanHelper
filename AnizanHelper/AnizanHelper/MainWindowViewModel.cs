using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Windows;
using AnizanHelper.Data;


namespace AnizanHelper
{
	internal class MainWindowViewModel : BaseNotifyPropertyChanged
	{
		#region private fields
		ISongInfoParser parser_ = null;
		ISongInfoSerializer serializer_ = null;
		#endregion

		#region コンストラクタ
		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="dispatcher">ディスパッチャ</param>
		public MainWindowViewModel(Dispatcher dispatcher)
			: base(dispatcher)
		{
			serializer_ = new Data.Serializers.AnizanListSerializer();
			parser_ = new Data.Parsers.AnisonDBParser(
				new ReplaceInfo[]{
					new ReplaceInfo("♥", "▼"),
					new ReplaceInfo("♡", "▽"),
					new ReplaceInfo("＆", "&"),
					new ReplaceInfo("？", "?"),
					new ReplaceInfo("！", "!"),
					new ReplaceInfo("／", "/"),
					new ReplaceInfo("、", "､"),
					new ReplaceInfo("。", "｡"),
					new ReplaceInfo("・", "･"),
					new ReplaceInfo("「", "｢"),
					new ReplaceInfo("」", "｣"),
					new ReplaceInfo("～", "~"),
					new ReplaceInfo("￥", "\\"),
					new ReplaceInfo("＿", "_"),
					new ReplaceInfo("’", "'"),
					new ReplaceInfo("”", "\""),
					new ReplaceInfo("＃", "#"),
					new ReplaceInfo("＄", "$"),
					new ReplaceInfo("％", "%"),
					new ReplaceInfo("（", "("),
					new ReplaceInfo("）", ")"),
					new ReplaceInfo("＝", "="),
					new ReplaceInfo("―", "-"),
					new ReplaceInfo("＾", "^"),
					new ReplaceInfo("　", " "),
					// 必ず最後
					new ReplaceInfo("&", " & "),
				});
		}
		#endregion

		#region いろいろやるやつ
		void Parse()
		{
			if (parser_ == null) { throw new InvalidOperationException("パーサが設定されていません。"); }
			var info = parser_.Parse(InputText);
			SongTitle = info.Title;
			Singer = info.Singer;
			Series = info.Series;
			SongType = info.SongType;
			IsNotAnison = info.IsNotAnison;
		}

		void Serialize()
		{
			if (serializer_ == null) { throw new InvalidOperationException("シリアライザが設定されていません。"); }
			var songInfo = new SongInfo{
				Title = SongTitle,
				Singer = Singer,
				Series = Series,
				SongType = SongType,
				IsNotAnison = IsNotAnison
			};

			ResultText = serializer_.Serialize(songInfo);
		}

		void CopyToClipboard(bool appendNumber)
		{
			string format = (appendNumber ? "{0:D4}.{1}" : "{1}");
			var str = string.Format(format,
				SongNumber, ResultText);
			if (string.IsNullOrEmpty("str")) { str = " "; }
			System.Windows.Forms.Clipboard.SetText(str);

			if (IncrementSongNumberWhenCopied) {
				SongNumber++;
			}
		}
		#endregion

		#region Bindings

		#region 設定
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

		#region CopyOnParse
		bool copyAfterParse_ = true;
		public bool CopyAfterParse
		{
			get
			{
				return copyAfterParse_;
			}
			set
			{
				copyAfterParse_ = value;
				RaisePropertyChanged();
			}
		}
		#endregion

		#region CopyAfterApply
		bool copyAfterApply_ = false;
		public bool CopyAfterApply
		{
			get
			{
				return copyAfterApply_;
			}
			set
			{
				copyAfterApply_ = value;
				RaisePropertyChanged();
			}
		}
		#endregion

		#region IncrementSongNumberWhenCopied
		bool incrementSongNumberWhenCopied_ = false;
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
		#endregion // 設定


		#region InputText
		string inputText_ = "";
		public string InputText
		{
			get
			{
				return inputText_;
			}
			set
			{
				inputText_ = value;
				RaisePropertyChanged();
			}
		}
		#endregion

		#region ResultText
		string resultText_ = "";
		public string ResultText
		{
			get
			{
				return resultText_;
			}
			set
			{
				resultText_ = value;
				RaisePropertyChanged();
			}
		}
		#endregion

		#region SongNumber
		int songNumber_ = 1;
		public int SongNumber
		{
			get
			{
				return songNumber_;
			}
			set
			{
				songNumber_ = value;
				RaisePropertyChanged();
			}
		}
		#endregion

		#region IsNotAnison
		bool isNotAnison_ = false;
		public bool IsNotAnison
		{
			get
			{
				return isNotAnison_;
			}
			set
			{
				isNotAnison_ = value;
				RaisePropertyChanged();
			}
		}
		#endregion

		#region SongName
		string songName_ = "";
		public string SongTitle
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
		public string Singer
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

		#region ParseInputCommand
		DelegateCommand parseInputCommand_ = null;
		public DelegateCommand ParseInputCommand
		{
			get
			{
				if (parseInputCommand_ == null) {
					parseInputCommand_ = new DelegateCommand {
						ExecuteHandler = param => {
							try {
								Parse();
								Serialize();
								if (CopyAfterParse) {
									CopyToClipboard(true);
								}
							}
							catch (Exception ex) {
								ErrMsg("曲情報の解析に失敗しました。", ex);
							}
						}
					};
				}
				return parseInputCommand_;
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
							try {
								Serialize();
							}
							catch (Exception ex) {
								ErrMsg("曲情報の適用に失敗しました。", ex);
							}
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
							try {
								CopyToClipboard(false);
								if (CopyAfterApply) {
									CopyToClipboard(true);
								}
							}
							catch (Exception ex) {
								ErrMsg("コピーに失敗しました。", ex);
							}
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
							try {
								CopyToClipboard(true);
							}
							catch (Exception ex) {
								ErrMsg("コピーに失敗しました。", ex);
							}
						}
					};
				}
				return copyResultAndSongNumberCommand;
			}
		}
		#endregion

		#endregion // Commands

		#region メッセージ
		void InfoMsg(string message)
		{
			MessageBox.Show(message, "情報", MessageBoxButton.OK, MessageBoxImage.Information);
		}

		void InfoMsg(string message, Exception ex)
		{
			InfoMsg(string.Format("{0}\n\n***例外情報***\n{1}",
				message, ex.ToString()));
		}

		void ErrMsg(string message)
		{
			MessageBox.Show(message, "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
		}

		void ErrMsg(string message, Exception ex)
		{
			ErrMsg(string.Format("{0}\n\n***例外情報***\n{1}",
				message, ex.ToString()));
		}
		#endregion
	}
}
