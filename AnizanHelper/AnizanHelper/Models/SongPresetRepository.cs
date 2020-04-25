using System.ComponentModel;

namespace AnizanHelper.Models
{
	internal class SongPresetRepository : INotifyPropertyChanged
	{
		AnizanSongInfo[] presets_;

		public AnizanSongInfo[] Presets
		{
			get => presets_;
			set
			{
				if (presets_ != value)
				{
					presets_ = value;
					PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Presets)));
				}
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;
	}
}
