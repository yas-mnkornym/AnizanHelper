using System;
using System.Windows;
using AnizanHelper.Models;
using AnizanHelper.Models.Parsers;

namespace AnizanHelper.ViewModels.Pages
{
	public class SongParserPageViewModel : SongParserVmBase
	{
		private ISongInfoParser parser = new AnisonDBParser();

		public override void ClearInput()
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

		#endregion // Bindings

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
							var info = this.parser.Parse(this.InputText);
							this.OnSongParsed(new SongParsedEventArgs(info));
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
		#endregion
		#endregion // Commands
	}
}
