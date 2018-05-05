using System;
using System.Windows;
using AnizanHelper.Models;
using AnizanHelper.Models.Parsers;
using Studiotaiha.Toolkit;

namespace AnizanHelper.ViewModels
{
	public class SongParserVm : SongParserVmBase
	{
		ISongInfoParser parser = new AnisonDBParser();

		public SongParserVm(IDispatcher dispatcher)
			: base(dispatcher)
		{ }

		public override void ClearInput()
		{
			InputText = string.Empty;
		}

		#region Bindings

		public string InputText
		{
			get
			{
				return GetValue<string>(string.Empty);
			}
			set
			{
				if (SetValue(value)) {
					ParseCommand.RaiseCanExecuteChanged();
				}
			}
		}

		#endregion // Bindings

		#region Commands
		#region ParseCommand
		DelegateCommand parseCommand_ = null;
		public DelegateCommand ParseCommand
		{
			get
			{
				return parseCommand_ ?? (parseCommand_ = new DelegateCommand {
					ExecuteHandler = param => {
						try {
							var info = parser.Parse(InputText);
							OnSongParsed(new SongParsedEventArgs(info));
						}
						catch (Exception ex) {
							MessageBox.Show(ex.ToString(), "解析失敗", MessageBoxButton.OK, MessageBoxImage.Stop);
						}

					},
					CanExecuteHandler = param => {
						return true;
					}
				});
			}
		}
		#endregion 
		#endregion // Commands
	}
}
