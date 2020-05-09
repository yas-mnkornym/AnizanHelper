using System;
using AnizanHelper.Models;

namespace AnizanHelper.ViewModels
{
	public sealed class ZanmaiSongInfoViewModel : ViewModelBase
	{
		public ZanmaiSongInfoViewModel()
		{
		}

		public ZanmaiSongInfoViewModel(ZanmaiSongInfo zanmaiSongInfo)
		{
			if (zanmaiSongInfo == null) { throw new ArgumentNullException(nameof(zanmaiSongInfo)); }

			this.Number = zanmaiSongInfo.Number;
			this.Title = zanmaiSongInfo.Title;
			this.Artists = string.Join(",", zanmaiSongInfo.Artists);
			this.Genre = zanmaiSongInfo.Genre;
			this.Series = zanmaiSongInfo.Series;
			this.SongType = zanmaiSongInfo.SongType;
			this.IsSpecialItem = zanmaiSongInfo.IsSpecialItem;
			this.SpecialHeader = zanmaiSongInfo.SpecialHeader;
			this.SpecialItemName = zanmaiSongInfo.SpecialItemName;
			this.Additional = zanmaiSongInfo.Additional;
			this.ShortDescription = zanmaiSongInfo.ShortDescription;
		}

		public int Number
		{
			get => this.GetValue<int>();
			set => this.SetValue(value);
		}

		public string Title
		{
			get => this.GetValue(string.Empty);
			set => this.SetValue(value);
		}

		public string Artists
		{
			get => this.GetValue(string.Empty);
			set => this.SetValue(value);
		}

		public string Genre
		{
			get => this.GetValue(string.Empty);
			set => this.SetValue(value);
		}

		public string Series
		{
			get => this.GetValue(string.Empty);
			set => this.SetValue(value);
		}

		public string SongType
		{
			get => this.GetValue(string.Empty);
			set => this.SetValue(value);
		}

		public bool IsSpecialItem
		{
			get => this.GetValue(false);
			set => this.SetValue(value);
		}

		public string SpecialItemName
		{
			get => this.GetValue(string.Empty);
			set => this.SetValue(value);
		}

		public string SpecialHeader
		{
			get => this.GetValue(string.Empty);
			set => this.SetValue(value);
		}

		public string Additional
		{
			get => this.GetValue(string.Empty);
			set => this.SetValue(value);
		}

		public string ShortDescription
		{
			get => this.GetValue(string.Empty);
			set => this.SetValue(value);
		}

		public ZanmaiSongInfo ToZanmaiSongInfo()
		{
			return new ZanmaiSongInfo
			{
				Number = this.Number,
				Title = this.Title,
				Artists = this.Artists.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries),
				Genre = this.Genre,
				Series = this.Series,
				SongType = this.SongType,

				IsSpecialItem = this.IsSpecialItem,
				SpecialItemName = this.SpecialItemName,
				SpecialHeader = this.SpecialHeader,
				Additional = this.Additional,
				ShortDescription = this.ShortDescription,
			};
		}

		public static implicit operator ZanmaiSongInfo(ZanmaiSongInfoViewModel viewModel)
		{
			return viewModel?.ToZanmaiSongInfo();
		}
	}
}
