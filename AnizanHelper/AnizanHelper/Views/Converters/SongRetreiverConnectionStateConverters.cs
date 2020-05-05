using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using AnizanHelper.ViewModels.Pages;

namespace AnizanHelper.Views.Converters
{
	public class SongRetreiverConnectionStateConverterBase<T> : IValueConverter
	{
		public T StoppedValue { get; set; }
		public T ConnectingValue { get; set; }
		public T RunningValue { get; set; }
		public T ReconnectingValue { get; set; }

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return ((SongRetreiverConnectionState)value) switch
			{
				SongRetreiverConnectionState.Stopped => this.StoppedValue,
				SongRetreiverConnectionState.Running => this.RunningValue,
				SongRetreiverConnectionState.Connecting => this.ConnectingValue,
				SongRetreiverConnectionState.Reconnecting => this.ReconnectingValue,
				_ => throw new NotSupportedException(),
			};
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotSupportedException();
		}
	}

	public class SongRetreiverConnectionStateBoolConverter : SongRetreiverConnectionStateConverterBase<bool>
	{
	}

	public class SongRetreiverConnectionStateVisibilityConverter : SongRetreiverConnectionStateConverterBase<Visibility>
	{
	}

	public class SongRetreiverConnectionStateStringConverter : SongRetreiverConnectionStateConverterBase<string>
	{
	}

	public class SongRetreiverConnectionStateBrushConverter : SongRetreiverConnectionStateConverterBase<Brush>
	{
	}
}
