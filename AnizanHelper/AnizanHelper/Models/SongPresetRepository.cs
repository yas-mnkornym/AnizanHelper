using System.ComponentModel;

namespace AnizanHelper.Models
{
	public class SongPresetRepository : INotifyPropertyChanged
	{
		private ZanmaiSongInfo[] presets_;

		public ZanmaiSongInfo[] Presets
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
