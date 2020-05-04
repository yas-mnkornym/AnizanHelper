using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AnizanHelper.Views
{
	/// <summary>
	/// Interaction logic for SongMetadataViewerControl.xaml
	/// </summary>
	public partial class SongMetadataViewerControl : UserControl
	{
		public SongMetadataViewerControl()
		{
			InitializeComponent();
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
