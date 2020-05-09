using System;
using System.Reactive.Subjects;

namespace AnizanHelper
{
	public class MessageService
	{
		protected MessageService() { }

		private static MessageService current_ = null;
		public static MessageService Current
		{
			get
			{
				return current_ ?? (current_ = new MessageService());
			}
		}

		private Subject<string> subject_ = new Subject<string>();
		public IObservable<string> MessageObservable
		{
			get
			{
				return this.subject_;
			}
		}
		public void ShowMessage(string message)
		{
			this.subject_.OnNext(message);
		}
	}
}
