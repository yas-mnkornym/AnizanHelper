using System;
using System.Collections.Generic;
using System.Text;

namespace Twin.Tools
{
	public interface ITimerObject
	{
		void ResetStart();
		void Stop();
		int Timeleft
		{
			get;
		}
		int Interval
		{
			get;
			set;
		}
		bool Enabled
		{
			get;
			set;
		}
	}
}
