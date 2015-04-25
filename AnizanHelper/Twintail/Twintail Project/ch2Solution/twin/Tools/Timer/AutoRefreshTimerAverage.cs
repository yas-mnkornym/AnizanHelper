using System;
using System.Collections.Generic;
using System.Text;
using System.Timers;
using System.Text.RegularExpressions;
using System.Globalization;

namespace Twin.Tools
{
	public class AutoRefreshTimerAverage : AutoRefreshTimerBase
	{
		private Dictionary<ThreadControl, TimerObject> dic = new Dictionary<ThreadControl, TimerObject>();

		public AutoRefreshTimerAverage()
		{
		}

		private int interval = 1000 * 30;
		public override int Interval
		{
			get
			{
				return interval;
			}
			set
			{
				interval = Math.Max(value, 1000 * 30);
			}
		}

		public override void Add(ThreadControl client)
		{
			lock (dic)
			{
				if (dic.ContainsKey(client))
					return;
				dic.Add(client, new TimerObject(this, client, Interval));
			}
		}

		public override void Remove(ThreadControl client)
		{
			lock (dic)
			{
				if (dic.ContainsKey(client))
				{
					dic[client].Dispose();
				}
				dic.Remove(client);
			}
		}

		public override bool Contains(ThreadControl client)
		{
			lock (dic)
				return dic.ContainsKey(client);
		}

		public override int GetInterval(ThreadControl client)
		{
			lock (dic)
			{
				if (!dic.ContainsKey(client))
					return -1;

				TimerObject obj = dic[client];
				return obj.Interval;
			}
		}

		public override int GetTimeleft(ThreadControl client)
		{
			lock (dic)
			{
				if (!dic.ContainsKey(client))
					return -1;

				TimerObject obj = dic[client];
				return obj.Timeleft;

			}
		}

		public override void Clear()
		{
			lock (dic)
			{
				foreach (KeyValuePair<ThreadControl, TimerObject> kv in dic)
					kv.Value.Dispose();

				dic.Clear();
			}
		}

		public override ITimerObject GetTimerObject(ThreadControl client)
		{
			lock (dic)
			{
				if (dic.ContainsKey(client))
					return dic[client];
				else
					return null;
			}
		}

		public class TimerObject : ITimerObject
		{
			private AutoRefreshTimerBase parent;

			private static Regex daterex = new Regex(@"(?<d>\d{4}/\d{2}/\d{2})\(\w\)\s(?<t>\d{2}:\d{2}:\d{2})");
			private ThreadControl client;
			private Timer timer;
			private int startTick = 0;
			private bool timerEnabled = false;
			public int Interval
			{
				get
				{
					return (int)timer.Interval;
				}
				set
				{
					timer.Interval = value;
				}
			}
			public int Timeleft
			{
				get
				{
					if (startTick == 0)
						return 0;

					return (Interval - (Environment.TickCount - startTick)) / 1000;
				}
			}
			public bool Enabled
			{
				get
				{
					return timerEnabled;
				}
				set
				{
					timerEnabled = value;
					if (timerEnabled == false)
					{
						timer.Stop();
					}
				}
			}

			public TimerObject(AutoRefreshTimerBase parent, ThreadControl client, int interval)
			{
				this.parent = parent;
				this.client = client;
				client.Complete += new CompleteEventHandler(client_Complete);
				this.timer = new Timer();
				timer.Elapsed += new ElapsedEventHandler(timer_Elapsed);
				timer.AutoReset = false;
				ResetStart();
			}

			void client_Complete(object sender, CompleteEventArgs e)
			{
				timer.Stop();
				if (client.HeaderInfo.IsLimitOverThread || client.HeaderInfo.Pastlog)
				{
					Stop();
					parent.Remove(this.client);
				}
				else
				{
					ResetStart();
				}
			}

			void timer_Elapsed(object sender, ElapsedEventArgs e)
			{
				if (client.IsOpen)
					client.Reload();
			}

			private void CalcInterval()
			{
				int length = client.ResSets.Count;
				if (length == 0)
				{
					timer.Interval = 30000;
					return;
				}
				try
				{
					/*
					int count = Math.Min(10, length);
					TimeSpan span = TimeSpan.Zero;
					ResSet res = client.ResSets[length - count];
					Match m = daterex.Match(res.DateString);
					if (m.Success)
					{
						string dateStr = m.Groups["d"].Value + m.Groups["t"].Value;
						DateTime dt;
						if (DateTime.TryParseExact(dateStr, "yyyy/MM/ddHH:mm:ss", null, DateTimeStyles.None, out dt))
						{
							span = DateTime.Now - dt;
						}
					}

					int averageSeconds = (int)span.TotalSeconds / count;
					timer.Interval = Math.Max(10000, averageSeconds * 1000);*/

					DateTime[] dt = new DateTime[3];
					dt[0] = dt[1] = dt[2] = DateTime.Now;

					for (int i = 1; i <= 2 && (length-i) >= 0; i++)
					{
						ResSet res = client.ResSets[length - i];
						DateTime ret;

						Match m = daterex.Match(res.DateString);
						if (m.Success)
						{
							DateTime.TryParseExact(m.Groups["d"].Value + m.Groups["t"].Value,
								"yyyy/MM/ddHH:mm:ss", null, DateTimeStyles.None, out ret);

							if (ret != DateTime.MinValue)
								dt[i] = ret;
						}
					}

					TimeSpan span = CalcAverageInterval(dt[0], dt[1], dt[2]);
					int seconds = (int)Math.Min(span.TotalSeconds, 2000000);
					timer.Interval = Math.Max(10000, seconds * 1000);
				}
				catch
				{
				}
			}

			private TimeSpan CalcAverageInterval(DateTime now, DateTime last1, DateTime last2)
			{
				TimeSpan s1 = now - last1;
				TimeSpan s2 = last1 - last2;
				return new TimeSpan(0, 0, (int)(s1.TotalSeconds * 2 + s2.TotalSeconds) / 3);
			}

			public void ResetStart()
			{
				timer.Stop();
				CalcInterval();
				startTick = Environment.TickCount;
				timer.Start();
			}

			public void Stop()
			{
				startTick = 0;
				timer.Stop();
			}

			public void Dispose()
			{
				client.Complete -= client_Complete;
				timer.Dispose();
				timer = null;
			}
		}

	}

}
