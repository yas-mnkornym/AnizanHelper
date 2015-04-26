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


namespace AnizanHelper.ViewModels
{
	internal class MainWindowViewModel : BindableBase
	{
		#region private fields
		ISongInfoSerializer serializer_ = null;
		AnizanSongInfoConverter converter_ = null;
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
			settings.PropertyChanged += settings_PropertyChanged;

			serializer_ = new Models.Serializers.AnizanListSerializer();

			SearchVm = new SongSearchViewModel(dispatcher);
			SongParserVm = new ViewModels.SongParserVm(dispatcher);

			SearchVm.SongParsed += SongParsed;
			songParserVm.SongParsed += SongParsed;

			
			converter_ = new AnizanSongInfoConverter(
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

		}

		// 曲情報がパースされたよ！
		void SongParsed(object sender, SongParsedEventArgs e)
		{
			SongInfo = converter_.Convert(e.SongInfo);
			Serialize();
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

		void Serialize()
		{
			if (serializer_ == null) { throw new InvalidOperationException("シリアライザが設定されていません。"); }
			ResultText = serializer_.Serialize(SongInfo);
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

					case  PostResponse.Error:
					case PostResponse.Samba:
					case PostResponse.Timeout:
						MessageBox.Show(e.Text, "書き込み失敗", MessageBoxButton.OK, MessageBoxImage.Warning);
						e.Retry = false;
						break;

					default:
						StatusText = e.Response.ToString() + "  " + e.Text;
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

					if (Settings.IncrementSongNumberWhenCopied) {
						SongNumber++;
					}
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

		#region SearchVm
		SongSearchViewModel searchVm_;
		public SongSearchViewModel SearchVm
		{
			get
			{
				return searchVm_;
			}
			set
			{
				SetValue(ref searchVm_, value, GetMemberName(() => SearchVm));
			}
		}
		#endregion

		#region SongParserVm
		SongParserVm songParserVm = null;
		public SongParserVm SongParserVm
		{
			get
			{
				return songParserVm;
			}
			set
			{
				SetValue(ref songParserVm, value, GetMemberName(() => SongParserVm));
			}
		}
		#endregion

		#region SongInfo
		AnizanSongInfo songInfo_ = new AnizanSongInfo();
		public AnizanSongInfo SongInfo
		{
			get
			{
				return songInfo_;
			}
			set
			{
				SetValue(ref songInfo_, value, GetMemberName(() => SongInfo));
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
								Serialize();

								// 自動コピー
								if (Settings.CopyAfterParse) {
									CopyToClipboard(true);
								}

								// 入力欄クリア
								if (Settings.ClearInputAutomatically) {
									SongParserVm.ClearInput();
									SearchVm.ClearInput();
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
