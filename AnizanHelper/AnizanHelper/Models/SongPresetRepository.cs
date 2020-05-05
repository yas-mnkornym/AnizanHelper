using System.ComponentModel;

namespace AnizanHelper.Models
{
	internal class SongPresetRepository : INotifyPropertyChanged
	{
		private AnizanSongInfo[] presets_;

		public AnizanSongInfo[] Presets
		{
			get => this.presets_;
			set
			{
				if (this.presets_ != value)
				{
					this.presets_ = value;
					PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.Presets)));
				}
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;
	}
}
