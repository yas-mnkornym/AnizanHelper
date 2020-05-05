using System.Windows;

namespace AnizanHelper.Views
{
	public partial class SongListWindow : Window
	{
		public SongListWindow()
		{
			this.InitializeComponent();
			this.Closing += this.SongListWindow_Closing;
		}

		private void SongListWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			if (!this.canClose_)
			{
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
