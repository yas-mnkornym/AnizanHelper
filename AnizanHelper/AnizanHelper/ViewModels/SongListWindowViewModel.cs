using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Windows;
using AnizanHelper.Models;
using AnizanHelper.Models.Parsers;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace AnizanHelper.ViewModels
{
	internal class SongListWindowViewModel : ReactiveViewModelBase
	{
		private ISongInfoSerializer SongInfoSerializer { get; }
		private AnizanFormatParser AnizanFormatParser { get; } = new AnizanFormatParser();

		public SongListWindowViewModel(
			Settings settings,
			ISongInfoSerializer songInfoSerializer,
			IObservable<ZanmaiSongInfo> songInfoObservable)
		{
			if (settings == null) { throw new ArgumentNullException(nameof(settings)); }
			if (songInfoObservable == null) { throw new ArgumentNullException(nameof(songInfoObservable)); }

			this.SongInfoSerializer = songInfoSerializer ?? throw new ArgumentNullException(nameof(songInfoSerializer));

			this.SongList = songInfoObservable
				.Select(x => new ZanmaiSongInfoViewModel(x))
				.ToReactiveCollection()
				.AddTo(this.Disposables);

			foreach (var item in settings.SongList)
			{
				this.SongList.Add(new ZanmaiSongInfoViewModel(item));
			}

			this.SongList.CollectionChangedAsObservable()
				.Throttle(TimeSpan.FromMilliseconds(250))
				.Subscribe(_ =>
				{
					settings.SongList = this.SongList.Select(x => x.ToZanmaiSongInfo()).ToArray();
				})
				.AddTo(this.Disposables);

			this.CopyAllCommand = this.SongList
				.CollectionChangedAsObservable()
				.Select(_ => this.SongList.Count > 0)
				.ToReactiveCommand(this.SongList.Count > 0)
				.AddTo(this.Disposables)
				.WithSubscribe(() =>
				{
					this.CopyToClipboard(this.SongList.Select(x => x.ToZanmaiSongInfo()));
				});

			this.CopySelectedCommand = this.SelectedItems
				.CollectionChangedAsObservable()
				.Select(_ => this.SelectedItems.Count > 0)
				.ToReactiveCommand(false)
				.AddTo(this.Disposables)
				.WithSubscribe(() =>
				{
					this.CopyToClipboard(this.SelectedItems.Select(x => x.ToZanmaiSongInfo()));
				});

			this.DeleteAllCommand = this.SongList
				.CollectionChangedAsObservable()
				.Select(_ => this.SongList.Count > 0)
				.ToReactiveCommand(this.SongList.Count > 0)
				.AddTo(this.Disposables)
				.WithSubscribe(() =>
				{
					var ret = MessageBox.Show("全てのアイテムを削除しますか？", "確認", MessageBoxButton.YesNo, MessageBoxImage.Question);
					if (ret == MessageBoxResult.Yes)
					{
						this.SongList.Clear();
					}
				});

			this.DeleteSelectedCommand = this.SelectedItems
				.CollectionChangedAsObservable()
				.Select(_ => this.SelectedItems.Count > 0)
				.ToReactiveCommand(false)
				.AddTo(this.Disposables)
				.WithSubscribe(() =>
				{
					var ret = MessageBox.Show("選択したアイテムを削除しますか？", "確認", MessageBoxButton.YesNo, MessageBoxImage.Question);
					if (ret == MessageBoxResult.Yes)
					{
						foreach (var item in this.SelectedItems.ToArray())
						{
							this.SongList.Remove(item);
						}
					}
				});

			this.NumberSelectedCommand = this.SelectedItems
				.CollectionChangedAsObservable()
				.Select(_ => this.SelectedItems.Count > 1)
				.ToReactiveCommand(false)
				.AddTo(this.Disposables)
				.WithSubscribe(() =>
				{
					var ret = MessageBox.Show("選択されたアイテムの番号を自動更新しますか？\n (最初のアイテムを基準に採番します。)", "確認", MessageBoxButton.YesNo, MessageBoxImage.Question);
					if (ret == MessageBoxResult.Yes)
					{
						var targetSelectedItems = this.SelectedItems
							.Where(x => string.IsNullOrWhiteSpace(x.SpecialHeader))
							.ToArray();

						var startNumber = targetSelectedItems.First().Number + 1;
						var selectedItemsToUpdate = new HashSet<ZanmaiSongInfoViewModel>(targetSelectedItems.Skip(1).Select(x => new ZanmaiSongInfoViewModel(x)));

						var itemsToUpdate = this.SongList
							.Where(x => selectedItemsToUpdate.Contains(x))
							.ToArray();

						foreach (var item in itemsToUpdate)
						{
							item.Number = startNumber;
							startNumber++;
						}
					}
				});

			this.PasteFromClipboardCommand = new ReactiveCommand()
				.WithSubscribe(() =>
				{
					try
					{
						var text = Clipboard.GetText();
						if (string.IsNullOrWhiteSpace(text))
						{
							return;
						}

						var items = text.Trim().Replace("\r", "").Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries)
							.Select(line => new
							{
								Line = line,
								Info = this.AnizanFormatParser.Parse(line),
							})
							.ToArray();

						var registeredItems = items
							.Where(x => x.Info != null)
							.ToArray();

						var nonRegisteredItems = items
							.Where(x => x.Info == null)
							.ToArray();


						if (registeredItems.Length == 0)
						{
							return;
						}

						var sb = new StringBuilder();
						sb.AppendLine("以下のアイテムを登録します。");
						if (registeredItems.Length > 0)
						{
							foreach (var item in registeredItems)
							{
								sb.AppendLine(this.SongInfoSerializer.SerializeFull(item.Info));
							}
						}

						if (nonRegisteredItems.Length > 0)
						{
							sb.AppendLine();
							sb.AppendLine("※以下のアイテムは登録されません。");
							foreach (var item in nonRegisteredItems)
							{
								sb.AppendLine(item.Line);
							}
						}

						var ret = MessageBox.Show(sb.ToString(), "クリップボードから登録", MessageBoxButton.OKCancel, MessageBoxImage.Question);
						if (ret == MessageBoxResult.OK)
						{
							this.SongList.AddRangeOnScheduler(registeredItems.Select(x => new ZanmaiSongInfoViewModel(x.Info)).ToArray());
						}
					}
					catch (Exception ex)
					{
						var sb = new StringBuilder();
						sb.AppendLine("クリップボードからのアイテム登録に失敗しました。");
						sb.AppendLine(ex.Message);

						MessageBox.Show(sb.ToString(), "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
					}
				});

			this.MoveDownCommand = this.SelectedItems
				.CollectionChangedAsObservable()
				.Select(_ => this.SelectedItems.Count > 0)
				.ToReactiveCommand(false)
				.AddTo(this.Disposables)
				.WithSubscribe(() =>
				{
					var pairs = this.SelectedItems
						.Select(x => new
						{
							Item = x,
							Index = this.SongList.IndexOf(x),
						})
						.OrderByDescending(x => x.Index)
						.ToArray();

					var maxIndex = this.SongList.Count;
					foreach (var pair in pairs)
					{
						if (pair.Index < maxIndex - 1)
						{
							this.SongList.Remove(pair.Item);

							var newIndex = pair.Index + 1;
							if (newIndex > this.SongList.Count - 1)
							{
								this.SongList.Add(pair.Item);
							}
							else
							{
								this.SongList.Insert(newIndex, pair.Item);
							}

							this.SelectedItems.Add(pair.Item);
							maxIndex = pair.Index + 1;
						}
						else
						{
							maxIndex = pair.Index;
						}
					}
				});

			this.MoveUpCommand = this.SelectedItems
				.CollectionChangedAsObservable()
				.Select(_ => this.SelectedItems.Count > 0)
				.ToReactiveCommand(false)
				.AddTo(this.Disposables)
				.WithSubscribe(() =>
				{
					var pairs = this.SelectedItems
						.Select(x => new
						{
							Item = x,
							Index = this.SongList.IndexOf(x),
						})
						.OrderBy(x => x.Index)
						.ToArray();

					var minIndex = -1;
					foreach (var pair in pairs)
					{
						if (pair.Index > minIndex + 1)
						{
							this.SongList.Remove(pair.Item);
							this.SongList.Insert(pair.Index - 1, pair.Item);
							this.SelectedItems.Add(pair.Item);
							minIndex = pair.Index - 1;
						}
						else
						{
							minIndex = pair.Index;
						}
					}
				});

			this.SelectOrDeselectAllCommand = new ReactiveCommand()
				.WithSubscribe(() =>
				{
					if (this.SelectedItems.Count > 0)
					{
						this.SelectedItems.Clear();
					}
					else
					{
						foreach (var item in this.SongList.ToArray())
						{
							this.SelectedItems.Add(item);
						}
					}
				});

			this.SortSelectedCommand = this.SelectedItems
				.CollectionChangedAsObservable()
				.Select(_ => this.SelectedItems.Count > 0)
				.ToReactiveCommand(false)
				.AddTo(this.Disposables)
				.WithSubscribe(() =>
				{
					var pairs = this.SelectedItems
						.Where(x => string.IsNullOrWhiteSpace(x.SpecialHeader))
						.Select(x => new
						{
							Item = x,
							Index = this.SongList.IndexOf(x),
						})
						.ToArray();

					if (pairs.Length == 0)
					{
						return;
					}

					var ret = MessageBox.Show("選択されたアイテムをソートしますか？", "確認", MessageBoxButton.YesNo, MessageBoxImage.Question);
					if (ret != MessageBoxResult.OK)
					{
						return;
					}

					foreach (var pair in pairs)
					{
						this.SongList.Remove(pair.Item);
					}

					var minIndex = pairs
						.Select(x => x.Index)
						.Min();

					var indices = Enumerable.Range(minIndex, pairs.Length);

					var itemsToAdd = pairs
						.OrderBy(x => x.Item.Number)
						.Zip(indices, (x, i) => new
						{
							Item = x.Item,
							Index = i,
						});
					foreach (var pair in itemsToAdd)
					{
						this.SongList.Insert(pair.Index, pair.Item);
					}
				});

			this.AddSpecialItemCommand = new ReactiveCommand()
				.WithSubscribe(() =>
				{
					this.SongList.Add(new ZanmaiSongInfoViewModel
					{
						SpecialHeader = "★",
					});
				});
		}




		private void CopyToClipboard(IEnumerable<ZanmaiSongInfo> items)
		{
			var lines = items
				.Select(item => this.SongInfoSerializer.SerializeFull(item));

			var text = string.Join(
				Environment.NewLine,
				lines);

			App.Current.Dispatcher.BeginInvoke((Action)(() =>
			{
				try
				{
					System.Windows.Forms.Clipboard.SetText(text);
				}
				catch (Exception ex)
				{
					this.ShowErrorMessage("コピーに失敗しました。", ex);
				}
			}));
		}

		public ReactiveCollection<ZanmaiSongInfoViewModel> SongList { get; }
		public ReactiveCollection<ZanmaiSongInfoViewModel> SelectedItems { get; } = new ReactiveCollection<ZanmaiSongInfoViewModel>();


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
