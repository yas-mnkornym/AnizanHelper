using AnizanHelper.Models;
using AnizanHelper.Models.SongList;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Linq;
using System.Net.Http;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

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
		public SongMetadataViewerControlViewModel(
			Settings settings,
			HttpClient httpClient)
		{
			if (settings == null) { throw new ArgumentNullException(nameof(settings)); }
			if (httpClient == null) { throw new ArgumentNullException(nameof(httpClient)); }

			this.EnableAutoReconnection = settings
				.ToReactivePropertyAsSynchronized(x => x.EnableMetadataStreamAutoReconnection)
				.AddTo(this.Disposables);

			this.StreamUri = settings
				.ToReactivePropertyAsSynchronized(x => x.MetadataStreamUri)
				.AddTo(this.Disposables);

			this.ShowHistory = settings
				.ToReactivePropertyAsSynchronized(x => x.ShowMetadataStreamHistory)
				.AddTo(this.Disposables);

			this.CurrentSongTitle = this.CurrentSongMetadata
				.Select(x => x?.StreamTitle)
				.ToReactiveProperty()
				.AddTo(this.Disposables);

			this.CurrentSongMetadata
				.Where(x => x != null)
				.Subscribe(x =>
				{
					this.SongMetadataHistory.Insert(0, x);
				})
				.AddTo(this.Disposables);

			this.RetreiverState
				.Subscribe(state =>
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

			this.RetreiverCancellationTokenSource
				.Where(x => x != null)
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
							var retreiver = new IcecastSongMetadataRetreiver(httpClient, streamUri, Encoding.GetEncoding("Shift_JIS"));

							Observable.FromEventPattern<ISongMetadata>(retreiver, nameof(ISongMetadataRetreiver.SongMetadataReceived))
								.Select(x => x.EventArgs)
								.ObserveOnUIDispatcher()
								.Subscribe(songMetadata =>
								{
									this.CurrentSongMetadata.Value = songMetadata;
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
									this.ShowErrorMessage("このストリームはタグの配信に対応していません。");
									this.RetreiverState.Value = SongRetreiverConnectionState.Stopped;
									break;

								case SongListFeedStopStatus.StreamClosed:
									if (settings.EnableMetadataStreamAutoReconnection)
									{
										if (this.CurrentRetryCount.Value < settings.MaxMetadataStreamAutoReconnectionTrialCount)
										{
											MessageService.Current.ShowMessage($"タグ取得用ストリームが切断されました。再接続しています... ({this.CurrentRetryCount.Value + 1}回目)");
											shouldRetry = true;
										}
										else
										{
											MessageService.Current.ShowMessage($"タグ取得用ストリームが切断されました。最大再接続回数に達しました。({settings.MaxMetadataStreamAutoReconnectionTrialCount}回)");
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
						if (settings.EnableMetadataStreamAutoReconnection)
						{
							if (this.CurrentRetryCount.Value < settings.MaxMetadataStreamAutoReconnectionTrialCount)
							{
								MessageService.Current.ShowMessage($"タグ取得用ストリームへの接続に失敗しました。再接続しています... ({this.CurrentRetryCount.Value + 1}回目)");
								shouldRetry = true;
							}
							else
							{
								MessageService.Current.ShowMessage($"タグ取得用ストリームが切断されました。最大再接続回数に達しました。({settings.MaxMetadataStreamAutoReconnectionTrialCount}回)");
							}
						}
						else
						{
							this.ShowErrorMessage("タグの取得に失敗しました。", ex);
						}
					}
					finally
					{
						if (shouldRetry)
						{
							this.CurrentSongMetadata.Value = null;

							try
							{
								this.RetreiverState.Value = SongRetreiverConnectionState.Reconnecting;
								this.CurrentRetryCount.Value++;
								await Task.Delay(settings.MetadataStreamReconnectionInterval, cts.Token);

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

			this.StartRetreivingCommand =
				new[] {
					this.RetreiverCancellationTokenSource.Select(x => x == null),
					this.StreamUri.Select(x => !string.IsNullOrWhiteSpace(x)),
					this.RetreiverState.Select(x => x == SongRetreiverConnectionState.Stopped),
				}
				.CombineLatestValuesAreAllTrue()
				.ToReactiveCommand(true)
				.AddTo(this.Disposables)
				.WithSubscribe(
					() =>
					{
						this.RetreiverCancellationTokenSource.Value = new CancellationTokenSource();
					},
					x => x.AddTo(this.Disposables));

			this.StopRetreivingCommand = this.RetreiverState.Select(x => x != SongRetreiverConnectionState.Stopped)
				.ToReactiveCommand(false)
				.AddTo(this.Disposables)
				.WithSubscribe(() =>
				{
					this.RetreiverCancellationTokenSource.Value?.Cancel();
				},
				x => x.AddTo(this.Disposables));

			this.ClearHistoryCommand = this.SongMetadataHistory
				.CollectionChangedAsObservable()
				.Select(_ => this.SongMetadataHistory.Count > 0)
				.ToReactiveCommand()
				.AddTo(this.Disposables)
				.WithSubscribe(() =>
				{
					this.SongMetadataHistory.Clear();
				},
				x => x.AddTo(this.Disposables));
		}

		#region Bindings

		public ReactiveProperty<CancellationTokenSource> RetreiverCancellationTokenSource { get; } = new ReactiveProperty<CancellationTokenSource>();
		public ReactiveProperty<ISongMetadata> CurrentSongMetadata { get; } = new ReactiveProperty<ISongMetadata>();

		public ReactiveProperty<string> CurrentSongTitle { get; }
		public ReactiveCollection<ISongMetadata> SongMetadataHistory { get; } = new ReactiveCollection<ISongMetadata>();
		public ReactiveProperty<SongRetreiverConnectionState> RetreiverState { get; } = new ReactiveProperty<SongRetreiverConnectionState>(SongRetreiverConnectionState.Stopped);

		public ReactiveProperty<string> StreamUri { get; } = new ReactiveProperty<string>();
		public ReactiveProperty<bool> EnableAutoReconnection { get; }
		public ReactiveProperty<bool> ShowHistory { get; }

		public ReactiveProperty<int> CurrentRetryCount { get; } = new ReactiveProperty<int>();

		#endregion Bindings

		#region Commands

		public ReactiveCommand StartRetreivingCommand { get; }
		public ReactiveCommand StopRetreivingCommand { get; }
		public ReactiveCommand ClearHistoryCommand { get; }

		#endregion Commands
	}
}
