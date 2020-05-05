using System;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using AnizanHelper.Models;
using AnizanHelper.Models.DbSearch;
using AnizanHelper.Models.SongList;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using Studiotaiha.LazyProperty;

namespace AnizanHelper.ViewModels.Pages
{
	public enum SongRetreiverConnectionState
	{
		Stopped,
		Running,
		Connecting,
		Reconnecting,
	}

	public class SongMetadataViewerPageViewModel : ReactiveViewModelBase
	{
		private Settings Settings { get; }
		private HttpClient HttpClient { get; }
		private ISearchController SearchManager { get; }
		private IcecastSongMetadataRetreiver songMetadataRetreiver;

		public SongMetadataViewerPageViewModel(
			Settings settings,
			HttpClient httpClient,
			ISearchController searchManager)
		{
			this.Settings = settings ?? throw new ArgumentNullException(nameof(settings));
			this.HttpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
			this.SearchManager = searchManager ?? throw new ArgumentNullException(nameof(searchManager));

			var regexTimeout = TimeSpan.FromMilliseconds(250);

			this.CurrentSongMetadata.Select(_ => Unit.Default)
				.Merge(this.RegexFormat.Select(_ => Unit.Default))
				.Throttle(regexTimeout)
				.ObserveOnUIDispatcher()
				.Subscribe(x =>
				{
					var target = this.CurrentSongMetadata.Value;
					var regexFormat = this.RegexFormat.Value;

					var treamTitle = target?.Content;

					if (treamTitle == null || regexFormat == null)
					{
						if (target != null)
						{
							target.ExtractedTitle = null;
							target.ExtractedArtist = null;
						}

						return;
					}

					try
					{
						MessageService.Current.ShowMessage("楽曲情報を解析しています...");

						var regex = new Regex(regexFormat, RegexOptions.IgnoreCase, regexTimeout);
						var sw = new Stopwatch();
						sw.Start();
						var match = regex.Match(treamTitle);
						sw.Stop();

						if (match?.Success == true)
						{
							target.ExtractedTitle = match.Groups["Title"].Value?.Trim();
							target.ExtractedArtist = match.Groups["Artist"].Value?.Trim();

							MessageService.Current.ShowMessage(string.Format(
								"楽曲情報の解析に成功しました。({0}ms) @{1}",
								(int)sw.Elapsed.TotalMilliseconds,
								DateTimeOffset.Now.ToString("HH:mm:ss.fff")));
						}
						else
						{
							target.ExtractedTitle = null;
							target.ExtractedArtist = null;

							MessageService.Current.ShowMessage(string.Format(
								"楽曲情報の解析に失敗しました。 - フォーマットにマッチしませんでした。 ({0}ms) @{1}",
								(int)sw.Elapsed.TotalMilliseconds,
								DateTimeOffset.Now.ToString("HH:mm:ss.fff")));
						}
					}
					catch (Exception ex)
					{
						target.ExtractedTitle = null;
						target.ExtractedArtist = null;

						MessageService.Current.ShowMessage(string.Format(
							"楽曲情報の自動解析に失敗しました。 - {0} @{1}",
							ex.Message,
							DateTimeOffset.Now.ToString("HH:mm:ss.fff")));
					}
				});
		}

		#region Bindings

