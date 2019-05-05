using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using AnizanHelper.Models;
using AnizanHelper.Models.Parsers;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace AnizanHelper.ViewModels
{
	class SongListWindowViewModel : ReactiveViewModelBase
	{
		ISongInfoSerializer SongInfoSerializer { get; }
		AnizanFormatParser AnizanFormatParser { get; } = new AnizanFormatParser();

		public SongListWindowViewModel(
			Settings settings,
			ISongInfoSerializer songInfoSerializer,
			IObservable<AnizanSongInfo> songInfoObservable)
		{
			if (settings == null) { throw new ArgumentNullException(nameof(settings)); }
			if (songInfoObservable == null) { throw new ArgumentNullException(nameof(songInfoObservable)); }

			SongInfoSerializer = songInfoSerializer ?? throw new ArgumentNullException(nameof(songInfoSerializer));

			SongList = songInfoObservable
				.ToReactiveCollection()
				.AddTo(this.Disposables);

			foreach (var item in settings.SongList) {
				SongList.Add(item);
			}

			SongList.CollectionChangedAsObservable()
				.Throttle(TimeSpan.FromMilliseconds(250))
				.Subscribe(_ => {
					settings.SongList = SongList.ToArray();
				})
				.AddTo(Disposables);

			CopyAllCommand = SongList
				.CollectionChangedAsObservable()
				.Select(_ => SongList.Count > 0)
				.ToReactiveCommand(SongList.Count > 0)
				.AddTo(Disposables)
				.WithSubscribe(() => {
					CopyToClipboard(SongList);
				});

			CopySelectedCommand = SelectedItems
				.CollectionChangedAsObservable()
				.Select(_ => SelectedItems.Count > 0)
				.ToReactiveCommand(false)
				.AddTo(Disposables)
				.WithSubscribe(() => {
					CopyToClipboard(SelectedItems);
				});

			DeleteAllCommand = SongList
				.CollectionChangedAsObservable()
				.Select(_ => SongList.Count > 0)
				.ToReactiveCommand(SongList.Count > 0)
				.AddTo(Disposables)
				.WithSubscribe(() => {
					var ret = MessageBox.Show("全てのアイテムを削除しますか？", "確認", MessageBoxButton.YesNo, MessageBoxImage.Question);
					if (ret == MessageBoxResult.Yes) {
						SongList.Clear();
					}
				});

			DeleteSelectedCommand = SelectedItems
				.CollectionChangedAsObservable()
				.Select(_ => SelectedItems.Count > 0)
				.ToReactiveCommand(false)
				.AddTo(Disposables)
				.WithSubscribe(() => {
					var ret = MessageBox.Show("選択したアイテムを削除しますか？", "確認", MessageBoxButton.YesNo, MessageBoxImage.Question);
					if (ret == MessageBoxResult.Yes) {
						foreach (var item in SelectedItems.ToArray()) {
							SongList.Remove(item);
						}
					}
				});

			NumberSelectedCommand = SelectedItems
				.CollectionChangedAsObservable()
				.Select(_ => SelectedItems.Count > 1)
				.ToReactiveCommand(false)
				.AddTo(Disposables)
				.WithSubscribe(() => {
					var ret = MessageBox.Show("選択されたアイテムの番号を自動更新しますか？\n (最初のアイテムを基準に採番します。)", "確認", MessageBoxButton.YesNo, MessageBoxImage.Question);
					if (ret == MessageBoxResult.Yes) {
						var targetSelectedItems = SelectedItems
							.Where(x => string.IsNullOrWhiteSpace(x.SpecialHeader))
							.ToArray();

						var startNumber = targetSelectedItems.First().Number + 1;
						var selectedItemsToUpdate = new HashSet<AnizanSongInfo>(targetSelectedItems.Skip(1));

						var itemsToUpdate = this.SongList
							.Where(x => selectedItemsToUpdate.Contains(x))
							.ToArray();

						foreach (var item in itemsToUpdate) {
							item.Number = startNumber;
							startNumber++;
						}
					}
				});

			this.PasteFromClipboardCommand = new ReactiveCommand()
				.WithSubscribe(() => {
					try {
						var text = Clipboard.GetText();
						if (string.IsNullOrWhiteSpace(text)) {
							return;
						}

						var items = text.Trim().Replace("\r", "").Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries)
							.Select(line => new {
								Line = line,
								Info = AnizanFormatParser.ParseAsAnizanInfo(line),
							})
							.ToArray();

						var registeredItems = items
							.Where(x => x.Info != null)
							.ToArray();

						var nonRegisteredItems = items
							.Where(x => x.Info == null)
							.ToArray();


						if (registeredItems.Length == 0) {
							return;
						}

						var sb = new StringBuilder();
						sb.AppendLine("以下のアイテムを登録します。");
						if (registeredItems.Length > 0) {
							foreach (var item in registeredItems) {
								sb.AppendLine(SongInfoSerializer.SerializeFull(item.Info));
							}
						}

						if (nonRegisteredItems.Length > 0) {
							sb.AppendLine();
							sb.AppendLine("※以下のアイテムは登録されません。");
							foreach (var item in nonRegisteredItems) {
								sb.AppendLine(item.Line);
							}
						}

						var ret = MessageBox.Show(sb.ToString(), "クリップボードから登録", MessageBoxButton.OKCancel, MessageBoxImage.Question);
						if (ret == MessageBoxResult.OK) {
							SongList.AddRangeOnScheduler(registeredItems.Select(x => x.Info).ToArray());
						}
					}
					catch (Exception ex) {
						var sb = new StringBuilder();
						sb.AppendLine("クリップボードからのアイテム登録に失敗しました。");
						sb.AppendLine(ex.Message);

						MessageBox.Show(sb.ToString(), "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
					}
				});

			this.MoveDownCommand = SelectedItems
				.CollectionChangedAsObservable()
				.Select(_ => SelectedItems.Count > 0)
				.ToReactiveCommand(false)
				.AddTo(Disposables)
				.WithSubscribe(() => {
					var pairs = SelectedItems
						.Select(x => new {
							Item = x,
							Index = SongList.IndexOf(x),
						})
						.OrderByDescending(x => x.Index)
						.ToArray();

					var maxIndex = SongList.Count;
					foreach (var pair in pairs) {
						if (pair.Index < maxIndex - 1) {
							SongList.Remove(pair.Item);

							var newIndex = pair.Index + 1;
							if (newIndex > SongList.Count - 1) {
								SongList.Add(pair.Item);
							}
							else {
								SongList.Insert(newIndex, pair.Item);
							}

							SelectedItems.Add(pair.Item);
							maxIndex = pair.Index + 1;
						}
						else {
							maxIndex = pair.Index;
						}
					}
				});

			this.MoveUpCommand = SelectedItems
				.CollectionChangedAsObservable()
				.Select(_ => SelectedItems.Count > 0)
				.ToReactiveCommand(false)
				.AddTo(Disposables)
				.WithSubscribe(() => {
					var pairs = SelectedItems
						.Select(x => new {
							Item = x,
							Index = SongList.IndexOf(x),
						})
						.OrderBy(x => x.Index)
						.ToArray();

					var minIndex = -1;
					foreach (var pair in pairs) {
						if (pair.Index > minIndex + 1) {
							SongList.Remove(pair.Item);
							SongList.Insert(pair.Index - 1, pair.Item);
							SelectedItems.Add(pair.Item);
							minIndex = pair.Index - 1;
						}
						else {
							minIndex = pair.Index;
						}
					}
				});

			SelectOrDeselectAllCommand = new ReactiveCommand()
				.WithSubscribe(() => {
					if (SelectedItems.Count > 0) {
						SelectedItems.Clear();
					}
					else {
						foreach (var item in SongList.ToArray()) {
							SelectedItems.Add(item);
						}
					}
				});

			this.SortSelectedCommand = SelectedItems
				.CollectionChangedAsObservable()
				.Select(_ => SelectedItems.Count > 0)
				.ToReactiveCommand(false)
				.AddTo(Disposables)
				.WithSubscribe(() => {
					var pairs = SelectedItems
						.Where(x => string.IsNullOrWhiteSpace(x.SpecialHeader))
						.Select(x => new {
							Item = x,
							Index = SongList.IndexOf(x),
						})
						.ToArray();

					if (pairs.Length == 0) {
						return;
					}

					var ret = MessageBox.Show("選択されたアイテムをソートしますか？", "確認", MessageBoxButton.YesNo, MessageBoxImage.Question);
					if (ret != MessageBoxResult.OK) {
						return;
					}

					foreach (var pair in pairs) {
						SongList.Remove(pair.Item);
					}

					var minIndex = pairs
						.Select(x => x.Index)
						.Min();

					var indices = Enumerable.Range(minIndex, pairs.Length);

					var itemsToAdd = pairs
						.OrderBy(x => x.Item.Number)
						.Zip(indices, (x, i) => new {
							Item = x.Item,
							Index = i,
						});
					foreach (var pair in itemsToAdd) {
						SongList.Insert(pair.Index, pair.Item);
					}
				});

			this.AddSpecialItemCommand = new ReactiveCommand()
				.WithSubscribe(() => {
					this.SongList.Add(new AnizanSongInfo {
						SpecialHeader = "★",
					});
				});
		}


		

		private void CopyToClipboard(IEnumerable<AnizanSongInfo> items)
		{
			var lines = items
				.Select(item => SongInfoSerializer.SerializeFull(item));

			var text = string.Join(
				Environment.NewLine,
				lines);

			App.Current.Dispatcher.BeginInvoke((Action)(() =>
			{
				try
				{
					System.Windows.Forms.Clipboard.SetText(text);
				}
				catch(Exception ex)
				{
					this.ShowErrorMessage("コピーに失敗しました。", ex);
				}
			}));
		}

		public ReactiveCollection<AnizanSongInfo> SongList { get; }
		public ReactiveCollection<AnizanSongInfo> SelectedItems { get; } = new ReactiveCollection<AnizanSongInfo>();

		
		public ReactiveCommand CopyAllCommand { get; }
		public ReactiveCommand CopySelectedCommand { get; }
		public ReactiveCommand DeleteAllCommand { get; }
		public ReactiveCommand DeleteSelectedCommand { get; }
		public ReactiveCommand NumberSelectedCommand { get; }
		public ReactiveCommand PasteFromClipboardCommand { get; }

		public ReactiveCommand MoveUpCommand { get; }
		public ReactiveCommand MoveDownCommand { get; }
		public ReactiveCommand SelectOrDeselectAllCommand { get; }
		
		public ReactiveCommand AddSpecialItemCommand { get; }
		public ReactiveCommand SortSelectedCommand { get; }

	}
}
