using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using AnizanHelper.Models;
using AnizanHelper.Models.Parsers;
using AnizanHelper.Models.SettingComponents;

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
			InputText = "";
		}

		#region Bindings
		#region InputText
		string inputText_ = "";
		public string InputText
		{
			get
			{
				return inputText_;
			}
			set
			{
				if (SetValue(ref inputText_, value, GetMemberName(() => InputText))) {
					ParseCommand.RaiseCanExecuteChanged();
				}
			}
		}
		#endregion
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
