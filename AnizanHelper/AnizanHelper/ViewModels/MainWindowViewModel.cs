using System;
using System.Linq;
using System.Net.Http;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using AnizanHelper.Models;
using AnizanHelper.Models.DbSearch;
using AnizanHelper.Models.Parsers;
using AnizanHelper.Services;
using AnizanHelper.ViewModels.Pages;
using AnizanHelper.Views;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using Studiotaiha.LazyProperty;
using Twin;
using Twin.Bbs;

namespace AnizanHelper.ViewModels
{
	internal class MainWindowViewModel : ReactiveViewModelBase, IDisposable
	{
		private ISongInfoSerializer serializer_ = null;
		private AnizanSongInfoConverter converter_ = null;
		private SongPresetRepository songPresetRepository_ = null;
		private AnizanFormatParser AnizanFormatParser { get; } = new AnizanFormatParser();
		private Subject<AnizanSongInfo> SongsSubject { get; } = new Subject<AnizanSongInfo>();
		private SongListWindow SongListWindow { get; }
		private UpdateCheckerService UpdateCheckerService { get; }

		#region コンストラクタ

		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="dispatcher">ディスパッチャ</param>
		public MainWindowViewModel(
			Settings settings,
			AnizanSongInfoConverter converter,
			SongPresetRepository songPresetRepository,
			IServiceManager serviceManager,
			HttpClient httpClient,
			ISearchController searchManager)
		{
			if (serviceManager == null) { throw new ArgumentNullException(nameof(serviceManager)); }
			if (httpClient == null) { throw new ArgumentNullException(nameof(httpClient)); }

			this.Settings = settings ?? throw new ArgumentNullException(nameof(settings));
			this.converter_ = converter ?? throw new ArgumentNullException(nameof(converter));
			this.songPresetRepository_ = songPresetRepository ?? throw new ArgumentNullException(nameof(songPresetRepository));

			this.SongInfo = new AnizanSongInfo();
			this.serializer_ = new Models.Serializers.AnizanListSerializer();

			//this.SearchVm.SongParsed += this.SongParsed;
			//this.SongParserVm.SongParsed += this.SongParsed;

			settings.PropertyChanged += this.settings_PropertyChanged;

			MessageService.Current.MessageObservable
				.Subscribe(message => this.StatusText = message)
				.AddTo(this.Disposables);

			this.SongsSubject.AddTo(this.Disposables);
			this.SongListWindowViewModel = new SongListWindowViewModel(settings, this.serializer_, this.SongsSubject)
				.AddTo(this.Disposables);

			this.SongListWindow = new SongListWindow
			{
				DataContext = SongListWindowViewModel,
				Topmost = this.Settings.AlwaysOnTop
			};

			this.SongListWindow.Loaded += (_, __) =>
			{
				//this.SongListWindow.Owner = mainWindow;
				//this.SongListWindow.Left = mainWindow.Left + mainWindow.Width;
				//this.SongListWindow.Top = mainWindow.Top;
				//this.SongListWindow.Height = mainWindow.Height;
			};

			//mainWindow.Closed += (_, __) =>
			//{
			//	this.SongListWindow.CloseImmediately();
			//};

			//mainWindow.LocationChanged += (_, __) =>
			//{
			//	if (mainWindow.Visibility == Visibility.Visible && this.Settings.SnapListWindow)
			//	{
			//		this.SongListWindow.Left = mainWindow.Left + mainWindow.Width;
			//		this.SongListWindow.Top = mainWindow.Top;
			//	}
			//};

			//mainWindow.SizeChanged += (_, __) =>
			//{
			//	if (mainWindow.Visibility == Visibility.Visible && this.Settings.SnapListWindow)
			//	{
			//		this.SongListWindow.Left = mainWindow.Left + mainWindow.Width;
			//		this.SongListWindow.Top = mainWindow.Top;
			//	}
			//};

			//this.SongListWindow.SizeChanged += (_, __) =>
			//{
			//	if (mainWindow.Visibility == Visibility.Visible && this.SongListWindow.Visibility == Visibility.Visible && this.Settings.SnapListWindow)
			//	{
			//		this.SongListWindow.Left = mainWindow.Left + mainWindow.Width;
			//		this.SongListWindow.Top = mainWindow.Top;
			//	}
			//};

			this.SongListWindow.IsVisibleChanged += (_, e) =>
			{
				if (this.SongListWindow.Visibility != Visibility.Visible)
				{
					this.ShowListWindow.Value = false;
				}
			};

			this.UpdateCheckerService = serviceManager.Services.First(x => x is UpdateCheckerService) as UpdateCheckerService;

			settings.PropertyChangedAsObservable()
				.Where(x => x.PropertyName == nameof(this.Settings.SnapListWindow))
				.ObserveOnDispatcher()
				.Subscribe(_ =>
				{
					//if (mainWindow.Visibility == Visibility.Visible && this.Settings.SnapListWindow)
					//{
					//	this.SongListWindow.Left = mainWindow.Left + mainWindow.Width;
					//	this.SongListWindow.Top = mainWindow.Top;
					//}
				});

			this.Settings.PropertyChangedAsObservable()
				.Where(x => x.PropertyName == nameof(this.Settings.AlwaysOnTop))
				.ObserveOnUIDispatcher()
				.Subscribe(_ =>
				{
					this.SongListWindow.Topmost = this.Settings.AlwaysOnTop;
				})
				.AddTo(this.Disposables);

			this.ShowListWindow
				.ObserveOnUIDispatcher()
				.Subscribe(show =>
				{
					if (show)
					{
						this.SongListWindow.Show();
					}
					else
					{
						if (this.SongListWindow.Visibility == Visibility.Visible)
						{
							this.SongListWindow.Hide();
						}
					}
				})
				.AddTo(this.Disposables);

			this.ShowParserControl = this.Settings
				.ToReactivePropertyAsSynchronized(x => x.ShowParserControl, mode: ReactivePropertyMode.RaiseLatestValueOnSubscribe | ReactivePropertyMode.DistinctUntilChanged)
				.AddTo(this.Disposables);

			this.ShowTagRetreiver = this.Settings
				.ToReactivePropertyAsSynchronized(x => x.ShowStreamMetadataRetreiver)
				.AddTo(this.Disposables);

			this.ShowFrequentlyPlayedSongs = this.Settings
				.ToReactivePropertyAsSynchronized(x => x.ShowFrequentlyPlayedSongs)
				.AddTo(this.Disposables);

			this.SongPresets = songPresetRepository
				.ToReactivePropertyAsSynchronized(x => x.Presets);

			this.SetToSpecialCommand = new ReactiveCommand<string>()
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

					this.SongInfo.IsSpecialItem = true;
					this.SongInfo.SpecialHeader = header;
					this.SongInfo.SpecialItemName = body;
				});

