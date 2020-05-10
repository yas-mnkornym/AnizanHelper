// Lha.cs

using System;
using System.Runtime.InteropServices;

namespace CSharpSamples
{
	/// <summary>
	/// Lha型式、ファイルを解凍／圧縮する機能を提供します。
	/// このクラスはスレッドアンセーフです。
	/// </summary>
	public class Lha : IArchivable
	{
		/// <summary>
		/// 圧縮に対応しているかどうかを示す値を返します。
		/// このプロパティは常に true です。
		/// </summary>
		public bool CanCompress
		{
			get
			{
				return true;
			}
		}

		/// <summary>
		/// 解凍に対応しているかどうかを示す値を返します。
		/// このプロパティは常に true です。
		/// </summary>
		public bool CanExtract
		{
			get
			{
				return true;
			}
		}

		/// <summary>
		/// Unlha32.dll のバージョンを取得します。
		/// </summary>
		public float Version
		{
			get
			{
				int ver = UnlhaGetVersion();
				return Convert.ToSingle(ver / 100);
			}
		}

		/// <summary>
		/// 現在 Unlha32.dll が動作中かどうかを判断します。
		/// </summary>
		public bool IsRunning
		{
			get
			{
				int ret = UnlhaGetRunning();
				return Convert.ToBoolean(ret);
			}
		}

		/// <summary>
		/// バックグラウンドモードかどうかを取得または設定します。
		/// </summary>
		public bool IsBackground
		{
			set
			{
				int boolean = Convert.ToInt32(value);
				UnlhaSetBackGroundMode(boolean);
			}
			get
			{
				int ret = UnlhaGetBackGroundMode();
				return Convert.ToBoolean(ret);
			}
		}

		/// <summary>
		/// Unlha32.dll の動作中にカーソルを表示するモードかどうかを取得または設定します。
		/// </summary>
		public bool CursorMode
		{
			set
			{
				int boolean = Convert.ToInt32(value);
				UnlhaSetCursorMode(boolean);
			}
			get
			{
				int ret = UnlhaGetCursorMode();
				return Convert.ToBoolean(ret);
			}
		}

		/// <summary>
		/// Lha クラスのインスタンスを初期化します。
		/// </summary>
		public Lha()
		{
		}

		/// <summary>
		/// Unlha 関数を直接呼び出します。
		/// </summary>
		/// <param name="format"></param>
		/// <param name="objects"></param>
		/// <returns></returns>
		public int Unlha(string format, params object[] objects)
		{
			string cmdline = string.Format(format, objects);
			return this.Unlha(cmdline);
		}

		/// <summary>
		/// Unlha 関数を直接呼び出します。
		/// </summary>
		/// <param name="cmdline"></param>
		/// <returns></returns>
		public int Unlha(string cmdline)
		{
			if (cmdline == null)
			{
				throw new ArgumentNullException("cmdline");
			}

			return Unlha(IntPtr.Zero, cmdline, null, 0);
		}

		/// <summary>
		/// ファイルを圧縮します。
		/// </summary>
		/// <param name="archive">作成するアーカイブ名。</param>
		/// <param name="fileName">圧縮するファイル名。</param>
		/// <returns>正常終了なら 0 を返し、以上終了の場合は 0 以外の値を返します。</returns>
		public int Compress(string archive, string fileName)
		{
			if (archive == null)
			{
				throw new ArgumentNullException("archive");
			}
			if (fileName == null)
			{
				throw new ArgumentNullException("fileName");
			}

			string cmdline = string.Format("a \"{0}\" \"{1}\"", archive, fileName);

			return Unlha(IntPtr.Zero, cmdline, null, 0);
		}

		/// <summary>
		/// 複数のファイルをまとめて圧縮します。
		/// </summary>
		/// <param name="archive">作成するアーカイブ名。</param>
		/// <param name="fileName">圧縮するファイル名の配列。</param>
		/// <returns>正常終了なら 0 を返し、以上終了の場合は 0 以外の値を返します。</returns>
		public int Compress(string archive, string[] fileNames)
		{
			if (archive == null)
			{
				throw new ArgumentNullException("archive");
			}
			if (fileNames == null)
			{
				throw new ArgumentNullException("fileNames");
			}

			int ret = 0;

			foreach (string fileName in fileNames)
			{
				string cmdline = string.Format("a \"{0}\" \"{1}\"", archive, fileName);
				ret = Unlha(IntPtr.Zero, cmdline, null, 0);

				if (ret != 0)
				{
					break;
				}
			}

			return ret;
		}

		/// <summary>
		/// アーカイブを解凍します。
		/// </summary>
		/// <param name="archive">解凍するアーカイブ。</param>
		/// <param name="directory">出力ディレクトリ。</param>
		/// <param name="fileName">解凍するファイル名</param>
		/// <returns>正常終了なら 0 を返し、以上終了の場合は 0 以外の値を返します。</returns>
		public int Extract(string archive, string directory, string fileName)
		{
			if (archive == null)
			{
				throw new ArgumentNullException("archive");
			}
			if (directory == null)
			{
				throw new ArgumentNullException("directory");
			}
			if (fileName == null)
			{
				throw new ArgumentNullException("fileName");
			}

			string cmdline = string.Format("e \"{0}\" \"{1}\" \"{2}\"",
				archive, directory, fileName);

			return Unlha(IntPtr.Zero, cmdline, null, 0);
		}

		/// <summary>
		/// アーカイブを解凍します。
		/// </summary>
		/// <param name="archive">解凍するアーカイブ。</param>
		/// <param name="directory">出力ディレクトリ。</param>
		/// <returns>正常終了なら 0 を返し、以上終了の場合は 0 以外の値を返す。</returns>
		public int Extract(string archive, string directory)
		{
			if (archive == null)
			{
				throw new ArgumentNullException("archive");
			}
			if (directory == null)
			{
				throw new ArgumentNullException("directory");
			}

			string cmdline = string.Format("e \"{0}\" -c -y \"{1}\" *.*",
				archive, directory);

			return Unlha(IntPtr.Zero, cmdline, null, 0);
		}

		[DllImport("Unlha32.dll")]
		private static extern int UnlhaGetCursorMode();

		[DllImport("Unlha32.dll")]
		private static extern int UnlhaSetCursorMode(int bCursorMode);

		[DllImport("Unlha32.dll")]
		private static extern int UnlhaGetBackGroundMode();

		[DllImport("Unlha32.dll")]
		private static extern int UnlhaSetBackGroundMode(int backGroundMode);

		[DllImport("Unlha32.dll")]
		private static extern int UnlhaGetVersion();

		[DllImport("Unlha32.dll")]
		private static extern int UnlhaGetRunning();

		[DllImport("Unlha32.dll")]
		private static extern int Unlha(IntPtr hwnd, string szCmdLine, string buffer, int buffSize);

	}
}
