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
using System.Reactive.Disposables;


namespace AnizanHelper.ViewModels
{
	internal class MainWindowViewModel : BindableBase, IDisposable
	{
		#region private fields
		ISongInfoSerializer serializer_ = null;
		AnizanSongInfoConverter converter_ = null;
		CompositeDisposable disposables_ = new CompositeDisposable();
		#endregion

		#region コンストラクタ
		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="dispatcher">ディスパッチャ</param>
		public MainWindowViewModel(
			Settings settings,
			AnizanSongInfoConverter converter,
			IDispatcher dispatcher)
			: base(dispatcher)
		{
			if (settings == null) { throw new ArgumentNullException("settings"); }
			if (converter == null) { throw new ArgumentNullException("converter"); }

			Settings = settings;
			SongInfo = new AnizanSongInfo();
			converter_ = converter;
			serializer_ = new Models.Serializers.AnizanListSerializer();
		
			SearchVm = new SongSearchViewModel(settings, dispatcher);
			SongParserVm = new ViewModels.SongParserVm(dispatcher);

			SearchVm.SongParsed += SongParsed;
			songParserVm.SongParsed += SongParsed;

			settings.PropertyChanged += settings_PropertyChanged;

			disposables_.Add(MessageService.Current.MessageObservable.Subscribe(message => StatusText = message));
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

			// 番号インクリメント
			if (Settings.IncrementSongNumberWhenCopied) {
				SongNumber++;
			}

			// 入力欄クリア
			if (Settings.ClearInputAutomatically) {
				Dispatch(() => {
					SongParserVm.ClearInput();
					SearchVm.ClearInput();
				});
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
				Body = str,
			};
			if (Settings.WriteAsSage) {
				res.Email = "sage";
			}
			var post = new X2chPost();
			post.Posted += (s, e) => {
				Console.WriteLine("Response: ", e.Response);
				switch (e.Response) {
					case PostResponse.Success:
						MessageService.Current.ShowMessage(string.Format("書き込み成功! ( {0} )",
							str.Length > 40 ? str.Substring(0, 40) + "..." : str));
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
						MessageService.Current.ShowMessage(e.Response.ToString() + "  " + e.Text);
						break;
				}
			};
			post.Error += (s, e) => {
				MessageService.Current.ShowMessage("書き込み失敗orz");
				MessageBox.Show(
					string.Format("投稿に失敗しました。\n\n【例外情報】\n{0}", e.Exception),
					"エラー", MessageBoxButton.OK, MessageBoxImage.Stop);
			};

			Task.Factory.StartNew(() => {
				try {
					CanWrite = false;
					MessageService.Current.ShowMessage("書き込んでいます...");
					post.Post(thread, res);

					if (Settings.IncrementSongNumberWhenCopied) {
						SongNumber++;
					}

					// 入力欄クリア
					if (Settings.ClearInputAutomatically) {
						Dispatch(() => {
							SongParserVm.ClearInput();
							SearchVm.ClearInput();
						});
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
		AnizanSongInfo songInfo_ = null;
		public AnizanSongInfo SongInfo
		{
			get
			{
				return songInfo_;
			}
			set
			{
				SetValue(ref songInfo_, value,
					actBeforeChange: oldInfo => {
						if (oldInfo != null) {
							oldInfo.PropertyChanged -= SongInfo_PropertyChanged;
						}
					},
					actAfterChange: newInfo => {
						newInfo.PropertyChanged += SongInfo_PropertyChanged;
						Dispatch(() => {
							ToTvSizeCommand.RaiseCanExecuteChanged();
							ToLiveVersionCommand.RaiseCanExecuteChanged();
							SetAdditionalCommand.RaiseCanExecuteChanged();
						});
					},
					propertyName: GetMemberName(() => SongInfo));
			}
		}

		void SongInfo_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if (Settings.ApplySongInfoAutomatically) {
				Serialize();
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

		#region CheckForDictionaryUpdateCommand
		DelegateCommand checkForDictionaryUpdateCommand_ = null;
		public DelegateCommand CheckForDictionaryUpdateCommand
		{
			get
			{
				return checkForDictionaryUpdateCommand_ ?? (checkForDictionaryUpdateCommand_ = new DelegateCommand {
					ExecuteHandler = param => {
						var app = App.Current as App;
						if (app != null) {
							Task.Factory.StartNew(() => {
								app.UpdateDictionary();
							});
						}
					}
				});
			}
		}
		#endregion 

		#region ToTvSizeCommand
		DelegateCommand toTvSizeCommand_ = null;
		public DelegateCommand ToTvSizeCommand
		{
			get
			{
				return toTvSizeCommand_ ?? (toTvSizeCommand_ = new DelegateCommand {
					ExecuteHandler = param => {
						var oldStr = SongInfo.Title ?? "";
						SongInfo.Title = oldStr.TrimEnd() + "(TV size)";
					},
					CanExecuteHandler = param => {
						return (SongInfo != null);
					}
				});
			}
		}
		#endregion 

		#region ToLiveVersionCommand
		DelegateCommand toLiveVersionCommand_ = null;
		public DelegateCommand ToLiveVersionCommand
		{
			get
			{
				return toLiveVersionCommand_ ?? (toLiveVersionCommand_ = new DelegateCommand {
					ExecuteHandler = param => {
						var oldStr = SongInfo.Title ?? "";
						SongInfo.Title = oldStr.TrimEnd() + "(Live ver.)";
						SongInfo.SongType = "AR";
					},
					CanExecuteHandler = param => {
						return (SongInfo != null);
					}
				});
			}
		}
		#endregion 
		#endregion // Commands

		#region SetAdditionalCommand
		DelegateCommand setAdditionalCommand_ = null;
		public DelegateCommand SetAdditionalCommand
		{
			get
			{
				return setAdditionalCommand_ ?? (setAdditionalCommand_ = new DelegateCommand {
					ExecuteHandler = param => {
						var str = param as string;
						if (string.IsNullOrWhiteSpace(str)) { return; }
						if (!string.IsNullOrWhiteSpace(SongInfo.Additional)) {
							SongInfo.Additional += "," + str;
						}
						else {
							SongInfo.Additional = str;
						}
					},
					CanExecuteHandler = param => {
						return (SongInfo != null);
					}
				});
			}
		}
		#endregion 

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

		#region IDisposable メンバ
		bool isDisposed_ = false;
		virtual protected void Dispose(bool disposing)
		{
			if (isDisposed_) { return; }
			if (disposing) {
				disposables_.Dispose();
			}
			isDisposed_ = true;
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
		#endregion
	}
}