		public ReactiveProperty<Encoding> SelectedEncoding => this.LazyReactiveProperty(() =>
		{
			var defaultEncoding = this.Settings.SongMetadatSelectedEncodingName != null
				? this.MetadataEncodings.Value.FirstOrDefault(x => x.BodyName == this.Settings.SongMetadatSelectedEncodingName) ?? this.MetadataEncodings.Value.First()
				: this.MetadataEncodings.Value.First();

			var prop = new ReactiveProperty<Encoding>(defaultEncoding);

			this.Settings.PropertyChangedAsObservable(nameof(this.Settings.SongMetadatSelectedEncodingName))
				.Select(_ => this.Settings.SongMetadatSelectedEncodingName)
				.Subscribe(encodingName =>
				{
					var encoding = this.Settings.SongMetadatSelectedEncodingName != null
						? this.MetadataEncodings.Value.FirstOrDefault(x => x.BodyName == this.Settings.SongMetadatSelectedEncodingName) ?? this.MetadataEncodings.Value.First()
						: this.MetadataEncodings.Value.First();

					if (prop.Value != encoding)
					{
						prop.Value = encoding;
					}
				});

			prop
				.Where(x => x != null)
				.Subscribe(encoding =>
				{
					if (this.Settings.SongMetadatSelectedEncodingName != encoding.BodyName)
					{
						this.Settings.SongMetadatSelectedEncodingName = encoding.BodyName;
					}

					if (this.songMetadataRetreiver != null && this.songMetadataRetreiver.MetadataEncoding != encoding)
					{
						this.songMetadataRetreiver.MetadataEncoding = encoding;
					}
				})
				.AddTo(this.Disposables);

			return prop;
		});

		public ReadOnlyReactiveProperty<string> SelectedEncodingName => this.LazyReadOnlyReactiveProperty(() => this.SelectedEncoding.Select(x => x.EncodingName).ToReadOnlyReactiveProperty());

		public ReactiveProperty<Encoding[]> MetadataEncodings { get; } = new ReactiveProperty<Encoding[]>(Constants.MetadataEncodings);

