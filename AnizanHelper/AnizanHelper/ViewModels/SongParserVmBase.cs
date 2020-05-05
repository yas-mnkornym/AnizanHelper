using System;
using AnizanHelper.Models;

namespace AnizanHelper.ViewModels
{
	public abstract class SongParserVmBase : ReactiveViewModelBase
	{
		public SongParserVmBase() { }

		protected void OnSongParsed(SongParsedEventArgs args)
		{
			if (SongParsed != null)
			{
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
			this.SongInfo = songInfo;
		}

		public GeneralSongInfo SongInfo { get; private set; }
	}
}