			this.ApplyPresetCommand = new ReactiveCommand<AnizanSongInfo>()
				.WithSubscribe(preset =>
				{
					if (preset == null)
					{
						return;
					}

					try
					{
						this.SongInfo.Title = preset.Title;
						this.SongInfo.Singer = preset.Singer;
						this.SongInfo.Genre = preset.Genre;
						this.SongInfo.Series = preset.Series;
						this.SongInfo.SongType = preset.SongType;
						this.SongInfo.Additional = preset.Additional;

						if (settings.ApplySongInfoAutomatically)
						{
							this.Serialize();
						}
					}
					catch (Exception ex)
					{
						this.ShowErrorMessage("曲情報の適用に失敗為ました。", ex);
					}
				},
				x => x.AddTo(this.Disposables))
				.AddTo(this.Disposables);

			this.SetPresetCommand = new ReactiveCommand<string>()
				.WithSubscribe(type =>
				{
					switch (type)
					{
						case "NOANIME":
							this.SongInfo.Genre = string.Empty;
							this.SongInfo.Series = "一般曲";
							this.SongInfo.SongType = string.Empty;
							break;

						case "CLEARALL":
							if (MessageBox.Show("記入内容を全てクリアします", "確認", MessageBoxButton.OKCancel, MessageBoxImage.Warning) == MessageBoxResult.OK)
							{
								this.SongInfo = new AnizanSongInfo();
							}
							break;
					}
				});