		public ReactiveProperty<CancellationTokenSource> RetreiverCancellationTokenSource => this.LazyReactiveProperty(() =>
		{
			var prop = new ReactiveProperty<CancellationTokenSource>();

			prop.Where(x => x != null)
				.SelectMany(async cts =>
				{
					var shouldRetry = false;

					try
					{
						if (this.RetreiverState.Value == SongRetreiverConnectionState.Stopped)
						{
							this.RetreiverState.Value = SongRetreiverConnectionState.Connecting;
						}

						using (var disposables = new CompositeDisposable())
						{
							var streamUri = new Uri(this.StreamUri.Value);
							var retreiver = this.songMetadataRetreiver = new IcecastSongMetadataRetreiver(this.HttpClient, streamUri, this.SelectedEncoding.Value);

							Observable.FromEventPattern<ISongMetadata>(retreiver, nameof(ISongMetadataRetreiver.SongMetadataReceived))
								.Select(x => x.EventArgs)
								.ObserveOnUIDispatcher()
								.Subscribe(songMetadata =>
								{
									var historyItem = this.SongMetadataHistory.FirstOrDefault(x => x.Id == songMetadata.Id);
									if (historyItem == null)
									{
										historyItem = new SongHistoryItem
										{
											Id = songMetadata.Id,
											Timestamp = songMetadata.Timestamp,
											Content = songMetadata.StreamTitle,
										};

										this.SongMetadataHistory.Insert(0, historyItem);
									}
									else
									{
										historyItem.Timestamp = songMetadata.Timestamp;
										historyItem.Content = songMetadata.StreamTitle;
									}

									this.CurrentSongMetadata.Value = null;
									this.CurrentSongMetadata.Value = historyItem;
									this.RetreiverState.Value = SongRetreiverConnectionState.Running;
								})
								.AddTo(disposables);

							var stopStatus = await retreiver.RunAsync(cts.Token);

							switch (stopStatus)
							{
								case SongListFeedStopStatus.Unknown:
									MessageService.Current.ShowMessage("タグの取得が異常終了しました。");
									this.RetreiverState.Value = SongRetreiverConnectionState.Stopped;
									break;

								case SongListFeedStopStatus.MetadataNotSupported:
									MessageService.Current.ShowMessage("このストリームはタグの配信に対応していません。");
									this.RetreiverState.Value = SongRetreiverConnectionState.Stopped;
									break;

								case SongListFeedStopStatus.StreamClosed:
									if (this.Settings.EnableMetadataStreamAutoReconnection)
									{
										if (this.CurrentRetryCount.Value < this.Settings.MaxMetadataStreamAutoReconnectionTrialCount)
										{
											MessageService.Current.ShowMessage($"タグ取得用ストリームが切断されました。再接続しています... ({this.CurrentRetryCount.Value + 1}回目)");
											shouldRetry = true;
										}
										else
										{
											MessageService.Current.ShowMessage($"タグ取得用ストリームが切断されました。最大再接続回数に達しました。({this.Settings.MaxMetadataStreamAutoReconnectionTrialCount}回)");
										}
									}
									else
									{
										MessageService.Current.ShowMessage("タグ取得用ストリームが切断されました。");
									}
									break;
							}
						}
					}
					catch (OperationCanceledException)
					{
						MessageService.Current.ShowMessage("タグの取得が停止されました。");
					}
					catch (Exception ex)
					{
						if (this.Settings.EnableMetadataStreamAutoReconnection)
						{
							if (this.CurrentRetryCount.Value < this.Settings.MaxMetadataStreamAutoReconnectionTrialCount)
							{
								MessageService.Current.ShowMessage($"タグ取得用ストリームへの接続に失敗しました。再接続しています... ({this.CurrentRetryCount.Value + 1}回目)");
								shouldRetry = true;
							}
							else
							{
								MessageService.Current.ShowMessage($"タグ取得用ストリームが切断されました。最大再接続回数に達しました。({this.Settings.MaxMetadataStreamAutoReconnectionTrialCount}回)");
							}
						}
						else
						{
							this.ShowErrorMessage("タグの取得に失敗しました。", ex);
						}
					}
					finally
					{
						this.songMetadataRetreiver = null;

						if (shouldRetry)
						{
							this.CurrentSongMetadata.Value = null;

							try
							{
								this.RetreiverState.Value = SongRetreiverConnectionState.Reconnecting;
								this.CurrentRetryCount.Value++;
								await Task.Delay(this.Settings.MetadataStreamReconnectionInterval, cts.Token);

								this.RetreiverCancellationTokenSource.Value = null;
								this.RetreiverCancellationTokenSource.Value = new CancellationTokenSource();
							}
							catch (OperationCanceledException)
							{
								this.RetreiverCancellationTokenSource.Value = null;
								this.RetreiverState.Value = SongRetreiverConnectionState.Stopped;
								MessageService.Current.ShowMessage("タグの取得が停止されました。");
							}
							finally
							{
								cts.Dispose();
							}
						}
						else
						{
							this.RetreiverCancellationTokenSource.Value = null;
							cts.Dispose();
							this.RetreiverState.Value = SongRetreiverConnectionState.Stopped;
						}
					}

					return cts;
				})
				.Subscribe()
				.AddTo(this.Disposables);

			return prop;
		});

		public ReactiveProperty<SongHistoryItem> CurrentSongMetadata { get; } = new ReactiveProperty<SongHistoryItem>();
		public ReactiveCollection<SongHistoryItem> SongMetadataHistory { get; } = new ReactiveCollection<SongHistoryItem>();
		public ReactiveProperty<SongHistoryItem> SelectedHistoryItem { get; } = new ReactiveProperty<SongHistoryItem>();

		public ReactiveProperty<SongRetreiverConnectionState> RetreiverState => this.LazyReactiveProperty(() =>
		{
			var prop = new ReactiveProperty<SongRetreiverConnectionState>();

			prop.Subscribe(state =>
				{
					switch (state)
					{
						case SongRetreiverConnectionState.Stopped:
							this.CurrentRetryCount.Value = 0;
							break;

						case SongRetreiverConnectionState.Running:
							MessageService.Current.ShowMessage("タグ取得用ストリームへ接続しました。");
							this.CurrentRetryCount.Value = 0;
							break;
					}
				})
				.AddTo(this.Disposables);

			return prop;
		});

