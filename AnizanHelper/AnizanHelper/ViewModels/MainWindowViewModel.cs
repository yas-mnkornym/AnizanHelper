using System;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Security.AccessControl;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using AnizanHelper.Models;
using AnizanHelper.Models.Parsers;
using AnizanHelper.Models.Searching.Zanmai;
using AnizanHelper.Models.Serializers;
using AnizanHelper.Modules.Dictionaries;
using AnizanHelper.Services;
using AnizanHelper.ViewModels.Events;
using AnizanHelper.Views;
using Prism.Events;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using Studiotaiha.LazyProperty;
using Twin;
using Twin.Bbs;
using Unity;

namespace AnizanHelper.ViewModels
{
	internal class MainWindowViewModel : ReactiveViewModelBase, IDisposable
	{
		private AnizanFormatParser AnizanFormatParser { get; } = new AnizanFormatParser();
		private IServiceManager ServiceManager { get; }
		private Settings Settings { get; }
		private AnizanSongInfoConverter SongInfoConverter { get; }
		private ISongInfoSerializer SongInfoSerializer { get; } = new AnizanListSerializer();
		private SongListWindow SongListWindow { get; }
		private SongPresetRepository SongPresetRepository { get; }
		private Subject<AnizanSongInfo> SongsSubject { get; }
		private IDictionaryManager DictionaryManager { get; }
		private IEventAggregator EventAggregator { get; }
		private IUnityContainer UnityContainer { get; }

		public MainWindowViewModel(
			Settings settings,
			AnizanSongInfoConverter songInfoConverter,
			SongPresetRepository songPresetRepository,
			IDictionaryManager dictionaryManager,
			IServiceManager serviceManager,
			IEventAggregator eventAggregator,
			IUnityContainer unityContainer)
		{
			this.Settings = settings ?? throw new ArgumentNullException(nameof(settings));
			this.SongInfoConverter = songInfoConverter ?? throw new ArgumentNullException(nameof(songInfoConverter));
			this.SongPresetRepository = songPresetRepository ?? throw new ArgumentNullException(nameof(songPresetRepository));
			this.DictionaryManager = dictionaryManager ?? throw new ArgumentNullException(nameof(dictionaryManager));
			this.ServiceManager = serviceManager ?? throw new ArgumentNullException(nameof(serviceManager));
			this.EventAggregator = eventAggregator ?? throw new ArgumentNullException(nameof(eventAggregator));
			this.UnityContainer = unityContainer ?? throw new ArgumentNullException(nameof(unityContainer));

			//this.SearchVm.SongParsed += this.SongParsed;
			//this.SongParserVm.SongParsed += this.SongParsed;

			MessageService.Current.MessageObservable
				.ObserveOnUIDispatcher()
				.Subscribe(message => this.StatusText.Value = message)
				.AddTo(this.Disposables);

			this.EventAggregator
				.GetEvent<SongParsedEvent>()
				.Subscribe(songInfo =>
				{
					this.SongInfo.Value = this.SongInfoConverter.Convert(songInfo);
					this.Serialize();
				})
				.AddTo(this.Disposables);

			this.SongsSubject = new Subject<AnizanSongInfo>().AddTo(this.Disposables);
			this.SongListWindowViewModel = new SongListWindowViewModel(settings, this.SongInfoSerializer, this.SongsSubject)
				.AddTo(this.Disposables);

			this.SongListWindow = new SongListWindow
			{
				DataContext = SongListWindowViewModel,
				Topmost = this.Settings.AlwaysOnTop
			};

			var mainWindow = App.Current.MainWindow;

			this.SongListWindow.Loaded += (_, __) =>
			{
				this.SongListWindow.Owner = mainWindow;
				this.SongListWindow.Left = mainWindow.Left + mainWindow.Width;
				this.SongListWindow.Top = mainWindow.Top;
				this.SongListWindow.Height = mainWindow.Height;
			};

			mainWindow.Closed += (_, __) =>
			{
				this.SongListWindow.CloseImmediately();
			};

			mainWindow.LocationChanged += (_, __) =>
			{
				if (mainWindow.Visibility == Visibility.Visible && this.Settings.SnapListWindow)
				{
					this.SongListWindow.Left = mainWindow.Left + mainWindow.Width;
					this.SongListWindow.Top = mainWindow.Top;
				}
			};

			mainWindow.SizeChanged += (_, __) =>
			{
				if (mainWindow.Visibility == Visibility.Visible && this.Settings.SnapListWindow)
				{
					this.SongListWindow.Left = mainWindow.Left + mainWindow.Width;
					this.SongListWindow.Top = mainWindow.Top;
				}
			};

			this.SongListWindow.SizeChanged += (_, __) =>
			{
				if (mainWindow.Visibility == Visibility.Visible && this.SongListWindow.Visibility == Visibility.Visible && this.Settings.SnapListWindow)
				{
					this.SongListWindow.Left = mainWindow.Left + mainWindow.Width;
					this.SongListWindow.Top = mainWindow.Top;
				}
			};

			this.SongListWindow.IsVisibleChanged += (_, e) =>
			{
				if (this.SongListWindow.Visibility != Visibility.Visible)
				{
					this.ShowListWindow.Value = false;
				}
			};

			settings.PropertyChangedAsObservable()
				.Where(x => x.PropertyName == nameof(this.Settings.SnapListWindow))
				.ObserveOnDispatcher()
				.Subscribe(_ =>
				{
					if (mainWindow.Visibility == Visibility.Visible && this.Settings.SnapListWindow)
					{
						this.SongListWindow.Left = mainWindow.Left + mainWindow.Width;
						this.SongListWindow.Top = mainWindow.Top;
					}
				});

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
		}

