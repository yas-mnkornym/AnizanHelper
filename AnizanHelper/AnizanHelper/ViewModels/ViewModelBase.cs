using Studiotaiha.Toolkit;

namespace AnizanHelper.ViewModels
{
	public class ViewModelBase : NotificationObjectWithPropertyBag
	{
		public ViewModelBase(IDispatcher dispatcher = null) : base(dispatcher)
		{
		}

		public ViewModelBase(IDispatcher dispatcher, bool enableAutoDispatch) : base(dispatcher, enableAutoDispatch)
		{
		}
	}
}
