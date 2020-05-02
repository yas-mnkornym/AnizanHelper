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

namespace AnizanHelper.ViewModels
{
	public enum SongRetreiverConnectionState
	{
		Stopped,
		Running,
		Connecting,
		Reconnecting,
	}

	public class SongMetadataViewerControlViewModel : ReactiveViewModelBase
	{
		private Settings Settings { get; }
		private HttpClient HttpClient { get; }
		private ISearchManager SearchManager { get; }

		public SongMetadataViewerControlViewModel(
			Settings settings,
			HttpClient httpClient,
			ISearchManager searchManager)
		{
			Settings = settings ?? throw new ArgumentNullException(nameof(settings));
			HttpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
			SearchManager = searchManager ?? throw new ArgumentNullException(nameof(searchManager));

			var regexTimeout = TimeSpan.FromMilliseconds(250);

			this.CurrentSongTitle
				.Merge(this.RegexFormat)
				.Throttle(regexTimeout)
				.ObserveOnUIDispatcher()
				.Subscribe(x =>
				{
					var songTitle = this.CurrentSongTitle.Value;
					var regexFormat = this.RegexFormat.Value;

					if (songTitle == null || regexFormat == null)
					{
						this.ExtractedSongTitle.Value = null;
						this.ExtractedArtist.Value = null;

						return;
					}

					try
					{
						MessageService.Current.ShowMessage("楽曲情報を解析しています...");

						var regex = new Regex(regexFormat, RegexOptions.IgnoreCase, regexTimeout);
						var sw = new Stopwatch();
						sw.Start();
						var match = regex.Match(songTitle);
						sw.Stop();

						if (match?.Success == true)
						{
							this.ExtractedSongTitle.Value = match.Groups["Title"].Value?.Trim();
							this.ExtractedArtist.Value = match.Groups["Artist"].Value?.Trim();

							MessageService.Current.ShowMessage(string.Format(
								"楽曲情報の解析に成功しました。({0}ms) @{1}",
								(int)sw.Elapsed.TotalMilliseconds,
								DateTimeOffset.Now.ToString("HH:mm:ss.fff")));
						}
						else
						{
							this.ExtractedSongTitle.Value = null;
							this.ExtractedArtist.Value = null;

							MessageService.Current.ShowMessage(string.Format(
								"楽曲情報の解析に失敗しました。 - フォーマットにマッチしませんでした。 ({0}ms) @{1}",
								(int)sw.Elapsed.TotalMilliseconds,
								DateTimeOffset.Now.ToString("HH:mm:ss.fff")));
						}
					}
					catch (Exception ex)
					{
						this.ExtractedSongTitle.Value = null;
						this.ExtractedArtist.Value = null;

						MessageService.Current.ShowMessage(string.Format(
							"楽曲情報の自動解析に失敗しました。 - {0} @{1}",
							ex.Message,
							DateTimeOffset.Now.ToString("HH:mm:ss.fff")));
					}
				});
		}

		#region Bindings

