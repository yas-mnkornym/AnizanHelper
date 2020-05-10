// X2chAuthenticator.cs

namespace Twin.Bbs
{
	using System;
	using System.Diagnostics;
	using System.Net;
	using System.Runtime.InteropServices;
	using System.Text;

	/// <summary>
	/// Summary description for X2chAuthenticator.
	/// </summary>
	public class X2chAuthenticator
	{
#if DEBUG
		public static bool __UseSampleSID = false;
		static string __SampleSID = "Monazilla/2.00:4373298c8948z3710L4758Y0624V8081Z8057C1299i5093s20646o2811s45242k42852u6725y95346g6820L6383H0297o62124l2450n64672G6826N2472L7957N2508x9686O8904U4108793x6855v1216b1499s6811a2729r";
#endif

		private	const string serverUrl = "2chv.tora3.net";
		private const string authenticateObject = "futen.cgi";

		private const int sessionLimitHours = 3;

		private static readonly X2chAuthenticator authenticator = new X2chAuthenticator();

		private const string sessionIdString = "session-id=";

		private string loginId = String.Empty;
		private string password = String.Empty;
		private string connectedLoginId = String.Empty;
		private string connectedPassword = String.Empty;
		private	string sessionId = String.Empty;
		private DateTime lastConnected;

		#region	WinInet	specific definitions
		private const uint  INTERNET_OPEN_TYPE_PRECONFIG = 0;
        
		private const uint  INTERNET_DEFAULT_HTTPS_PORT = 443;
        
		private	const uint  INTERNET_SERVICE_FTP	= 1;
		private	const uint	INTERNET_SERVICE_GOPHER	= 2;
		private	const uint	INTERNET_SERVICE_HTTP =	3;

		private const uint  INTERNET_FLAG_DONT_CACHE		= 0x04000000;
		private const uint  INTERNET_FLAG_SECURE            = 0x00800000;   // use PCT/SSL if applicable (HTTP)
		private const uint  INTERNET_FLAG_KEEP_CONNECTION   = 0x00400000;   // use keep-alive semantics

		[DllImport("Wininet.dll")]
		private	static extern IntPtr InternetOpen(
			string lpszAgent,
			uint dwAccessType,
			string lpszProxy,
			string lpszProxyBypass,
			uint dwFlags
			);
		[DllImport("Wininet.dll")]
		private	static extern IntPtr InternetConnect(
			IntPtr hInternet,
			string lpszServerName,
			uint port, // nServerPort
			string lpszUserName,
			string lpszPassword,
			uint dwService,
			uint dwFlags,
			IntPtr dwContext
			);

		[DllImport("Wininet.dll")]
		private	static extern bool InternetCloseHandle(IntPtr hInternet);

		[DllImport("Wininet.dll")]
		private static extern IntPtr HttpOpenRequest(
			IntPtr hConnect,
			string Verb,
			string ObjectName,
			string Version,
			string Referer,
			IntPtr AcceptTypes,
			uint Flags,
			uint Context
			);

		[DllImport("Wininet.dll")]
		private static extern bool HttpSendRequest(
			IntPtr hRequest,
			string lpszHeaders,
			int dwHeadersLength,
			byte[] lpOptional,
			int dwOptionalLength
			);

		[DllImport("Wininet.dll")]
		private static extern bool InternetReadFile(
			IntPtr hRequest,
			byte[] Buf,
			int BufSize,
			out int ReadSize
			);
		#endregion

		#region コンストラクタ
		/// <summary>
		/// プライベートコンストラクタ
		/// </summary>
		/// <remarks>X2chAuthenticatorを使用するためには<see cref="GetInstance"/>でインスタンスを取得する必要があります。</remarks>
		static X2chAuthenticator() { Disable(); }
		#endregion

		#region 初期化･有効化・無効化
		/// <summary>
		/// 認証を有効化します
		/// </summary>
		public static void Enable(string loginId, string password)
		{
			authenticator.loginId = loginId;
			authenticator.password = password;
		}

		/// <summary>
		/// 認証を無効化します
		/// </summary>
		public static void Disable()
		{
			authenticator.loginId = String.Empty;
			authenticator.password = String.Empty;
			authenticator.connectedLoginId = String.Empty;
			authenticator.connectedPassword = String.Empty;
			authenticator.sessionId = String.Empty;
		}
		#endregion

		#region インスタンス作成と接続
		public static X2chAuthenticator GetInstance()
		{
			if (authenticator.IsConnectNeeded())
			{
				authenticator.Connect(authenticator.loginId, authenticator.password);
			}
			return authenticator;
		}

		private bool IsConnectNeeded()
		{
			return IsNewIdOrPassword() || IsSessionTooOld();
		}

