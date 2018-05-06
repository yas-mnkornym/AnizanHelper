using System.Windows;

namespace AnizanHelper.Views
{
	/// <summary>
	/// SongListWindow.xaml の相互作用ロジック
	/// </summary>
	public partial class SongListWindow : Window
	{
		public SongListWindow()
		{
			InitializeComponent();
			this.Closing += SongListWindow_Closing;
		}

		private void SongListWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			if (!canClose_) {
				this.Hide();
				e.Cancel = true;
			}
		}

		bool canClose_ = false;
		public void CloseImmediately()
		{
			this.canClose_ = true;
			this.Close();
		}
	}
}
