using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace AnizanHelper
{
	internal class MainWindowViewModel : BaseNotifyPropertyChanged
	{
		public MainWindowViewModel(Dispatcher dispatcher)
			: base(dispatcher)
		{ }
	}
}
