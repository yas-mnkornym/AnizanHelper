using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AnizanHelper.Models;
using AnizanHelper.Models.SettingComponents;

namespace AnizanHelper.ViewModels
{
	public abstract class SongParserVmBase : BindableBase
	{
		public SongParserVmBase() { }

		public SongParserVmBase(IDispatcher dispatcher)
			: base(dispatcher)
		{ }


		protected void OnSongParsed(SongParsedEventArgs args)
		{
			if (SongParsed != null) {
				SongParsed(this, args);
			}
		}

		public event EventHandler<SongParsedEventArgs> SongParsed;

		public abstract void ClearInput();
	}

	public sealed class SongParsedEventArgs : EventArgs
	{
		public SongParsedEventArgs(GeneralSongInfo songInfo)
		{
			SongInfo = songInfo;
		}

		public GeneralSongInfo SongInfo { get; private set; }
	}
}