		public ReactiveProperty<string> StreamUri => this.LazyReactiveProperty(() => this.Settings.ToReactivePropertyAsSynchronized(x => x.MetadataStreamUri));
		public ReactiveProperty<bool> EnableAutoReconnection => this.LazyReactiveProperty(() => this.Settings.ToReactivePropertyAsSynchronized(x => x.EnableMetadataStreamAutoReconnection));
		public ReactiveProperty<bool> ShowHistory => this.LazyReactiveProperty(() => this.Settings.ToReactivePropertyAsSynchronized(x => x.ShowMetadataStreamHistory));
		public ReactiveProperty<bool> ShowSongInfoExtractorControl => this.LazyReactiveProperty(() => this.Settings.ToReactivePropertyAsSynchronized(x => x.ShowSongInfoExtractorControl));

		public ReactiveProperty<string> RegexFormat => this.LazyReactiveProperty(() =>
		{
			var prop = this.Settings.ToReactivePropertyAsSynchronized(x => x.SongInfoExtractorRegexFormat);

			return prop;
		});

		public ReactiveProperty<string[]> RegexFormatPresets => this.LazyReactiveProperty(() => this.Settings.ToReactivePropertyAsSynchronized(x => x.SongInfoExtractorPresets));

		public ReactiveProperty<int> CurrentRetryCount { get; } = new ReactiveProperty<int>();

		#endregion Bindings

		#region Commands

		public ICommand StartRetreivingCommand => this.LazyReactiveCommand(
			new[] {
				this.RetreiverCancellationTokenSource.Select(x => x == null),
				this.StreamUri.Select(x => !string.IsNullOrWhiteSpace(x)),
				this.RetreiverState.Select(x => x == SongRetreiverConnectionState.Stopped),
			}
			.CombineLatestValuesAreAllTrue(),
			() =>
			{
				this.RetreiverCancellationTokenSource.Value = new CancellationTokenSource();
			});

		public ICommand StopRetreivingCommand => this.LazyReactiveCommand(
			this.RetreiverState.Select(x => x != SongRetreiverConnectionState.Stopped),
			() =>
			{
				this.RetreiverCancellationTokenSource.Value?.Cancel();
			});

		public ICommand ClearHistoryCommand => this.LazyReactiveCommand(
			this.SongMetadataHistory
				.CollectionChangedAsObservable()
				.Select(_ => this.SongMetadataHistory.Count != 0),
			() =>
			{
				this.SongMetadataHistory.Clear();
			});

		public ICommand SearchCommand => this.LazyReactiveCommand<string>(
			searchTerm =>
			{
				if (!string.IsNullOrWhiteSpace(searchTerm))
				{
					this.SearchManager.TriggerSearch(searchTerm);
				}
				else
				{
					MessageService.Current.ShowMessage("検索ワードが空のため検索できません。");
				}
			});

		public ICommand SaveRegexFormatCommand => this.LazyReactiveCommand(
			this.RegexFormat.Select(_ => Unit.Default)
				.Merge(this.RegexFormatPresets.Select(_ => Unit.Default))
				.Select(_ =>
				{
					var format = this.RegexFormat.Value;
					var presets = this.RegexFormatPresets.Value;

					return !string.IsNullOrWhiteSpace(format) && presets?.Contains(format) != true;
				}),
			() =>
			{
				var regexFormat = this.RegexFormat.Value;

				if (this.Settings.SongInfoExtractorPresets?.Contains(regexFormat) == true)
				{
					MessageService.Current.ShowMessage(string.Format(
						"この自動解析フォーマットは既にプリセットに登録されています。 - {0}",
						regexFormat));
				}
				else
				{
					this.Settings.SongInfoExtractorPresets = (this.Settings.SongInfoExtractorPresets ?? Array.Empty<string>())
						.Concat(new string[] { regexFormat })
						.ToArray();
				}
			});

