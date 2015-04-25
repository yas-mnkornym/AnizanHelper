// TwinDll.cs

/*
 *	２ちゃんねる専用ブラウザ twintail用 ライブラリ
 * 
 *								作成日: 2003/10/21
 *								更新日: 2005/01/24
 */

namespace Twin
{
	using System;
	using System.Windows.Forms;
	using System.Reflection;
	using System.IO;
	using System.Text;
	using Twin.Bbs;

	using DebugOutput = System.Diagnostics.Trace;

	public class TwinDll
	{
		static TwinDll()
		{
			TypeCreator.Regist(BbsType.None, typeof(X2chThreadHeader), typeof(X2chThreadReader), typeof(X2chThreadListReader), typeof(X2chPost));
			TypeCreator.Regist(BbsType.Dat, typeof(X2chThreadHeader), typeof(X2chThreadReader), typeof(X2chThreadListReader), typeof(X2chPost));
			TypeCreator.Regist(BbsType.X2chKako, typeof(X2chKakoThreadHeader), typeof(X2chKakoThreadReader), typeof(X2chThreadListReader), typeof(X2chKakoPost));
			TypeCreator.Regist(BbsType.X2chAuthenticate, typeof(X2chThreadHeader), typeof(X2chAuthenticateThreadReader), typeof(X2chThreadListReader), typeof(X2chPost));
			TypeCreator.Regist(BbsType.X2ch, typeof(X2chThreadHeader), typeof(X2chThreadReader), typeof(X2chThreadListReader), typeof(X2chPost));
			TypeCreator.Regist(BbsType.Be2ch, typeof(X2chThreadHeader), typeof(Be2chThreadReader), typeof(Be2chThreadListReader), typeof(Be2chPost));
			TypeCreator.Regist(BbsType.Zeta, typeof(ZetaThreadHeader), typeof(ZetaThreadReader), typeof(ZetaThreadListReader), typeof(ZetaPost));
			TypeCreator.Regist(BbsType.Machi, typeof(MachiThreadHeader), typeof(MachiThreadReader), typeof(MachiThreadListReader), typeof(MachiPost));
			TypeCreator.Regist(BbsType.Jbbs, typeof(JbbsThreadHeader), typeof(JbbsThreadReader), typeof(JbbsThreadListReader), typeof(JbbsPost));
			TypeCreator.Regist(BbsType.MilkCafe, typeof(X2chThreadHeader), typeof(X2chThreadReader), typeof(X2chThreadListReader), typeof(MilkcafePost));
		}

		/// <summary>
		/// argsを書式化してメッセージボックスに表示
		/// </summary>
		/// <param name="obj"></param>
		public static void ShowOutput(string format, params object[] args)
		{
			string text =
				String.Format(format, args);

			ShowOutput((object)text);
		}

		/// <summary>
		/// メッセージボックスでobjを表示
		/// </summary>
		/// <param name="obj"></param>
		public static void ShowOutput(object obj)
		{
			MessageBox.Show(obj.ToString(), "twintail",
				MessageBoxButtons.OK, MessageBoxIcon.Warning);

			Output(obj);
		}

		/// <summary>
		/// デバッグ用の出力メソッド
		/// </summary>
		/// <param name="format"></param>
		/// <param name="arguments"></param>
		public static void Output(string format, params object[] arguments)
		{
			Output((object)String.Format(format, arguments));
		}

		/// <summary>
		/// デバッグ用の出力メソッド
		/// </summary>
		/// <param name="format"></param>
		/// <param name="arguments"></param>
		public static void Output(object obj)
		{
			try {
			string head = String.Format("ver{0} ({1})", Version, DateTime.Now);
			string info = String.Format("{0} CLR {1}", Environment.OSVersion, Environment.Version);

			DebugOutput.WriteLine(head);
			DebugOutput.WriteLine(info);
			DebugOutput.WriteLine(obj.ToString());}catch{}
			DebugOutput.Write("\r\n");
		}

		public static void Debug(string format, params object[] args)
		{
			DebugOutput.WriteLine(
				String.Format(format, args));
		}
 
		/// <summary>
		/// 共通のユーザーエージェントを取得
		/// </summary>
		public static string UserAgent {
			get {
				return String.Format("Monazilla/1.00 (twintail/{0})", Version);
			}
		}

		/// <summary>
		/// IEのユーザーエージェントを取得
		/// </summary>
		public static string IEUserAgent {
			get {
				return "Mozilla/4.0 (compatible; MSIE 6.0; Windows 2000; DigExt)";
			}
		}

		/// <summary>
		/// twin.dllのバージョンを取得
		/// </summary>
		public static Version Version {
			get {
				return Assembly.GetAssembly(typeof(TwinDll)).GetName().Version;
			}
		}

		/// <summary>
		/// 内部で使用するデフォルトのエンコーディングを取得
		/// </summary>
		public static Encoding DefaultEncoding {
			get {
				return Encoding.GetEncoding("Shift_Jis");
			}
		}

		/// <summary>
		/// Be2chの投稿時に送信するクッキーを取得または設定
		/// </summary>
		private static Be2chCookie be2chCookie = new Be2chCookie();

		public static Be2chCookie Be2chCookie {
			set {
				if (value == null)
				{
					be2chCookie = new Be2chCookie();
				}
				else {
					be2chCookie = value;
				}
			}
			get {
				return be2chCookie;
			}
		}

		private static string addAgreementField = String.Empty;
		public static string AditionalAgreementField
		{
			set
			{
				addAgreementField = value;
			}
			get
			{
				return addAgreementField;
			}
		}

		private static string addWriteSection = String.Empty;
		public static string AddWriteSection
		{
			set
			{
				addWriteSection = value;
			}
			get
			{
				return addWriteSection;
			}
		}
	}
}
