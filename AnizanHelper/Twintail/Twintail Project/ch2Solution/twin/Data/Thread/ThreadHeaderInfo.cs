// ThreadHeaderInfo.cs

namespace Twin
{
	using System;
	using System.ComponentModel;

	/// <summary>
	/// スレッドの各情報を管理
	/// </summary>
	public class ThreadHeaderInfo
	{
		private ThreadHeader header;

		/// <summary>
		/// ThreadHeaderInfoクラスのインスタンスを初期化
		/// </summary>
		/// <param name="h"></param>
		public ThreadHeaderInfo(ThreadHeader h)
		{
			// 
			// TODO: コンストラクタ ロジックをここに追加してください。
			//
			header = h;
		}

		/// <summary>
		/// 1日あたりのレス数を取得
		/// </summary>
		public float ForceValueDay {
			get {
				TimeSpan span = DateTime.Now - header.Date.ToLocalTime();
				return (float)Math.Round(header.ResCount / span.TotalDays, 1);
			}
		}

		/// <summary>
		/// 1時間あたりのレス数を取得
		/// </summary>
		public float ForceValueHour {
			get {
				TimeSpan span = DateTime.Now - header.Date.ToLocalTime();
				return (float)Math.Round(header.ResCount / span.TotalHours, 1);
			}
		}

		/// <summary>
		/// このスレッドの重要度を計算
		/// </summary>
		public int Valuable {
			get {
				// 更新スレ = 3、 全既得スレ = 2、新着スレ = 1, 通常スレ = 0
				if (header.IsNewThread) return 1;
				if (header.GotResCount == 0) return 0;
				if (header.ResCount == header.GotResCount) return 2;
				if (header.SubNewResCount > 0) return 3;

				return 0;
			}
		}

		/// <summary>
		/// このスレッドが24時間以内に立てられたスレッドかどうか
		/// </summary>
		public bool Within24Hours {
			get { return IsWithinHours(24); }
		}

		/// <summary>
		/// このスレッドがhours時間以内に立てられたスレッドかどうか
		/// </summary>
		/// <param name="hours"></param>
		/// <returns></returns>
		public bool IsWithinHours(int hours)
		{
			DateTime date = header.Date.AddHours(hours);
			return (DateTime.Now <= date) ? true : false;
		}

		/// <summary>
		/// 勢いを計算し、結果を文字列形式で返す
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public string GetForceValue(ForceValueOf type)
		{
			if (type == ForceValueOf.Day)
			{
				return ForceValueDay.ToString("0.0d");
			}
			else {
				return ForceValueHour.ToString("0.0h");
			}
		}

		public static string GetForceValue(ThreadHeader h, ForceValueOf valueType)
		{
			return new ThreadHeaderInfo(h).GetForceValue(valueType);
		}
	}

	
	/// <summary>
	/// 勢いを計算するときに使用する単位を表す
	/// </summary>
	public enum ForceValueOf
	{
		/// <summary>
		/// 一時間あたりの勢い
		/// </summary>
		[Description("1時間")]
		Hour,
		/// <summary>
		/// 一日あたりの勢い
		/// </summary>
		[Description("1日")]
		Day,
	}
}
