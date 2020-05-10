using System;
using System.IO;
using System.Reflection;

namespace AnizanHelper
{
	internal class AppInfo
	{
		protected AppInfo() { }

		private static AppInfo current_ = new AppInfo();
		public static AppInfo Current
		{
			get
			{
				return current_;
			}
		}


		#region StartupPath
		private string startupPath_ = null;
		/// <summary>
		/// プログラム実行ファイルのパスを取得する。
		/// </summary>
		public string StartupPath
		{
			get
			{
				return this.startupPath_ ?? (this.startupPath_ =
					Assembly.GetEntryAssembly().Location);
			}
		}
		#endregion

		#region StartupDirectory
		private string startupDirectory_ = null;

		/// <summary>
		/// プログラム実行ファイルのディレクトリを取得する。
		/// </summary>
		public string StartupDirectory
		{
			get
			{
				return this.startupDirectory_ ?? (this.startupDirectory_ = Path.GetDirectoryName(this.StartupPath));
			}
		}
		#endregion

		#region ExecutionFileName
		private string executionFileName_ = null;

		/// <summary>
		/// プログラム実行ファイルの名前を取得する。
		/// </summary>
		public string ExecutionFileName
		{
			get
			{
				if (this.executionFileName_ == null)
				{
					this.executionFileName_ = Path.GetFileName(this.StartupPath);
				}
				return this.executionFileName_;
			}
		}
		#endregion

		private string dictionaryFilePath_ = null;
		public string DictionaryFilePath
		{
			get
			{
				return this.dictionaryFilePath_ ?? (this.dictionaryFilePath_ =
					Path.Combine(AppInfo.Current.StartupDirectory, Constants.DictionaryFileName));
			}
		}

		private string userDictionaryFilePath_ = null;
		public string UserDictionaryfilePath
		{
			get
			{
				return this.userDictionaryFilePath_ ?? (this.userDictionaryFilePath_ =
					Path.Combine(AppInfo.Current.StartupDirectory, Constants.UserDictionaryFileName));
			}
		}

		public string ZanmaiSearchIndexPath => Path.Combine(AppInfo.Current.StartupDirectory, Constants.AnizanSearchIndexPath);

		#region Version
		private Version version_ = null;
		public Version Version
		{
			get
			{
				return this.version_ ?? (this.version_ = Assembly.GetExecutingAssembly().GetName().Version);
			}
		}
		#endregion
	}
}
