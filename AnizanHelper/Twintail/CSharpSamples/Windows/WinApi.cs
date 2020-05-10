// WinApi.cs

namespace CSharpSamples.Winapi
{
	using System;
	using System.Diagnostics;
	using System.Drawing;
	using System.IO;
	using System.Runtime.InteropServices;
	using System.Text;

	/// <summary>
	/// shlwapi.dllのPath系関数群
	/// 参考URL: http://nienie.com/~masapico/
	/// </summary>
	public class Shlwapi
	{
		/// <summary>
		/// fromPathからtoPathへの相対パスを取得
		/// </summary>
		/// <param name="fromPath">参照元のパス</param>
		/// <param name="toPath">参照先のパス</param>
		/// <returns>相対パス (エラーであればnullを返す)</returns>
		public static string GetRelativePath(
			string fromPath, string toPath)
		{
			StringBuilder sb = new StringBuilder(512);
			FileAttributes attrfrom = File.GetAttributes(fromPath);
			FileAttributes attrto = File.GetAttributes(toPath);

			if (PathRelativePathTo(sb, fromPath, (uint)attrfrom, toPath, (uint)attrto) != 0)
			{
				return sb.ToString();
			}

			// 失敗
			return null;
		}

		/// <summary>
		/// 文字数を指定してパスを縮める
		/// </summary>
		/// <param name="path">縮めるパス</param>
		/// <param name="count">何文字に縮めるかを指定</param>
		/// <returns>count文字に縮められたパス</returns>
		public static string GetCompactPath(string path, int count)
		{
			StringBuilder sb = new StringBuilder(512);

			if (PathCompactPathEx(sb, path, (uint)count, '\\') != 0)
			{
				return sb.ToString();
			}

			return null;
		}

		/// <summary>
		/// ..\ などを含むパスの変換
		/// </summary>
		/// <param name="path">変換対象のパス</param>
		/// <returns>変換後のパス</returns>
		public static string Canonicalize(string path)
		{
			StringBuilder sb = new StringBuilder(512);

			if (PathCanonicalize(sb, path) != 0)
			{
				return sb.ToString();
			}

			return null;
		}

		/// <summary>
		/// pathのフルパスを求める
		/// </summary>
		/// <param name="path">フルパスを求めるパス</param>
		/// <returns>求めたフルパス</returns>
		public static string GetFullPath(string path)
		{
			StringBuilder sb = new StringBuilder(512);

			if (PathSearchAndQualify(path, sb, 512) != 0)
			{
				return sb.ToString();
			}

			return null;
		}

		[DllImport("shlwapi.dll")]
		private static extern int PathRelativePathTo(
			StringBuilder sb, string from, uint attrfrom, string to, uint attrto);

		[DllImport("shlwapi.dll")]
		private static extern int PathCompactPathEx(
			StringBuilder dest, string src, uint count, ulong flags);

		[DllImport("shlwapi.dll")]
		private static extern int PathCanonicalize(StringBuilder dest, string src);

		[DllImport("shlwapi.dll")]
		private static extern int PathSearchAndQualify(
			string path, StringBuilder fullyQualifiedPath, uint fullyQualifiedPathSize);
	}

	/// <summary>
	/// WinAPI の概要の説明です。
	/// </summary>
	public class WinApi
	{
		/// <summary>アプリケーション用定義メッセージ</summary>
		public const int WM_APP = 0x8000;

		[DllImport("user32.dll")]
		public static extern int SetForegroundWindow(IntPtr hwnd);

		[DllImport("user32.dll")]
		public static extern int IsIconic(IntPtr hwnd);

		[DllImport("user32.dll")]
		public static extern int OpenIcon(IntPtr hwnd);

		[DllImport("user32.dll")]
		public static extern int MessageBeep(uint type);

		[DllImport("user32.dll")]
		public static extern int SendMessage(IntPtr hwnd, uint uMsg, uint wParam, long lParam);

		[DllImport("user32.dll")]
		public static extern int SendMessage(IntPtr hwnd, uint uMsg, int wParam, long lParam);

		[DllImport("user32.dll")]
		public static extern long GetWindowLong(IntPtr hwnd, int index);

		[DllImport("user32.dll")]
		public static extern long SetWindowLong(IntPtr hwnd, int index, ulong newLong);

		[DllImport("kernel32.dll")]
		public static extern int GlobalFindAtom(string atomString);

		[DllImport("kernel32.dll")]
		public static extern int GlobalAddAtom(string atomString);

		[DllImport("kernel32.dll")]
		public static extern int GlobalDeleteAtom(int atom);

		[DllImport("kernel32.dll")]
		public static extern int GlobalGetAtomName(int atom, StringBuilder buffer, int bufferSize);

		[DllImport("gdi32.dll")]
		public static extern int GetTextExtentPoint32(IntPtr hdc, string lpString, int cbString, ref Size size);

		[DllImport("uxtheme.dll", CharSet = CharSet.Unicode)]
		public static extern int SetWindowTheme(IntPtr hWnd, string pszSubAppName, string pszSubIdList);

	}

	/// <summary>
	/// WindowsAPIのちょっとしたラップ
	/// </summary>
	public class WrapApi
	{
		/// <summary>
		/// ビープ音を鳴らす
		/// </summary>
		/// <param name="type">再生するビープの種類を表すBeepSound列挙体</param>
		/// <returns>再生に成功したらtrue、失敗したらfalse</returns>
		public static bool Beep(BeepSound type)
		{
			return WinApi.MessageBeep((uint)type) != 0 ? true : false;
		}

		/// <summary>
		/// 指定したウインドウハンドルを最前面に表示。
		/// 最小化されていた場合は元のサイズに戻す。
		/// </summary>
		/// <param name="hwnd">最前面に表示するウインドウのハンドル</param>
		public static void SetForegroundWindow(IntPtr hwnd)
		{
			// なぜか失敗する
			if (WinApi.IsIconic(hwnd) != 0)
			{
				Debug.Assert(WinApi.OpenIcon(hwnd) != 0);
			}
			else
			{
				Debug.Assert(WinApi.SetForegroundWindow(hwnd) != 0);
			}
		}
	}

	/// <summary>
	/// ビープ音の種類を表す
	/// </summary>
	public enum BeepSound : long
	{
		/// <summary>
		/// コンピュータのスピーカから発生する標準的なビープ音
		/// </summary>
		Default = 0xFFFFFFFF,
		/// <summary>
		/// 一般の警告音
		/// </summary>
		OK = 0x00000000L,
		/// <summary>
		/// システムエラー
		/// </summary>
		Hand = 0x00000010L,
		/// <summary>
		/// メッセージ（問い合わせ）
		/// </summary>
		Question = 0x00000020L,
		/// <summary>
		/// メッセージ（警告）
		/// </summary>
		Exclamation = 0x00000030L,
		/// <summary>
		/// メッセージ（情報）
		/// </summary>
		Asterisk = 0x00000040L,
	}

	/// <summary>
	/// Windowsメッセージを表す
	/// </summary>
	public enum Wm
	{
		User = 0x0400,
		App = 0x8000,
	}

	/// <summary>
	/// コモンコントロールメッセージ
	/// </summary>
	public enum Ccm
	{
		First = 0x2000,
		SetBkColor = 0x2000 + 1,
	}

	/// <summary>
	/// WindowLong
	/// </summary>
	public enum WindowLong
	{
		WndProc = (-4),
		hInstance = (-6),
		HwndParent = (-8),
		Style = (-16),
		ExStyle = (-20),
		UserData = (-21),
		ID = (-12),
	}
}