			this.PasteZanmaiFormatCommand = new ReactiveCommand()
				.WithSubscribe(() =>
				{
					try
					{
						var item = Clipboard.GetText()
							.Replace("\r", "")
							.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries)
							.Select(line => line.Trim())
							.Select(line => this.AnizanFormatParser.ParseAsAnizanInfo(line))
							.FirstOrDefault(x => x != null);

						if (item == null)
						{
							return;
						}

						item.IsSpecialItem = !string.IsNullOrWhiteSpace(item.SpecialHeader);
						this.SongInfo = item;
						//this.SearchVm.SearchWord = item.Title;
					}
					catch (Exception ex)
					{
						var sb = new StringBuilder();
						sb.AppendLine("クリップボードからの情報取得に失敗しました。");
						sb.AppendLine(ex.Message);

						MessageBox.Show(sb.ToString(), "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
					}
				});

			this.CheckForUpdateCommand = new AsyncReactiveCommand()
				.WithSubscribe(async () =>
				{
					try
					{
						var ret = await this.UpdateCheckerService.CheckForUpdateAndShowDialogIfAvailableAsync(false);
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
		private void SongParsed(object sender, SongParsedEventArgs e)
		{
			this.SongInfo = this.converter_.Convert(e.SongInfo);
			this.Serialize();
		}

		private void settings_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if (e.PropertyName == this.GetMemberName(() => this.Settings.ServerName) ||
				e.PropertyName == this.GetMemberName(() => this.Settings.BoardPath) ||
				e.PropertyName == this.GetMemberName(() => this.Settings.ThreadKey))
			{
				this.Dispatch(() =>
				{
					this.WriteToThreadCommand.RaiseCanExecuteChanged();
				});
			}
		}

		#endregion コンストラクタ

		#region いろいろやるやつ

		private void Serialize()
		{
			if (this.serializer_ == null) { throw new InvalidOperationException("シリアライザが設定されていません。"); }
			this.ResultText = this.SongInfo.IsSpecialItem
				? this.serializer_.SerializeFull(this.SongInfo)
				: this.serializer_.Serialize(this.SongInfo);
		}

		private void CopyToClipboard(bool appendNumber)
		{
			string format = (appendNumber ? "{0:D4}{1}" : "{1}");
			var str = string.Format(format,
				this.SongNumber, this.ResultText);
			if (string.IsNullOrEmpty("str")) { str = " "; }

			App.Current.Dispatcher.BeginInvoke((Action)(() =>
			{
				try
				{
					System.Windows.Forms.Clipboard.SetText(str);
				}
				catch (Exception ex)
				{
					this.ShowErrorMessage("コピーに失敗しました。", ex);
					return;
				}

				this.AddCurrentSongToList();

				// 番号インクリメント
				if (this.Settings.IncrementSongNumberWhenCopied && !this.SongInfo.IsSpecialItem)
				{
					this.SongNumber++;
				}

				// 入力欄クリア
				if (this.Settings.ClearInputAutomatically)
				{
					this.Dispatch(() =>
					{
						//this.SongParserVm.ClearInput();
						//this.SearchVm.ClearInput();
					});
				}
			}));
		}

		private void AddCurrentSongToList()
		{
			var info = new AnizanSongInfo
			{
				Title = this.SongInfo.Title,
				Singer = this.SongInfo.Singer,
				Genre = this.SongInfo.Genre,
				Series = this.SongInfo.Series,
				SongType = this.SongInfo.SongType,
				Additional = this.SongInfo.Additional,
			};

			if (this.SongInfo.IsSpecialItem)
			{
				info.SpecialHeader = this.SongInfo.SpecialHeader;
				info.SpecialItemName = this.SongInfo.SpecialItemName;
				info.Number = 0;
			}
			else
			{
				info.Number = this.SongNumber;
			}

			this.SongsSubject.OnNext(info);
		}

		private void WriteToThread()
		{
			var board = new BoardInfo(this.Settings.ServerName, this.Settings.BoardPath, "板名");
			var thread = new X2chThreadHeader
			{
				BoardInfo = board,
				Key = this.Settings.ThreadKey
			};

			var str = this.SongInfo.IsSpecialItem
					? this.ResultText
					: string.Format("{0:D4}{1}", this.SongNumber, this.ResultText);

			var res = new PostRes
			{
				Body = str,
			};
			if (this.Settings.WriteAsSage)
			{
				res.Email = "sage";
			}
			var post = new X2chPost();
			post.Posted += (s, e) =>
			{
				Console.WriteLine("Response: ", e.Response);
				switch (e.Response)
				{
					case PostResponse.Success:
						MessageService.Current.ShowMessage(string.Format("書き込み成功! ( {0} )",
							str.Length > 40 ? str.Substring(0, 40) + "..." : str));

						this.AddCurrentSongToList();

						// 曲番号インクリメント
						if (!this.SongInfo.IsSpecialItem && this.Settings.IncrementSongNumberWhenCopied)
						{
							this.SongNumber++;
						}

						// 入力欄クリア
						if (this.Settings.ClearInputAutomatically)
						{
							this.Dispatch(() =>
							{
								//this.SongParserVm.ClearInput();
								//this.SearchVm.ClearInput();
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
			post.Error += (s, e) =>
			{
				MessageService.Current.ShowMessage("書き込み失敗orz");
				MessageBox.Show(
					string.Format("投稿に失敗しました。\n\n【例外情報】\n{0}", e.Exception),
					"エラー", MessageBoxButton.OK, MessageBoxImage.Stop);
			};

			Task.Factory.StartNew((Action)(() =>
			{
				try
				{
					this.CanWrite = false;
					MessageService.Current.ShowMessage("書き込んでいます...");
					post.Post(thread, res);
				}
				catch (Exception ex)
				{
					MessageBox.Show(
						string.Format("投稿に失敗しました。\n\n【例外情報】\n{0}", ex),
						"エラー", MessageBoxButton.OK, MessageBoxImage.Stop);
				}
				finally
				{
					this.CanWrite = true;
				}
			}));
		}

		#endregion いろいろやるやつ

		#region Bindings

		public Settings Settings
		{
			get
			{
				return this.GetValue<Settings>();
			}
			set
			{
				this.SetValue(value);
			}
		}

		public bool CanWrite
		{
			get
			{
				return this.GetValue(true);
			}
			set
			{
				if (this.SetValue(value))
				{
					this.Dispatch(() => this.WriteToThreadCommand.RaiseCanExecuteChanged());
				}
			}
		}

		public string StatusText
		{
			get
			{
				return this.GetValue<string>();
			}
			set
			{
				this.SetValue(value);
			}
		}

		public string ResultText
		{
			get
			{
				return this.GetValue<string>();
			}
			set
			{
				if (this.SetValue(value))
				{
					this.Dispatch(() => this.WriteToThreadCommand.RaiseCanExecuteChanged());
				}
			}
		}

		public int SongNumber
		{
			get
			{
				return this.GetValue(1);
			}
			set
			{
				this.SetValue(value);
			}
		}

		#region VersionName

		private string versionName_ = null;

		public string VersionName
		{
			get
			{
				if (this.versionName_ == null)
				{
					var version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
					this.versionName_ = string.Format("{0}.{1}.{2}", version.Major, version.Minor, version.Build);
				}
				return this.versionName_;
			}
		}

		#endregion VersionName

		public ReactiveProperty<bool> ShowListWindow { get; } = new ReactiveProperty<bool>();
		public ReactiveProperty<bool> SnapListWindow { get; } = new ReactiveProperty<bool>(true);

		public ReactiveProperty<AnizanSongInfo[]> SongPresets { get; }

		public AnizanSongInfo SongInfo
		{
			get
			{
				return this.GetValue<AnizanSongInfo>();
			}
			set
			{
				this.SetValue(
					value,
					actBeforeChange: (oldValue, newValue) =>
					{
						if (oldValue != null)
						{
							oldValue.PropertyChanged -= this.SongInfo_PropertyChanged;
						}
					},
					actAfterChange: (oldValue, newValue) =>
					{
						newValue.PropertyChanged += this.SongInfo_PropertyChanged;
						this.Dispatch(() =>
						{
							this.ToTvSizeCommand.RaiseCanExecuteChanged();
							this.ToLiveVersionCommand.RaiseCanExecuteChanged();
							this.SetAdditionalCommand.RaiseCanExecuteChanged();
						});
					});
			}
		}

		public SongListWindowViewModel SongListWindowViewModel
		{
			get
			{
				return this.GetValue<SongListWindowViewModel>();
			}
			set
			{
				this.SetValue(value);
			}
		}

		private void SongInfo_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if (this.Settings.ApplySongInfoAutomatically)
			{
				this.Serialize();
			}
		}

		public ReactiveProperty<bool> ShowParserControl { get; }
		public ReactiveProperty<bool> ShowTagRetreiver { get; }
		public ReactiveProperty<bool> ShowFrequentlyPlayedSongs { get; }

		public ReactiveProperty<double> WindowWidth => this.LazyReactiveProperty(() => this.Settings.ToReactivePropertyAsSynchronized(x => x.WindowWidth));
		public ReactiveProperty<double> WindowHeight => this.LazyReactiveProperty(() => this.Settings.ToReactivePropertyAsSynchronized(x => x.WindowHeight));

		#endregion Bindings

		#region Commands

		#region ParseInputCommand

		private DelegateCommand parseInputCommand_ = null;

		public DelegateCommand ParseInputCommand
		{
			get
			{
				if (this.parseInputCommand_ == null)
				{
					this.parseInputCommand_ = new DelegateCommand
					{
						ExecuteHandler = param =>
						{
							try
							{
								this.Serialize();

								// 自動コピー
								if (this.Settings.CopyAfterParse)
								{
									this.CopyToClipboard(true);
								}
							}
							catch (Exception ex)
							{
								this.ErrMsg("曲情報の解析に失敗しました。", ex);
							}
						}
					};
				}
				return this.parseInputCommand_;
			}
		}

		#endregion ParseInputCommand

		#region ApplyDetailsCommand

		private DelegateCommand applyDetailsCommand_ = null;

		public DelegateCommand ApplyDetailsCommand
		{
			get
			{
				if (this.applyDetailsCommand_ == null)
				{
					this.applyDetailsCommand_ = new DelegateCommand
					{
						ExecuteHandler = param =>
						{
							try
							{
								this.Serialize();
								if (this.Settings.CopyAfterApply)
								{
									this.CopyToClipboard(true);
								}
							}
							catch (Exception ex)
							{
								this.ErrMsg("曲情報の適用に失敗しました。", ex);
							}
						}
					};
				}
				return this.applyDetailsCommand_;
			}
		}

		#endregion ApplyDetailsCommand

		#region CopyResultCommand

		private DelegateCommand copyResultCommand_ = null;

		public DelegateCommand CopyResultCommand
		{
			get
			{
				if (this.copyResultCommand_ == null)
				{
					this.copyResultCommand_ = new DelegateCommand
					{
						ExecuteHandler = param =>
						{
							try
							{
								this.CopyToClipboard(false);
							}
							catch (Exception ex)
							{
								this.ShowErrorMessage("コピーに失敗しました。", ex);
							}
						}
					};
				}
				return this.copyResultCommand_;
			}
		}

		#endregion CopyResultCommand

		#region CopyResultAndSongNumberCommand

		private DelegateCommand copyResultAndSongNumberCommand = null;

		public DelegateCommand CopyResultAndSongNumberCommand
		{
			get
			{
				if (this.copyResultAndSongNumberCommand == null)
				{
					this.copyResultAndSongNumberCommand = new DelegateCommand
					{
						ExecuteHandler = param =>
						{
							try
							{
								this.CopyToClipboard(true);
							}
							catch (Exception ex)
							{
								this.ShowErrorMessage("コピーに失敗しました。", ex);
							}
						}
					};
				}
				return this.copyResultAndSongNumberCommand;
			}
		}

		#endregion CopyResultAndSongNumberCommand

		#region WriteToThreadCommand

		private DelegateCommand writeToThreadCommand_ = null;

		public DelegateCommand WriteToThreadCommand
		{
			get
			{
				return this.writeToThreadCommand_ ?? (this.writeToThreadCommand_ = new DelegateCommand
				{
					ExecuteHandler = param =>
					{
						this.WriteToThread();
					},
					CanExecuteHandler = param =>
					{
						return (
							this.CanWrite &&
							!string.IsNullOrWhiteSpace(this.ResultText) &&
							!string.IsNullOrWhiteSpace(this.Settings.ServerName) &&
							!string.IsNullOrWhiteSpace(this.Settings.BoardPath) &&
							!string.IsNullOrWhiteSpace(this.Settings.ThreadKey));
					}
				});
			}
		}

		#endregion WriteToThreadCommand

		#region CheckForDictionaryUpdateCommand

		private DelegateCommand checkForDictionaryUpdateCommand_ = null;

		public DelegateCommand CheckForDictionaryUpdateCommand
		{
			get
			{
				return this.checkForDictionaryUpdateCommand_ ?? (this.checkForDictionaryUpdateCommand_ = new DelegateCommand
				{
					ExecuteHandler = param =>
					{
						var app = App.Current as App;
						if (app != null)
						{
							Task.Factory.StartNew(() =>
							{
								app.UpdateDictionary();
							});
						}
					}
				});
			}
		}

		#endregion CheckForDictionaryUpdateCommand

		#region ReloadDictionaryCommand

		private DelegateCommand reloadDictionaryCommand_ = null;

		public DelegateCommand ReloadDictionaryCommand
		{
			get
			{
				return this.reloadDictionaryCommand_ ?? (this.reloadDictionaryCommand_ = new DelegateCommand
				{
					ExecuteHandler = param =>
					{
						if (App.Current is App app)
						{
							try
							{
								app.LoadDictionaries();
								this.InfoMsg("辞書の読み込みを完了しました。");
							}
							catch (Exception ex)
							{
								this.ErrMsg("辞書データの読み込みに失敗しました。", ex);
							}
						}
					}
				});
			}
		}

		#endregion ReloadDictionaryCommand

		#region ToTvSizeCommand

		private DelegateCommand toTvSizeCommand_ = null;

		public DelegateCommand ToTvSizeCommand
		{
			get
			{
				return this.toTvSizeCommand_ ?? (this.toTvSizeCommand_ = new DelegateCommand
				{
					ExecuteHandler = param =>
					{
						var oldStr = this.SongInfo.Title ?? "";
						this.SongInfo.Title = oldStr.TrimEnd() + "(TV size)";
					},
					CanExecuteHandler = param =>
					{
						return (this.SongInfo != null);
					}
				});
			}
		}

		#endregion ToTvSizeCommand

		#region ToLiveVersionCommand

		private DelegateCommand toLiveVersionCommand_ = null;

		public DelegateCommand ToLiveVersionCommand
		{
			get
			{
				return this.toLiveVersionCommand_ ?? (this.toLiveVersionCommand_ = new DelegateCommand
				{
					ExecuteHandler = param =>
					{
						var oldStr = this.SongInfo.Title ?? "";
						this.SongInfo.Title = oldStr.TrimEnd() + "(Live ver.)";
						this.SongInfo.SongType = "AR";
					},
					CanExecuteHandler = param =>
					{
						return (this.SongInfo != null);
					}
				});
			}
		}

		#endregion ToLiveVersionCommand

		public ReactiveCommand<string> SetToSpecialCommand { get; }
		public ReactiveCommand<string> SetPresetCommand { get; }
		public ReactiveCommand PasteZanmaiFormatCommand { get; }
		public AsyncReactiveCommand CheckForUpdateCommand { get; }
		public ReactiveCommand<AnizanSongInfo> ApplyPresetCommand { get; }

		#endregion Commands

		#region SetAdditionalCommand

		private DelegateCommand setAdditionalCommand_ = null;

		public DelegateCommand SetAdditionalCommand
		{
			get
			{
				return this.setAdditionalCommand_ ?? (this.setAdditionalCommand_ = new DelegateCommand
				{
					ExecuteHandler = param =>
					{
						var str = param as string;
						if (string.IsNullOrWhiteSpace(str)) { return; }
						if (!string.IsNullOrWhiteSpace(this.SongInfo.Additional))
						{
							this.SongInfo.Additional += "," + str;
						}
						else
						{
							this.SongInfo.Additional = str;
						}
					},
					CanExecuteHandler = param =>
					{
						return (this.SongInfo != null);
					}
				});
			}
		}

		#endregion SetAdditionalCommand

		#region メッセージ

		private void InfoMsg(string message)
		{
			MessageBox.Show(message, "情報", MessageBoxButton.OK, MessageBoxImage.Information);
		}

		private void InfoMsg(string message, Exception ex)
		{
			this.InfoMsg(string.Format("{0}\n\n***例外情報***\n{1}",
				message, ex.ToString()));
		}

		private void ErrMsg(string message)
		{
			MessageBox.Show(message, "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
		}

		private void ErrMsg(string message, Exception ex)
		{
			this.ErrMsg(string.Format("{0}\n\n***例外情報***\n{1}",
				message, ex.ToString()));
		}

		#endregion メッセージ
	}
}
