using System.Windows;
using System.Windows.Controls;

namespace AnizanHelper.Views.Pages
{
	public partial class StreamMetadataViewerPage : UserControl
	{
		public StreamMetadataViewerPage()
		{
			this.InitializeComponent();
		}

		private void PresetButton_Click(object sender, RoutedEventArgs e)
		{
			this.DropDownButton_RegexPresets.IsOpen = false;
		}

		private void SavePresetButton_Click(object sender, RoutedEventArgs e)
		{
			this.DropDownButton_RegexPresets.IsOpen = false;
		}

		private void ListView_Encoding_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (e.RemovedItems.Count != 0)
			{
				this.DropDownButton_EncodingSelector.IsOpen = false;
			}
		}
	}
}
