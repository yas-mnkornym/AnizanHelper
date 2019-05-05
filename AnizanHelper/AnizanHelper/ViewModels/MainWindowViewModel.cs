using System;
using System.Linq;
using System.Net.Http;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using AnizanHelper.Models;
using AnizanHelper.Models.Parsers;
using AnizanHelper.Services;
using AnizanHelper.Views;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using Twin;
using Twin.Bbs;


namespace AnizanHelper.ViewModels
{
	internal class MainWindowViewModel : ReactiveViewModelBase, IDisposable
	{
		ISongInfoSerializer serializer_ = null;
		AnizanSongInfoConverter converter_ = null;
		AnizanFormatParser AnizanFormatParser { get; } = new AnizanFormatParser();
		Subject<AnizanSongInfo> SongsSubject { get; } = new Subject<AnizanSongInfo>();
		SongListWindow SongListWindow { get; }
		UpdateCheckerService UpdateCheckerService { get; }

		#region コンストラクタ
		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="dispatcher">ディスパッチャ</param>
		public MainWindowViewModel(
			Window mainWindow,
			Settings settings,
			AnizanSongInfoConverter converter,
			IServiceManager serviceManager,
			HttpClient httpClient,
			Studiotaiha.Toolkit.IDispatcher dispatcher)
			: base(dispatcher)
		{
			if (mainWindow == null) { throw new ArgumentNullException(nameof(mainWindow)); }
			if (serviceManager == null) { throw new ArgumentNullException(nameof(serviceManager)); }
			if (httpClient == null) { throw new ArgumentNullException(nameof(httpClient)); }

			Settings = settings ?? throw new ArgumentNullException("settings");
			converter_ = converter ?? throw new ArgumentNullException("converter");

			SongInfo = new AnizanSongInfo();
			serializer_ = new Models.Serializers.AnizanListSerializer();

			SearchVm = new SongSearchViewModel(settings, dispatcher);
			SongParserVm = new SongParserVm(dispatcher);
			SongMetadataViewerViewmodel = new SongMetadataViewerControlViewModel(settings, httpClient);

			SearchVm.SongParsed += SongParsed;
			SongParserVm.SongParsed += SongParsed;

			settings.PropertyChanged += settings_PropertyChanged;

			MessageService.Current.MessageObservable
				.Subscribe(message => StatusText = message)
				.AddTo(Disposables);

			SongsSubject.AddTo(Disposables);
			SongListWindowViewModel = new SongListWindowViewModel(settings, serializer_, SongsSubject)
				.AddTo(Disposables);

			SongListWindow = new SongListWindow
			{
				DataContext = SongListWindowViewModel,
				Topmost = Settings.AlwaysOnTop
			};

			SongListWindow.Loaded += (_, __) =>
			{
				SongListWindow.Owner = mainWindow;
				SongListWindow.Left = mainWindow.Left + mainWindow.Width;
				SongListWindow.Top = mainWindow.Top;
				SongListWindow.Height = mainWindow.Height;
			};

			mainWindow.Closed += (_, __) =>
			{
				SongListWindow.CloseImmediately();
			};

			mainWindow.LocationChanged += (_, __) =>
			{
				if (mainWindow.Visibility == Visibility.Visible && Settings.SnapListWindow)
				{
					SongListWindow.Left = mainWindow.Left + mainWindow.Width;
					SongListWindow.Top = mainWindow.Top;
				}
			};

			mainWindow.SizeChanged += (_, __) =>
			{
				if (mainWindow.Visibility == Visibility.Visible && Settings.SnapListWindow)
				{
					SongListWindow.Left = mainWindow.Left + mainWindow.Width;
					SongListWindow.Top = mainWindow.Top;
				}
			};

			SongListWindow.SizeChanged += (_, __) =>
			{
				if (mainWindow.Visibility == Visibility.Visible && SongListWindow.Visibility == Visibility.Visible && Settings.SnapListWindow)
				{
					SongListWindow.Left = mainWindow.Left + mainWindow.Width;
					SongListWindow.Top = mainWindow.Top;
				}
			};

			SongListWindow.IsVisibleChanged += (_, e) =>
			{
				if (SongListWindow.Visibility != Visibility.Visible)
				{
					this.ShowListWindow.Value = false;
				}
			};

			UpdateCheckerService = serviceManager.Services.First(x => x is UpdateCheckerService) as UpdateCheckerService;

			settings.PropertyChangedAsObservable()
				.Where(x => x.PropertyName == nameof(Settings.SnapListWindow))
				.ObserveOnDispatcher()
				.Subscribe(_ =>
				{
					if (mainWindow.Visibility == Visibility.Visible && Settings.SnapListWindow)
					{
						SongListWindow.Left = mainWindow.Left + mainWindow.Width;
						SongListWindow.Top = mainWindow.Top;
					}
				});

			Settings.PropertyChangedAsObservable()
				.Where(x => x.PropertyName == nameof(Settings.AlwaysOnTop))
				.ObserveOnUIDispatcher()
				.Subscribe(_ =>
				{
					SongListWindow.Topmost = Settings.AlwaysOnTop;
				})
				.AddTo(Disposables);

			ShowListWindow
				.ObserveOnUIDispatcher()
				.Subscribe(show =>
				{
					if (show)
					{
						SongListWindow.Show();
					}
					else
					{
						if (SongListWindow.Visibility == Visibility.Visible)
						{
							SongListWindow.Hide();
						}
					}
				})
				.AddTo(Disposables);

			ShowParserControl = Settings
				.ToReactivePropertyAsSynchronized(x => x.ShowParserControl, mode: ReactivePropertyMode.RaiseLatestValueOnSubscribe | ReactivePropertyMode.DistinctUntilChanged)
				.AddTo(this.Disposables);

			ShowTagRetreiver = Settings
				.ToReactivePropertyAsSynchronized(x => x.ShowStreamMetadataRetreiver)
				.AddTo(this.Disposables);


			SetToSpecialCommand = new ReactiveCommand<string>()
				.WithSubscribe(type =>
				{
					var header = "★";
					var body = "";
					switch (type)
					{
						case "BGM":
							body = "繋ぎBGM";
							break;

						case "SE":
							body = "SE";
							break;

						case "CM":
							body = "CM";
							break;

						case "DJTALK":
							header = "▼";
							body = "DJトーク";
							break;

						default:
							return;
					}

					SongInfo.IsSpecialItem = true;
					SongInfo.SpecialHeader = header;
					SongInfo.SpecialItemName = body;
				});

			SetPresetCommand = new ReactiveCommand<string>()
				.WithSubscribe(type =>
				{
					switch (type)
					{
						case "NOANIME":
							SongInfo.Genre = string.Empty;
							SongInfo.Series = "一般曲";
							SongInfo.SongType = string.Empty;
							break;

						case "CLEARALL":
							if (MessageBox.Show("記入内容を全てクリアします", "確認", MessageBoxButton.OKCancel, MessageBoxImage.Warning) == MessageBoxResult.OK)
							{
								SongInfo = new AnizanSongInfo();
							}
							break;
					}
				});

			PasteZanmaiFormatCommand = new ReactiveCommand()
				.WithSubscribe(() =>
				{
					try
					{
						var item = Clipboard.GetText()
							.Replace("\r", "")
							.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries)
							.Select(line => line.Trim())
							.Select(line => AnizanFormatParser.ParseAsAnizanInfo(line))
							.FirstOrDefault(x => x != null);

						if (item == null)
						{
							return;
						}

						item.IsSpecialItem = !string.IsNullOrWhiteSpace(item.SpecialHeader);
						this.SongInfo = item;
						SearchVm.SearchWord = item.Title;
					}
					catch (Exception ex)
					{
						var sb = new StringBuilder();
						sb.AppendLine("クリップボードからの情報取得に失敗しました。");
						sb.AppendLine(ex.Message);

						MessageBox.Show(sb.ToString(), "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
					}
				});

			CheckForUpdateCommand = new AsyncReactiveCommand()
				.WithSubscribe(async () =>
				{
					try
					{
						var ret = await UpdateCheckerService.CheckForUpdateAndShowDialogIfAvailableAsync(false);
						if (ret == false)
						{
							MessageService.Current.ShowMessage("アップデートはありません。");
						}
					}
					catch (Exception ex)
					{
						var sb = new StringBuilder();
						sb.AppendLine("更新情報の取得に失敗しました。");
						sb.AppendLine(ex.Message);

						MessageBox.Show(sb.ToString(), "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
					}
				},
				x => x.AddTo(this.Disposables));
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
			ResultText = SongInfo.IsSpecialItem
				? serializer_.SerializeFull(SongInfo)
				: serializer_.Serialize(SongInfo);
		}

		void CopyToClipboard(bool appendNumber)
		{
			string format = (appendNumber ? "{0:D4}{1}" : "{1}");
			var str = string.Format(format,
				SongNumber, ResultText);
			if (string.IsNullOrEmpty("str")) { str = " "; }

			App.Current.Dispatcher.BeginInvoke((Action)(() =>
			{
				try
				{
					System.Windows.Forms.Clipboard.SetText(str);
				}
				catch(Exception ex)
				{
					this.ShowErrorMessage("コピーに失敗しました。", ex);
					return;
				}

				AddCurrentSongToList();

				// 番号インクリメント
				if (Settings.IncrementSongNumberWhenCopied && !SongInfo.IsSpecialItem)
				{
					SongNumber++;
				}

				// 入力欄クリア
				if (Settings.ClearInputAutomatically)
				{
					Dispatch(() =>
					{
						SongParserVm.ClearInput();
						SearchVm.ClearInput();
					});
				}
			}));
		}

		void AddCurrentSongToList()
		{
			var info = new AnizanSongInfo {
				Title = SongInfo.Title,
				Singer = SongInfo.Singer,
				Genre = SongInfo.Genre,
				Series = SongInfo.Series,
				SongType = SongInfo.SongType,
				Additional = SongInfo.Additional,
			};

			if (SongInfo.IsSpecialItem) {
				info.SpecialHeader = SongInfo.SpecialHeader;
				info.SpecialItemName = SongInfo.SpecialItemName;
				info.Number = 0;
			}
			else {
				info.Number = SongNumber;
			}

			SongsSubject.OnNext(info);
		}

		void WriteToThread()
		{
			var board = new BoardInfo(Settings.ServerName, Settings.BoardPath, "板名");
			var thread = new X2chThreadHeader {
				BoardInfo = board,
				Key = Settings.ThreadKey
			};
			
			var str = SongInfo.IsSpecialItem
					? ResultText
					: string.Format("{0:D4}{1}", SongNumber, ResultText);

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

						AddCurrentSongToList();

						// 曲番号インクリメント
						if (!SongInfo.IsSpecialItem && Settings.IncrementSongNumberWhenCopied) {
							SongNumber++;
						}

						// 入力欄クリア
						if (Settings.ClearInputAutomatically) {
							Dispatch(() => {
								SongParserVm.ClearInput();
								SearchVm.ClearInput();
							});
						}
						break;

					case PostResponse.Cookie:
						e.Retry = true;
						break;

					case PostResponse.Error:
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

			Task.Factory.StartNew((Action)(() => {
				try {
					this.CanWrite = false;
					MessageService.Current.ShowMessage("書き込んでいます...");
					post.Post(thread, res);
				}
				catch (Exception ex) {
					MessageBox.Show(
						string.Format("投稿に失敗しました。\n\n【例外情報】\n{0}", ex),
						"エラー", MessageBoxButton.OK, MessageBoxImage.Stop);
				}
				finally {
					this.CanWrite = true;
				}
			}));
		}
		#endregion

		#region Bindings
		
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

		public bool CanWrite
		{
			get
			{
				return GetValue<bool>(true);
			}
			set
			{
				if (SetValue(value)) {
					Dispatch(() => WriteToThreadCommand.RaiseCanExecuteChanged());
				}
			}
		}

		public string StatusText
		{
			get
			{
				return GetValue<string>();
			}
			set
			{
				SetValue(value);
			}
		}

		public string ResultText
		{
			get
			{
				return GetValue<string>();
			}
			set
			{
				if (SetValue(value)) {
					Dispatch(() => WriteToThreadCommand.RaiseCanExecuteChanged());
				}
			}
		}

		public int SongNumber
		{
			get
			{
				return GetValue<int>(1);
			}
			set
			{
				SetValue(value);
			}
		}

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

		public ReactiveProperty<bool> ShowListWindow { get; } = new ReactiveProperty<bool>();
		public ReactiveProperty<bool> SnapListWindow { get; } = new ReactiveProperty<bool>(true);

		public SongSearchViewModel SearchVm
		{
			get
			{
				return GetValue<SongSearchViewModel>();
			}
			set
			{
				SetValue(value);
			}
		}

		public SongParserVm SongParserVm
		{
			get
			{
				return GetValue<SongParserVm>();
			}
			set
			{
				SetValue(value);
			}
		}

		public SongMetadataViewerControlViewModel SongMetadataViewerViewmodel
		{
			get => this.GetValue<SongMetadataViewerControlViewModel>();
			set => this.SetValue(value);
		}

		public AnizanSongInfo SongInfo
		{
			get
			{
				return GetValue<AnizanSongInfo>();
			}
			set
			{
				SetValue(
					value,
					actBeforeChange: (oldValue, newValue) => {
						if (oldValue != null) {
							oldValue.PropertyChanged -= SongInfo_PropertyChanged;
						}
					},
					actAfterChange: (oldValue, newValue) => {
						newValue.PropertyChanged += SongInfo_PropertyChanged;
						Dispatch(() => {
							ToTvSizeCommand.RaiseCanExecuteChanged();
							ToLiveVersionCommand.RaiseCanExecuteChanged();
							SetAdditionalCommand.RaiseCanExecuteChanged();
						});
					});
			}
		}

		public SongListWindowViewModel SongListWindowViewModel
		{
			get
			{
				return GetValue<SongListWindowViewModel>();
			}
			set
			{
				SetValue(value);
			}
		}

		void SongInfo_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if (Settings.ApplySongInfoAutomatically) {
				Serialize();
			}
		}

		public ReactiveProperty<bool> ShowParserControl { get; }
		public ReactiveProperty<bool> ShowTagRetreiver { get; }

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
								this.ShowErrorMessage("コピーに失敗しました。", ex);
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
							catch (Exception ex)
							{
								this.ShowErrorMessage("コピーに失敗しました。", ex);
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

		#region ReloadDictionaryCommand
		DelegateCommand reloadDictionaryCommand_ = null;
		public DelegateCommand ReloadDictionaryCommand
		{
			get
			{
				return reloadDictionaryCommand_ ?? (reloadDictionaryCommand_ = new DelegateCommand {
					ExecuteHandler = param => {
						if (App.Current is App app) {
							try {
								app.LoadReplaceDictionary();
								InfoMsg("辞書の読み込みを完了しました。");
							}
							catch (Exception ex) {
								ErrMsg("辞書データの読み込みに失敗しました。", ex);
							}
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

		public ReactiveCommand<string> SetToSpecialCommand { get; }
		public ReactiveCommand<string> SetPresetCommand { get; }
		public ReactiveCommand PasteZanmaiFormatCommand { get; }
		public AsyncReactiveCommand CheckForUpdateCommand { get; }

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
	}
}
