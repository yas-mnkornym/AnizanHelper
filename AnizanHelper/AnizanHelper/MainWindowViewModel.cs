using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Windows;
using AnizanHelper.Models;
using AnizanHelper.Models.SettingComponents;
using Twin;
using Twin.Bbs;


namespace AnizanHelper
{
	internal class MainWindowViewModel : BindableBase
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
		public MainWindowViewModel(Settings settings, IDispatcher dispatcher)
			: base(dispatcher)
		{
			if (settings == null) { throw new ArgumentNullException("settings"); }
			Settings = settings;
			serializer_ = new Models.Serializers.AnizanListSerializer();
			parser_ = new Models.Parsers.AnisonDBParser(
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
					new ReplaceInfo("〜", "~"),
					new ReplaceInfo("−", "-"),
					// 必ず最後
					new ReplaceInfo("&", " & "),
					new ReplaceInfo("スポット放映", "[スポット放映]"),
					new ReplaceInfo("最終話", "[最終話]"),
					new ReplaceInfo("最終回に放映", "[最終話]"),

					// アイマス対策
					new ReplaceInfo("アイドルマスター シンデレラガールズ", "THE IDOLM@STER CINDERELLA GIRLS"),
					new ReplaceInfo("アイドルマスター ミリオンライブ!", "THE IDOLM@STER MILLION LIVE!"),
					new ReplaceInfo("アイドルマスター シャイニーフェスタ", "THE IDOLM@STER SHINY FESTA"),
					new ReplaceInfo("THE iDOLM@STER", "THE IDOLM@STER"),

					// アイカツ対策
					new ReplaceInfo("アイカツ!", "アイカツ!-アイドルカツドウ!-"),
					new ReplaceInfo("from STAR☆ANIS", "")
				});

			settings.PropertyChanged += settings_PropertyChanged;
		}

		void settings_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if (e.PropertyName == GetMemberName(() => Settings.ServerName) ||
				e.PropertyName == GetMemberName(() => Settings.BoardPath) ||
				e.PropertyName == GetMemberName(() => Settings.ThreadKey)) {
					Dispatch(() => {
						WriteToThreadCommand.RaiseCanExecuteChanged();
					});
			}
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
			Genre = info.Genre;
		}

		void Serialize()
		{
			if (serializer_ == null) { throw new InvalidOperationException("シリアライザが設定されていません。"); }
			var songInfo = new SongInfo{
				Title = SongTitle,
				Singer = Singer,
				Series = Series,
				SongType = SongType,
				IsNotAnison = IsNotAnison,
				Genre = Genre
			};

			ResultText = serializer_.Serialize(songInfo);
		}

		void CopyToClipboard(bool appendNumber)
		{
			string format = (appendNumber ? "{0:D4}{1}" : "{1}");
			var str = string.Format(format,
				SongNumber, ResultText);
			if (string.IsNullOrEmpty("str")) { str = " "; }
			System.Windows.Forms.Clipboard.SetText(str);

			if (Settings.IncrementSongNumberWhenCopied) {
				SongNumber++;
			}
		}

		void WriteToThread()
		{
			var board = new BoardInfo(Settings.ServerName, Settings.BoardPath, "板名");
			var thread = new X2chThreadHeader {
				BoardInfo = board,
				Key = Settings.ThreadKey
			};

			var str = string.Format("{0:D4}{1}", SongNumber, ResultText);

			var res = new PostRes {
				Body = str
			};
			var post = new X2chPost();
			post.Posted += (s, e) => {
				Console.WriteLine("Response: ", e.Response);
				switch (e.Response) {
					case PostResponse.Success:
						StatusText = string.Format("書き込み成功! ( {0} )",
							str.Length > 20 ? str.Substring(0, 20)+"..." : str);
						break;

					case PostResponse.Cookie:
						e.Retry = true;
						break;
				}
			};
			post.Error += (s, e) => {
				StatusText = "書き込み失敗orz";
				MessageBox.Show(
					string.Format("投稿に失敗しました。\n\n【例外情報】\n{0}", e.Exception),
					"エラー", MessageBoxButton.OK, MessageBoxImage.Stop);
			};

			Task.Factory.StartNew(() => {
				try {
					CanWrite = false;
					StatusText = "書き込んでいます...";
					post.Post(thread, res);
				}
				catch (Exception ex) {
					MessageBox.Show(
						string.Format("投稿に失敗しました。\n\n【例外情報】\n{0}", ex),
						"エラー", MessageBoxButton.OK, MessageBoxImage.Stop);
				}
				finally {
					CanWrite = true;
				}
			});
		}
		#endregion

		#region Bindings

		#region 設定

		#region Settings
		Settings settings_;
		public Settings Settings
		{
			get
			{
				return settings_;
			}
			private set{
				SetValue(ref settings_, value, GetMemberName(() => Settings));
			}
		}
		#endregion

		#endregion // 設定

		#region CanWrite
		bool canWrite_ = true;
		public bool CanWrite
		{
			get
			{
				return canWrite_;
			}
			set
			{
				if (SetValue(ref canWrite_, value, GetMemberName(() => CanWrite))) {
					Dispatch(() => WriteToThreadCommand.RaiseCanExecuteChanged());
				}
			}
		}
		#endregion

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
				RaisePropertyChanged("InputText");
			}
		}
		#endregion

		#region StatusText
		string statusText_ = "";
		public string StatusText
		{
			get
			{
				return statusText_;
			}
			set
			{
				SetValue(ref statusText_, value, GetMemberName(() => StatusText));
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
				if (SetValue(ref resultText_, value, GetMemberName(() => ResultText))) {
					Dispatch(() => WriteToThreadCommand.RaiseCanExecuteChanged());
				}
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
				RaisePropertyChanged("SongNumber");
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
				RaisePropertyChanged("IsNotAnison");
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
				RaisePropertyChanged("SongTitle");
			}
		}
		#endregion

		#region Genre
		string genre_ = "";
		public string Genre
		{
			get
			{
				return genre_;
			}
			set
			{
				genre_ = value;
				RaisePropertyChanged("Genre");
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
				RaisePropertyChanged("Singer");
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
				RaisePropertyChanged("Series");
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
				RaisePropertyChanged("SongType");
			}
		}
		#endregion

		#region VersionName
		string versionName_ = null;
		public string VersionName
		{
			get
			{
				if (versionName_ == null) {
					var version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
					versionName_ = string.Format("{0}.{1}.{2}", version.Major, version.Minor, version.Build);
				}
				return versionName_;
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
								if (Settings.CopyAfterParse) {
									CopyToClipboard(true);
								}
								if (Settings.ClearInputAutomatically) {
									InputText = "";
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
								if (Settings.CopyAfterApply) {
									CopyToClipboard(true);
								}
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

		#region WriteToThreadCommand
		DelegateCommand writeToThreadCommand_ = null;
		public DelegateCommand WriteToThreadCommand
		{
			get
			{
				return writeToThreadCommand_ ?? (writeToThreadCommand_ = new DelegateCommand {
					ExecuteHandler = param => {
						WriteToThread();
					},
					CanExecuteHandler = param => {
						return (
							CanWrite &&
							!string.IsNullOrWhiteSpace(ResultText) &&
							!string.IsNullOrWhiteSpace(Settings.ServerName) &&
							!string.IsNullOrWhiteSpace(Settings.BoardPath) &&
							!string.IsNullOrWhiteSpace(Settings.ThreadKey));
					}
				});
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
