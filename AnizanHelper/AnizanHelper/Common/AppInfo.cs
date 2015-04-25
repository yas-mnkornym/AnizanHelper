using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace AnizanHelper
{
	internal class AppInfo
	{
		protected AppInfo() { }

		static AppInfo current_ = new AppInfo();
		public static AppInfo Current
		{
			get
			{
				return current_;
			}
		}


		#region StartupPath
		string startupPath_ = null;
		/// <summary>
		/// プログラム実行ファイルのパスを取得する。
		/// </summary>
		public string StartupPath
		{
			get
			{
				return startupPath_ ?? (startupPath_ =
					Assembly.GetEntryAssembly().Location);
			}
		}
		#endregion

		#region StartupDirectory
		string startupDirectory_ = null;

		/// <summary>
		/// プログラム実行ファイルのディレクトリを取得する。
		/// </summary>
		public string StartupDirectory
		{
			get
			{
				return startupDirectory_ ?? (startupDirectory_ = Path.GetDirectoryName(StartupPath));
			}
		}
		#endregion

		#region ExecutionFileName
		string executionFileName_ = null;

		/// <summary>
		/// プログラム実行ファイルの名前を取得する。
		/// </summary>
		public string ExecutionFileName
		{
			get
			{
				if (executionFileName_ == null) {
					executionFileName_ = Path.GetFileName(StartupPath);
				}
				return executionFileName_;
			}
		}
		#endregion

		#region Version
		Version version_ = null;
		public Version Version
		{
			get
			{
				return version_ ?? (version_ = Assembly.GetExecutingAssembly().GetName().Version);
			}
		}
		#endregion
	}
}
