using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Text;

namespace AnizanHelper
{
	public class MessageService
	{
		protected MessageService() { }

		static MessageService current_ = null;
		public static MessageService Current
		{
			get
			{
				return current_ ?? (current_ = new MessageService());
			}
		}


		Subject<string> subject_ = new Subject<string>();
		public IObservable<string> MessageObservable
		{
			get
			{
				return subject_;
			}
		}
		public void ShowMessage(string message)
		{
			subject_.OnNext(message);
		}
	}
}