		public ICommand SetRegexFormatCommand => this.LazyReactiveCommand<string>(
			format =>
			{
				if (format != null)
				{
					this.RegexFormat.Value = format;
				}
			});

		public ICommand RemoveRegexFormatCommand => this.LazyReactiveCommand<string>(
			format =>
			{
				if (format != null)
				{
					this.Settings.SongInfoExtractorPresets = (this.Settings.SongInfoExtractorPresets ?? Array.Empty<string>())
						.Except(new string[] { format })
						.ToArray();
				}
			});

		public ICommand RestoreDefaultPresetsCommand => this.LazyReactiveCommand(
			() =>
			{
				this.Settings.SongInfoExtractorPresets = Models.Settings.DefaultSongInfoExtractorPresets
					.Concat(this.Settings.SongInfoExtractorPresets ?? Array.Empty<string>())
					.Distinct()
					.ToArray();
			});

		public ICommand ExtractHistoryCommand => this.LazyReactiveCommand(
			() =>
			{
				try
				{
					MessageService.Current.ShowMessage("履歴の楽曲情報を解析しています...");
					var regexTimeout = TimeSpan.FromMilliseconds(250);
					var regex = new Regex(this.RegexFormat.Value, RegexOptions.IgnoreCase, regexTimeout);

					foreach (var item in this.SongMetadataHistory.ToArray())
					{
						try
						{
							var sw = new Stopwatch();
							sw.Start();
							var match = regex.Match(item.Content);
							sw.Stop();

							if (match?.Success == true)
							{
								item.ExtractedTitle = match.Groups["Title"].Value?.Trim();
								item.ExtractedArtist = match.Groups["Artist"].Value?.Trim();

								MessageService.Current.ShowMessage(string.Format(
									"楽曲情報の解析に成功しました。({0}ms) @{1}",
									(int)sw.Elapsed.TotalMilliseconds,
									DateTimeOffset.Now.ToString("HH:mm:ss.fff")));
							}
							else
							{
								item.ExtractedTitle = null;
								item.ExtractedArtist = null;

								MessageService.Current.ShowMessage(string.Format(
									"楽曲情報の解析に失敗しました。 - フォーマットにマッチしませんでした。 ({0}ms) @{1}",
									(int)sw.Elapsed.TotalMilliseconds,
									DateTimeOffset.Now.ToString("HH:mm:ss.fff")));
							}
						}
						catch (RegexMatchTimeoutException)
						{
							throw;
						}
						catch (Exception ex)
						{
							item.ExtractedTitle = null;
							item.ExtractedArtist = null;

							MessageService.Current.ShowMessage(string.Format(
								"楽曲情報の自動解析に失敗しました。 - {0} @{1}",
								ex.Message,
								DateTimeOffset.Now.ToString("HH:mm:ss.fff")));
						}
					}
				}
				catch (Exception ex)
				{
					MessageService.Current.ShowMessage(string.Format(
						"楽曲情報の自動解析に失敗しました。 - {0} @{1}",
						ex.Message,
						DateTimeOffset.Now.ToString("HH:mm:ss.fff")));
				}
			});

		#endregion Commands

		public class SongHistoryItem : ViewModelBase
		{
			public Guid Id
			{
				get => this.GetValue<Guid>();
				set => this.SetValue(value);
			}

			public string Content
			{
				get => this.GetValue<string>();
				set => this.SetValue(value);
			}

			public DateTimeOffset Timestamp
			{
				get => this.GetValue<DateTimeOffset>();
				set => this.SetValue(value);
			}

			public string ExtractedTitle
			{
				get => this.GetValue<string>();
				set => this.SetValue(value);
			}

			public string ExtractedArtist
			{
				get => this.GetValue<string>();
				set => this.SetValue(value);
			}
		}
	}
}
