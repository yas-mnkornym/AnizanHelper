using System.ComponentModel;
using System.Runtime.CompilerServices;
using Studiotaiha.Toolkit;

namespace AnizanHelper.Models.SettingComponents
{
	public class NotificationObjectWithNotifyChaning : NotificationObject, INotifyPropertyChanging
	{
		public NotificationObjectWithNotifyChaning(IDispatcher dispatcher = null) : base(dispatcher)
		{
		}

		public NotificationObjectWithNotifyChaning(IDispatcher dispatcher, bool enableAutoDispatch) : base(dispatcher, enableAutoDispatch)
		{
		}

		protected void RaisePropertyChanging([CallerMemberName]string propertyName = null)
		{
			this.PropertyChanging?.Invoke(this, new PropertyChangingEventArgs(propertyName));
		}

		public event PropertyChangingEventHandler PropertyChanging;
	}
}