		private bool IsNewIdOrPassword()
		{
			return !(this.loginId.Equals(this.connectedLoginId) && this.password.Equals(this.connectedPassword));
		}

		private bool IsSessionTooOld()
		{
			TimeSpan span = DateTime.Now - this.lastConnected;
			return span.Hours >= sessionLimitHours;
		}

		private	void Connect(string	loginId, string	password)
		{
			IntPtr inetHandle =	IntPtr.Zero;
			IntPtr connectionHandle	= IntPtr.Zero;
			IntPtr requestHandle = IntPtr.Zero;

			try
			{
				this.loginId = loginId;
				this.password = password;
				this.sessionId = String.Empty;
				this.lastConnected = DateTime.Now;

				inetHandle = InternetOpen("DOLIB/1.00",	INTERNET_OPEN_TYPE_PRECONFIG, null, null, 0);
				connectionHandle = InternetConnect(inetHandle, serverUrl, INTERNET_DEFAULT_HTTPS_PORT, null, null, INTERNET_SERVICE_HTTP, 0, IntPtr.Zero);
				requestHandle = HttpOpenRequest(connectionHandle, "POST", authenticateObject, null, null, IntPtr.Zero, INTERNET_FLAG_DONT_CACHE | INTERNET_FLAG_SECURE, 0);

				if (requestHandle != IntPtr.Zero)
				{
					if (RequestAuthentication(requestHandle, loginId, password))
					{
						this.connectedLoginId = loginId;
						this.connectedPassword = password;
						this.sessionId = GetSessionId(requestHandle);
					}
				}
			}
			catch (ArgumentException ex) 
			{
				throw ex;
			}
			catch (WebException) 
			{
			}
			catch (Exception ex) 
			{
				Debug.WriteLine(ex.ToString());
			}
			finally	
			{
				if (!inetHandle.Equals(IntPtr.Zero))
				{
					InternetCloseHandle(inetHandle);
				}
				if (!connectionHandle.Equals(IntPtr.Zero))
				{
					InternetCloseHandle(connectionHandle);
				}
				if (requestHandle != IntPtr.Zero)
				{
					InternetCloseHandle(requestHandle);
				}
			}
		}

		private bool RequestAuthentication(IntPtr requestHandle, string loginId, string password)
		{
			byte[] body = Encoding.UTF8.GetBytes(string.Format("ID={0}&PW={1}", loginId, password));
			string x2chUserAgent = "X-2ch-UA:twintail/2.0";
			return HttpSendRequest(requestHandle, x2chUserAgent, x2chUserAgent.Length, body, body.Length);
		}

		private string GetSessionId(IntPtr requestHandle)
		{
			StringBuilder result = new StringBuilder();
			byte[] buf = new byte[512];
			int readSize;

			while (InternetReadFile(requestHandle, buf, buf.Length, out readSize))
			{
				if (readSize == 0)
					break;

				result.Append(Encoding.UTF8.GetString(buf, 0, readSize));
			}

			return ExtractSessionId(result.ToString());
		}

		private string ExtractSessionId(string serverResponse)
		{
			string sessionId = null;
			if (serverResponse != null && serverResponse.Length > 0)
			{
				if (serverResponse.ToLower().StartsWith(sessionIdString))
				{
					sessionId = serverResponse.Substring(sessionIdString.Length).TrimEnd(new char[]{'\n'});
				}
			}
			return sessionId;
		}
		#endregion

		#region Properties
		/// <summary>
		/// 認証された SessionId
		/// </summary>
		public string SessionId
		{
			get
			{
#if DEBUG
				if (__UseSampleSID)
					return __SampleSID;
#endif
				return sessionId;
			}
		}

		/// <summary>
		/// 有効なSessionIdの有無
		/// </summary>
		public bool HasSession
		{
			get
			{
#if DEBUG
				if (__UseSampleSID)
					return true;
#endif
				return IsValidSessionId(SessionId);
			}
		}
		#endregion

		#region Test Method
		public static bool IsValidUsernamePassword(string username, string password)
		{
			if (String.IsNullOrEmpty(username) || String.IsNullOrEmpty(password))
				return false;

			X2chAuthenticator test = new X2chAuthenticator();
			test.Connect(username, password);
			return test.IsValidSessionId(test.SessionId);
		}

		private bool IsValidSessionId(string sessionId)
		{
			bool result = false;
			if (sessionId != null && sessionId.Length > 0)
			{
				string[] ids = sessionId.Split(new char[]{':'});
				if (ids != null && ids.Length > 1)
				{
					result = !ids[0].ToLower().Equals("error");
				}
			}
			return result;
		}
		#endregion
	}
}
