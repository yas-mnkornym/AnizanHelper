using System;
using System.Windows;
using AnizanHelper.Models;
using AnizanHelper.Models.Parsers;
using AnizanHelper.ViewModels.Events;
using Prism.Events;
using Reactive.Bindings.Extensions;

namespace AnizanHelper.ViewModels.Pages
{
	public class SongParserPageViewModel : ReactiveViewModelBase
	{
		private ISongInfoParser Parser { get; } = new AnisonDBParser();
		private IEventAggregator EventAggregator { get; }
		private AnizanSongInfoProcessor AnizanSongInfoProcessor { get; }

		public SongParserPageViewModel(
			IEventAggregator eventAggregator,
			AnizanSongInfoProcessor anizanSongInfoProcessor)
		{
			this.EventAggregator = eventAggregator ?? throw new ArgumentNullException(nameof(eventAggregator));
			this.AnizanSongInfoProcessor = anizanSongInfoProcessor ?? throw new ArgumentNullException(nameof(anizanSongInfoProcessor));

			this.EventAggregator
				.GetEvent<ClearSearchInputEvent>()
				.Subscribe(() =>
				{
					this.ClearInput();
				})
				.AddTo(this.Disposables);
		}

		public void ClearInput()
		{
			this.InputText = string.Empty;
		}

		#region Bindings

		public string InputText
		{
			get
			{
				return this.GetValue(string.Empty);
			}
			set
			{
				if (this.SetValue(value))
				{
					this.ParseCommand.RaiseCanExecuteChanged();
				}
			}
		}

		#endregion Bindings

		#region Commands

		#region ParseCommand

		private DelegateCommand parseCommand_ = null;

		public DelegateCommand ParseCommand
		{
			get
			{
				return this.parseCommand_ ?? (this.parseCommand_ = new DelegateCommand
				{
					ExecuteHandler = param =>
					{
						try
						{
							var info = this.Parser.Parse(this.InputText);

							info = this.AnizanSongInfoProcessor.Convert(info);

							this.EventAggregator
								.GetEvent<SongParsedEvent>()
								.Publish(info);
						}
						catch (Exception ex)
						{
							MessageBox.Show(ex.ToString(), "解析失敗", MessageBoxButton.OK, MessageBoxImage.Stop);
						}
					},
					CanExecuteHandler = param =>
					{
						return true;
					}
				});
			}
		}

		#endregion ParseCommand

		#endregion Commands
	}
}
