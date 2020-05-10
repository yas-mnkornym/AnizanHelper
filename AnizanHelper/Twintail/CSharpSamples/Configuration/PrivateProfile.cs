// PrivateProfile.cs

namespace CSharpSamples
{
	using System.Runtime.InteropServices;
	using System.Text;

	/// <summary>
	/// Windows の ini ファイルを読み書きする API。
	/// </summary>
	public class PrivateProfile
	{
		[DllImport("kernel32.dll", EntryPoint = "GetPrivateProfileInt")]
		private static extern uint GetPrivateProfileInt(string section, string key, int def, string fileName);

		[DllImport("kernel32.dll", EntryPoint = "GetPrivateProfileString")]
		private static extern ulong GetPrivateProfileString(string section, string key, string def, StringBuilder buffer, int bufferSize, string fileName);

		[DllImport("kernel32.dll", EntryPoint = "WritePrivateProfileString")]
		private static extern bool WritePrivateProfileString(string section, string key, string value, string fileName);

		public static uint ReadInt(string section, string key, int def, string fileName)
		{
			return GetPrivateProfileInt(section, key, def, fileName);
		}

		public static ulong ReadString(string section, string key, string def, StringBuilder buffer, int bufferSize, string fileName)
		{
			return GetPrivateProfileString(section, key, def, buffer, bufferSize, fileName);
		}

		public static bool Write(string section, string key, int value, string fileName)
		{
			return WritePrivateProfileString(section, key, value.ToString(), fileName);
		}

		public static bool Write(string section, string key, string value, string fileName)
		{
			return WritePrivateProfileString(section, key, value, fileName);
		}
	}
}