		#region いろいろやるやつ

		private void AddCurrentSongToList()
		{
			var songInfo = this.SongInfo.Value;
			var info = new AnizanSongInfo
			{
				Title = songInfo.Title,
				Artist = songInfo.Artist,
				Genre = songInfo.Genre,
				Series = songInfo.Series,
				SongType = songInfo.SongType,
				Additional = songInfo.Additional,
			};

			if (songInfo.IsSpecialItem)
			{
				info.SpecialHeader = songInfo.SpecialHeader;
				info.SpecialItemName = songInfo.SpecialItemName;
				info.Number = 0;
			}
			else
			{
				info.Number = this.SongNumber.Value;
			}

			this.SongsSubject.OnNext(info);
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
				var songInfo = this.SongInfo.Value;
				if (this.Settings.IncrementSongNumberWhenCopied && !songInfo.IsSpecialItem)
				{
					this.SongNumber.Value++;
				}

				// 入力欄クリア
				if (this.Settings.ClearInputAutomatically)
				{
					this.EventAggregator.GetEvent<ClearSearchInputEvent>().Publish();
				}
			}));
		}

		private void Serialize()
		{
			var songInfo = this.SongInfo.Value;
			if (songInfo != null)
			{
				this.ResultText.Value = songInfo.IsSpecialItem
					? this.SongInfoSerializer.SerializeFull(songInfo)
					: this.SongInfoSerializer.Serialize(songInfo);
			}
		}

		private void WriteToThread()
		{
			var board = new BoardInfo(this.Settings.ServerName, this.Settings.BoardPath, "板名");
			var thread = new X2chThreadHeader
			{
				BoardInfo = board,
				Key = this.Settings.ThreadKey
			};

			var songInfo = this.SongInfo.Value;
			var str = songInfo.IsSpecialItem
					? this.ResultText.Value
					: string.Format("{0:D4}{1}", this.SongNumber, this.ResultText.Value);

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
						if (!songInfo.IsSpecialItem && this.Settings.IncrementSongNumberWhenCopied)
						{
							this.SongNumber.Value++;
						}

						// 入力欄クリア
						if (this.Settings.ClearInputAutomatically)
						{
							this.EventAggregator.GetEvent<ClearSearchInputEvent>().Publish();
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
					this.CanWrite.Value = false;
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
					this.CanWrite.Value = true;
				}
			}));
		}

		#endregion いろいろやるやつ

		#region Bindings

		#region Settings

		public ReactiveProperty<bool> IncrementSongNumberWhenCopied => this.LazyReactiveProperty(() => this.Settings.ToReactivePropertyAsSynchronized(x => x.ApplySongInfoAutomatically));
		public ReactiveProperty<bool> ShowWindowAlwaysOnTop => this.LazyReactiveProperty(() => this.Settings.ToReactivePropertyAsSynchronized(x => x.AlwaysOnTop));
		public ReactiveProperty<bool> ApplySongInfoAutomatically => this.LazyReactiveProperty(() => this.Settings.ToReactivePropertyAsSynchronized(x => x.ApplySongInfoAutomatically));
		public ReactiveProperty<string> BoardPath => this.LazyReactiveProperty(() => this.Settings.ToReactivePropertyAsSynchronized(x => x.BoardPath));
		public ReactiveProperty<bool> ClearInputAutomatically => this.LazyReactiveProperty(() => this.Settings.ToReactivePropertyAsSynchronized(x => x.ClearInputAutomatically));
		public ReactiveProperty<bool> CopyAfterApply => this.LazyReactiveProperty(() => this.Settings.ToReactivePropertyAsSynchronized(x => x.CopyAfterApply));
		public ReactiveProperty<string> ServerName => this.LazyReactiveProperty(() => this.Settings.ToReactivePropertyAsSynchronized(x => x.ServerName));
		public ReactiveProperty<bool> ShowListWindow => this.LazyReactiveProperty(() => this.Settings.ToReactivePropertyAsSynchronized(x => x.ShowListWindow));
		public ReactiveProperty<bool> ShowParserControl => this.LazyReactiveProperty(() => this.Settings.ToReactivePropertyAsSynchronized(x => x.ShowParserControl));
		public ReactiveProperty<bool> ShowTagRetreiver => this.LazyReactiveProperty(() => this.Settings.ToReactivePropertyAsSynchronized(x => x.ShowStreamMetadataRetreiver));
		public ReactiveProperty<bool> SnapListWindow => this.LazyReactiveProperty(() => this.Settings.ToReactivePropertyAsSynchronized(x => x.SnapListWindow));
		public ReactiveProperty<string> ThreadKey => this.LazyReactiveProperty(() => this.Settings.ToReactivePropertyAsSynchronized(x => x.ThreadKey));
		public ReactiveProperty<bool> WriteAsSage => this.LazyReactiveProperty(() => this.Settings.ToReactivePropertyAsSynchronized(x => x.WriteAsSage));
		public ReactiveProperty<bool> ShowFrequentlyPlayedSongs => this.LazyReactiveProperty(() => this.Settings.ToReactivePropertyAsSynchronized(x => x.ShowFrequentlyPlayedSongs));

		#endregion Settings

		public ReactiveProperty<bool> CanWrite { get; } = new ReactiveProperty<bool>();
		public ReactiveProperty<string> ResultText { get; } = new ReactiveProperty<string>();

		public ReactiveProperty<AnizanSongInfo> SongInfo => this.LazyReactiveProperty(() =>
		{
			var prop = new ReactiveProperty<AnizanSongInfo>(new AnizanSongInfo());

			prop.PropertyChanged += this.SongInfo_PropertyChanged;

			prop.Pairwise()
				.Subscribe(x =>
				{
					if (x.OldItem != null)
					{
						x.OldItem.PropertyChanged -= this.SongInfo_PropertyChanged;
					}

					if (x.NewItem != null)
					{
						x.NewItem.PropertyChanged += this.SongInfo_PropertyChanged;
						this.Serialize();
					}
				})
				.AddTo(this.Disposables);

			return prop;
		});

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

		public ReactiveProperty<int> SongNumber { get; } = new ReactiveProperty<int>();
		public ReadOnlyReactiveProperty<AnizanSongInfo[]> SongPresets => this.LazyReadOnlyReactiveProperty(() => this.SongPresetRepository.ToReadOnlyReactiveProperty(x => x.Presets));
		public ReactiveProperty<string> StatusText { get; } = new ReactiveProperty<string>();

		public ReactiveProperty<string> VersionName => this.LazyReactiveProperty<string>(() =>
		{
			var version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
			var versionName = string.Format("{0}.{1}.{2}", version.Major, version.Minor, version.Build);
			return new ReactiveProperty<string>(versionName);
		});

		public ReactiveProperty<double> WindowHeight => this.LazyReactiveProperty(() => this.Settings.ToReactivePropertyAsSynchronized(x => x.WindowHeight));

		public ReactiveProperty<double> WindowWidth => this.LazyReactiveProperty(() => this.Settings.ToReactivePropertyAsSynchronized(x => x.WindowWidth));

		private void SongInfo_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if (this.Settings.ApplySongInfoAutomatically)
			{
				this.Serialize();
			}
		}

		#endregion Bindings

		#region Commands

		public ICommand ApplyDetailsCommand => this.LazyReactiveCommand(() =>
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
		});

		public ICommand ApplyPresetCommand => this.LazyReactiveCommand<AnizanSongInfo>(
			preset =>
			{
				if (preset == null)
				{
					return;
				}

				try
				{
					var songInfo = this.SongInfo.Value;
					songInfo.Title = preset.Title;
					songInfo.Artist = preset.Artist;
					songInfo.Genre = preset.Genre;
					songInfo.Series = preset.Series;
					songInfo.SongType = preset.SongType;
					songInfo.Additional = preset.Additional;

					if (this.Settings.ApplySongInfoAutomatically)
					{
						this.Serialize();
					}
				}
				catch (Exception ex)
				{
					this.ShowErrorMessage("曲情報の適用に失敗為ました。", ex);
				}
			});

		public ICommand CheckForDictionaryUpdateCommand => this.LazyAsyncReactiveCommand(
			async () =>
			{
				var service = this.ServiceManager.Services
					.OfType<DictionaryUpdaterService>()
					.FirstOrDefault();

				if (service != null)
				{
					await service.CheckForUpdateAsync();
				}
			});

		public ICommand CheckForUpdateCommand => this.LazyAsyncReactiveCommand(
			async () =>
			{
				try
				{
					var service = this.ServiceManager.Services.OfType<UpdateCheckerService>().FirstOrDefault();
					if (service == null) { return; }

					var ret = await service.CheckForUpdateAndShowDialogIfAvailableAsync(false);
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
			});

		public ICommand CopyResultAndSongNumberCommand => this.LazyReactiveCommand(() =>
		{
			try
			{
				this.CopyToClipboard(true);
			}
			catch (Exception ex)
			{
				this.ShowErrorMessage("コピーに失敗しました。", ex);
			}
		});

		public ICommand CopyResultCommand => this.LazyReactiveCommand(() =>
		{
			try
			{
				this.CopyToClipboard(false);
			}
			catch (Exception ex)
			{
				this.ShowErrorMessage("コピーに失敗しました。", ex);
			}
		});

		public ICommand ParseInputCommand => this.LazyReactiveCommand(() =>
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
		});

		public ICommand PasteZanmaiFormatCommand => this.LazyReactiveCommand(() =>
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
				this.SongInfo.Value = item;
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

		public ICommand ReloadDictionaryCommand => this.LazyAsyncReactiveCommand(
			async () =>
			{
				try
				{
					await this.DictionaryManager.LoadAsync().ConfigureAwait(false);
					MessageService.Current.ShowMessage("辞書を読み込みました。");
				}
				catch (Exception ex)
				{
					this.ErrMsg("辞書データの読み込みに失敗しました。", ex);
				}
			});

		public ICommand SetAdditionalCommand => this.LazyReactiveCommand<string>(
			this.SongInfo.Select(x => x != null),
			text =>
			{
				var songInfo = this.SongInfo.Value;
				if (string.IsNullOrWhiteSpace(text)) { return; }
				if (!string.IsNullOrWhiteSpace(songInfo.Additional))
				{
					songInfo.Additional += "," + text;
				}
				else
				{
					songInfo.Additional = text;
				}
			});

		public ICommand SetPresetCommand => this.LazyReactiveCommand<string>(
			type =>
			{
				var songInfo = this.SongInfo.Value;
				switch (type)
				{
					case "NOANIME":
						songInfo.Genre = string.Empty;
						songInfo.Series = "一般曲";
						songInfo.SongType = string.Empty;
						break;

					case "CLEARALL":
						if (MessageBox.Show("記入内容を全てクリアします", "確認", MessageBoxButton.OKCancel, MessageBoxImage.Warning) == MessageBoxResult.OK)
						{
							this.SongInfo.Value = new AnizanSongInfo();
						}
						break;
				}
			});

		public ICommand SetToSpecialCommand => this.LazyReactiveCommand<string>(
			type =>
			{
				var songInfo = this.SongInfo.Value;
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

				songInfo.IsSpecialItem = true;
				songInfo.SpecialHeader = header;
				songInfo.SpecialItemName = body;
			});

		public ICommand ToLiveVersionCommand => this.LazyReactiveCommand(
			this.SongInfo.Select(x => x != null),
			() =>
			{
				var songInfo = this.SongInfo.Value;
				var oldStr = songInfo.Title ?? "";
				songInfo.Title = oldStr.TrimEnd() + "(Live ver.)";
				songInfo.SongType = "AR";
			});

		public ICommand ToTvSizeCommand => this.LazyReactiveCommand(
			this.SongInfo.Select(x => x != null),
			() =>
			{
				var songInfo = this.SongInfo.Value;
				var oldStr = songInfo.Title ?? "";
				songInfo.Title = oldStr.TrimEnd() + "(TV size)";
			});

		public ICommand WriteToThreadCommand => this.LazyReactiveCommand(
			new[]{
				this.CanWrite,
				this.ResultText.Select(x => !string.IsNullOrWhiteSpace(x)),
				this.ServerName.Select(x => !string.IsNullOrWhiteSpace(x)),
				this.BoardPath.Select(x => !string.IsNullOrWhiteSpace(x)),
				this.ThreadKey.Select(x => !string.IsNullOrWhiteSpace(x)),
			}
			.CombineLatestValuesAreAllTrue(),
			() =>
			{
				this.WriteToThread();
			});

		public ICommand GenerateZanmaiSearchIndexCommand => this.LazyAsyncReactiveCommand(
			async () =>
			{
				try
				{
					var generator = this.UnityContainer.Resolve<ZanmaiWikiSearchIndexGenerator>();
					using (var cts = new CancellationTokenSource())
					using (var fs = new FileStream(AppInfo.Current.ZanmaiSearchIndexPath, FileMode.Create, FileAccess.Write, FileShare.Read))
					{
						await generator.CreateIndexAsync(fs, cts.Token);
					}
				}
				catch (Exception ex)
				{
					this.ErrMsg("三昧サーチ用のインデックス作成に失敗しました。", ex);
				}
			});

		#endregion Commands

		#region メッセージ

		private void ErrMsg(string message)
		{
			MessageBox.Show(message, "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
		}

		private void ErrMsg(string message, Exception ex)
		{
			this.ErrMsg(string.Format("{0}\n\n***例外情報***\n{1}",
				message, ex.ToString()));
		}

		private void InfoMsg(string message)
		{
			MessageBox.Show(message, "情報", MessageBoxButton.OK, MessageBoxImage.Information);
		}

		private void InfoMsg(string message, Exception ex)
		{
			this.InfoMsg(string.Format("{0}\n\n***例外情報***\n{1}",
				message, ex.ToString()));
		}

		#endregion メッセージ
	}
}
