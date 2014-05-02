using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Threading;
using System.Runtime.Serialization;

namespace AnizanHelper
{
	abstract public class BaseNotifyPropertyChanged : INotifyPropertyChanged
	{
		public Dispatcher Dispatcher { get; protected set; }

		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="dispatcher">ディスパッチャ</param>
		protected BaseNotifyPropertyChanged(Dispatcher dispatcher = null)
		{
			Dispatcher = dispatcher;
		}

		public event PropertyChangedEventHandler PropertyChanged;

		/// <summary>
		/// プロパティの変更を通知する
		/// </summary>
		/// <param name="propertyName">プロパティ名</param>
		protected void RaisePropertyChanged(string propertyName)
		{
			if (PropertyChanged != null) {
				Dispatch(() => {
					PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
				});
			}
		}

		/// <summary>
		/// Dispatcherのスレッドに処理をディスパッチする。
		/// </summary>
		/// <param name="act">ディスパッチする処理</param>
		protected void Dispatch(Action act)
		{
			if (Dispatcher != null && Dispatcher.Thread.ManagedThreadId != System.Threading.Thread.CurrentThread.ManagedThreadId) {
				Dispatcher.Invoke(act);
			}
			else {
				act();
			}
		}
	}
}