		public ReactiveProperty<CancellationTokenSource> RetreiverCancellationTokenSource => this.LazyReactiveProperty(() =>
		{
			var prop = new ReactiveProperty<CancellationTokenSource>();

			prop.Where(x => x != null)
				.SelectMany(async cts =>
				{
					var shouldRetry = false;

					try
					{
						if (RetreiverState.Value == SongRetreiverConnectionState.Stopped)
						{
							RetreiverState.Value = SongRetreiverConnectionState.Connecting;
						}

						using (var disposables = new CompositeDisposable())
						{
							var streamUri = new Uri(StreamUri.Value);
							var retreiver = new IcecastSongMetadataRetreiver(this.HttpClient, streamUri, Encoding.GetEncoding("Shift_JIS"));

							Observable.FromEventPattern<ISongMetadata>(retreiver, nameof(ISongMetadataRetreiver.SongMetadataReceived))
								.Select(x => x.EventArgs)
								.ObserveOnUIDispatcher()
								.Subscribe(songMetadata =>
								{
									CurrentSongMetadata.Value = songMetadata;
									RetreiverState.Value = SongRetreiverConnectionState.Running;
								})
								.AddTo(disposables);

							var stopStatus = await retreiver.RunAsync(cts.Token);

							switch (stopStatus)
							{
								case SongListFeedStopStatus.Unknown:
									MessageService.Current.ShowMessage("タグの取得が異常終了しました。");
									RetreiverState.Value = SongRetreiverConnectionState.Stopped;
									break;

								case SongListFeedStopStatus.MetadataNotSupported:
									MessageService.Current.ShowMessage("このストリームはタグの配信に対応していません。");
									RetreiverState.Value = SongRetreiverConnectionState.Stopped;
									break;

								case SongListFeedStopStatus.StreamClosed:
									if (this.Settings.EnableMetadataStreamAutoReconnection)
									{
										if (CurrentRetryCount.Value < this.Settings.MaxMetadataStreamAutoReconnectionTrialCount)
										{
											MessageService.Current.ShowMessage($"タグ取得用ストリームが切断されました。再接続しています... ({CurrentRetryCount.Value + 1}回目)");
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
							if (CurrentRetryCount.Value < this.Settings.MaxMetadataStreamAutoReconnectionTrialCount)
							{
								MessageService.Current.ShowMessage($"タグ取得用ストリームへの接続に失敗しました。再接続しています... ({CurrentRetryCount.Value + 1}回目)");
								shouldRetry = true;
							}
							else
							{
								MessageService.Current.ShowMessage($"タグ取得用ストリームが切断されました。最大再接続回数に達しました。({this.Settings.MaxMetadataStreamAutoReconnectionTrialCount}回)");
							}
						}
						else
						{
							ShowErrorMessage("タグの取得に失敗しました。", ex);
						}
					}
					finally
					{
						if (shouldRetry)
						{
							CurrentSongMetadata.Value = null;

							try
							{
								RetreiverState.Value = SongRetreiverConnectionState.Reconnecting;
								CurrentRetryCount.Value++;
								await Task.Delay(this.Settings.MetadataStreamReconnectionInterval, cts.Token);

								RetreiverCancellationTokenSource.Value = null;
								RetreiverCancellationTokenSource.Value = new CancellationTokenSource();
							}
							catch (OperationCanceledException)
							{
								RetreiverCancellationTokenSource.Value = null;
								RetreiverState.Value = SongRetreiverConnectionState.Stopped;
								MessageService.Current.ShowMessage("タグの取得が停止されました。");
							}
							finally
							{
								cts.Dispose();
							}
						}
						else
						{
							RetreiverCancellationTokenSource.Value = null;
							cts.Dispose();
							RetreiverState.Value = SongRetreiverConnectionState.Stopped;
						}
					}

					return cts;
				})
				.Subscribe()
				.AddTo(Disposables);

			return prop;
		});

		public ReactiveProperty<ISongMetadata> CurrentSongMetadata => this.LazyReactiveProperty(() =>
		{
			var prop = new ReactiveProperty<ISongMetadata>();

			prop.Where(x => x != null)
				.Subscribe(x =>
				{
					SongMetadataHistory.Insert(0, x);
				})
				.AddTo(Disposables);

			return prop;
		});

		public ReactiveProperty<string> CurrentSongTitle => this.LazyReactiveProperty(() =>
		{
			return CurrentSongMetadata
				.Select(x => x?.StreamTitle)
				.ToReactiveProperty();
		});

		public ReactiveCollection<ISongMetadata> SongMetadataHistory { get; } = new ReactiveCollection<ISongMetadata>();

		public ReactiveProperty<SongRetreiverConnectionState> RetreiverState => this.LazyReactiveProperty(() =>
		{
			var prop = new ReactiveProperty<SongRetreiverConnectionState>();

			prop.Subscribe(state =>
				{
					switch (state)
					{
						case SongRetreiverConnectionState.Stopped:
							CurrentRetryCount.Value = 0;
							break;

						case SongRetreiverConnectionState.Running:
							MessageService.Current.ShowMessage("タグ取得用ストリームへ接続しました。");
							CurrentRetryCount.Value = 0;
							break;
					}
				})
				.AddTo(Disposables);

			return prop;
		});

		public ReactiveProperty<string> StreamUri => this.LazyReactiveProperty(() => Settings.ToReactivePropertyAsSynchronized(x => x.MetadataStreamUri));
		public ReactiveProperty<bool> EnableAutoReconnection => this.LazyReactiveProperty(() => Settings.ToReactivePropertyAsSynchronized(x => x.EnableMetadataStreamAutoReconnection));
		public ReactiveProperty<bool> ShowHistory => this.LazyReactiveProperty(() => Settings.ToReactivePropertyAsSynchronized(x => x.ShowMetadataStreamHistory));
		public ReactiveProperty<bool> ShowSongInfoExtractorControl => this.LazyReactiveProperty(() => Settings.ToReactivePropertyAsSynchronized(x => x.ShowSongInfoExtractorControl));

		public ReactiveProperty<string> RegexFormat => this.LazyReactiveProperty(() =>
		{
			var prop = Settings.ToReactivePropertyAsSynchronized(x => x.SongInfoExtractorRegexFormat);

			return prop;
		});

		public ReactiveProperty<string[]> RegexFormatPresets => this.LazyReactiveProperty(() => Settings.ToReactivePropertyAsSynchronized(x => x.SongInfoExtractorPresets));

		public ReactiveProperty<string> ExtractedSongTitle { get; } = new ReactiveProperty<string>();
		public ReactiveProperty<string> ExtractedArtist { get; } = new ReactiveProperty<string>();

		public ReactiveProperty<int> CurrentRetryCount { get; } = new ReactiveProperty<int>();

		#endregion Bindings

		#region Commands

		public ICommand StartRetreivingCommand => this.LazyReactiveCommand(
			new[] {
				RetreiverCancellationTokenSource.Select(x => x == null),
				StreamUri.Select(x => !string.IsNullOrWhiteSpace(x)),
				RetreiverState.Select(x => x == SongRetreiverConnectionState.Stopped),
			}
			.CombineLatestValuesAreAllTrue(),
			() =>
			{
				RetreiverCancellationTokenSource.Value = new CancellationTokenSource();
			});

		public ICommand StopRetreivingCommand => this.LazyReactiveCommand(
			RetreiverState.Select(x => x != SongRetreiverConnectionState.Stopped),
			() =>
			{
				RetreiverCancellationTokenSource.Value?.Cancel();
			});

		public ICommand ClearHistoryCommand => this.LazyReactiveCommand(
			SongMetadataHistory
				.CollectionChangedAsObservable()
				.Select(_ => SongMetadataHistory.Count != 0),
			() =>
			{
				SongMetadataHistory.Clear();
			});

		public ICommand SearchCommand => this.LazyReactiveCommand(
			this.ExtractedSongTitle.Select(x => !string.IsNullOrWhiteSpace(x)),
			() =>
			{
				var searchTerm = this.ExtractedSongTitle.Value;
				if (!string.IsNullOrWhiteSpace(searchTerm))
				{
					this.SearchManager.TriggerSearch(searchTerm);
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

		#endregion Commands
	}
}
