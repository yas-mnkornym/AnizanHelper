// X2chServerSetting.cs

namespace Twin.Tools
{
	using System;
	using System.Text;
	using System.Text.RegularExpressions;
	using System.Collections.Generic;
	using System.Drawing;
	using System.Net;
	using System.IO;

	/// <summary>
	/// 2chのサーバー設定情報を管理
	/// </summary>
	public class X2chServerSetting
	{
		private static readonly Regex Pattern =
			new Regex(@"(?<key>\w+)=(?<value>.+)", RegexOptions.Compiled);

		private BoardInfo board;
		private Dictionary<string, string> setting = new Dictionary<string, string>();

		private bool loaded = false;
		public bool IsLoaded
		{
			get
			{
				return loaded;
			}
		}

		/// <summary>
		/// 板情報を取得
		/// </summary>
		public BoardInfo Board
		{
			get
			{
				return board;
			}
		}

		private string textData = String.Empty;
		/// <summary>
		/// 
		/// </summary>
		public string TextData
		{
			get
			{
				return textData;
			}
		}
	

		/// <summary>
		/// 指定したキーの情報を取得
		/// </summary>
		public string this[string key]
		{
			get
			{
				return GetString(key);
			}
		}

		public string Title
		{
			get
			{
				return this["BBS_TITLE"];
			}
		}

		public Color TitleColor
		{
			get
			{
				return GetColor("BBS_TITLE_COLOR");
			}
		}

		public Color BGColor
		{
			get
			{
				return GetColor("BBS_BG_COLOR");
			}
		}

		public string TitleLink
		{
			get
			{
				return this["BBS_TITLE_LINK"];
			}
		}

		public string TitlePicture
		{
			get
			{
				return GetFullPath(this["BBS_TITLE_PICTURE"]);
			}
		}

		public string BGPicture
		{
			get
			{
				return GetFullPath(this["BBS_BG_PICTURE"]);
			}
		}

		public string NonameName
		{
			get
			{
				return this["BBS_NONAME_NAME"];
			}
		}

		public Color MakeThreadColor
		{
			get
			{
				return GetColor("BBS_MAKETHREAD_COLOR");
			}
		}

		public Color MenuColor
		{
			get
			{
				return GetColor("BBS_MENU_COLOR");
			}
		}

		public Color ThreadColor
		{
			get
			{
				return GetColor("BBS_THREAD_COLOR");
			}
		}

		public Color TextColor
		{
			get
			{
				return GetColor("BBS_TEXT_COLOR");
			}
		}

		public Color NameColor
		{
			get
			{
				return GetColor("BBS_NAME_COLOR");
			}
		}

		public Color LinkColor
		{
			get
			{
				return GetColor("BBS_LINK_COLOR");
			}
		}

		public Color ALinkColor
		{
			get
			{
				return GetColor("BBS_ALINK_COLOR");
			}
		}

		public Color VLinkColor
		{
			get
			{
				return GetColor("BBS_VLINK_COLOR");
			}
		}

		public int ThreadNumber
		{
			get
			{
				return GetInt("BBS_THREAD_NUMBER");
			}
		}

		public int ContentsNumber
		{
			get
			{
				return GetInt("BBS_CONTENTS_NUMBER");
			}
		}

		public int LineNumber
		{
			get
			{
				return GetInt("BBS_LINE_NUMBER");
			}
		}

		public int MaxMenuThread
		{
			get
			{
				return GetInt("BBS_MAX_MENU_THREAD");
			}
		}

		public Color SubjectColor
		{
			get
			{
				return GetColor("BBS_SUBJECT_COLOR");
			}
		}

		public string Unicode
		{
			get
			{
				return this["BBS_UNICODE"];
			}
		}

		public string DeleteName
		{
			get
			{
				return this["BBS_DELETE_NAME"];
			}
		}

		public bool NameCookieCheck
		{
			get
			{
				return GetBoolean("BBS_NAMECOOKIE_CHECK");
			}
		}

		public bool MailCookieCheck
		{
			get
			{
				return GetBoolean("BBS_MAILCOOKIE_CHECK");
			}
		}

		public int SubjectCount
		{
			get
			{
				return GetInt("BBS_SUBJECT_COUNT");
			}
		}

		public int NameCount
		{
			get
			{
				return GetInt("BBS_NAME_COUNT");
			}
		}

		public int MailCount
		{
			get
			{
				return GetInt("BBS_MAIL_COUNT");
			}
		}

		public int MessageCount
		{
			get
			{
				return GetInt("BBS_MESSAGE_COUNT");
			}
		}

		public bool ThreadTatesugi
		{
			get
			{
				return GetBoolean("BBS_THREAD_TATESUGI");
			}
		}

		public bool NanashiCheck
		{
			get
			{
				return GetBoolean("NANASHI_CHECK");
			}
		}

		public bool ProxyCheck
		{
			get
			{
				return GetBoolean("BBS_PROXY_CHECK");
			}
		}

		/// <summary>
		/// X2chServerSettingクラスのインスタンスを初期化
		/// </summary>
		public X2chServerSetting()
		{
			// 
			// TODO: コンストラクタ ロジックをここに追加してください。
			//
			board = null;
		}

		/// <summary>
		/// 指定した板の設定情報をダウンロード
		/// </summary>
		/// <param name="info"></param>
		public void Download(BoardInfo info)
		{
			if (info == null)
			{
				throw new ArgumentNullException("info");
			}

			// 設定情報の存在するURL
			string url = info.Url + "SETTING.TXT";

			HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
			req.UserAgent = TwinDll.UserAgent;
			req.Referer = info.Url;

			HttpWebResponse res = (HttpWebResponse)req.GetResponse();

			try
			{
				using (StreamReader sr = new StreamReader(res.GetResponseStream(), Encoding.GetEncoding("Shift_Jis")))
				{
					Read(info, sr.ReadToEnd());
				}
			}
			finally
			{
				if (res != null)
					res.Close();
			}
		}

		public void Read(BoardInfo bi, string settingText)
		{
			board = bi;
			setting.Clear();
			textData = settingText;
			loaded = false;

			using (StringReader sr = new StringReader(settingText))
			{
				string data;

				while ((data = sr.ReadLine()) != null)
				{
					Match m = Pattern.Match(data);
					if (m.Success)
					{
						string key = m.Groups["key"].Value;
						string value = m.Groups["value"].Value;
						setting[key] = value;						
					}
				}
			}

			if (setting.Count > 0)
				loaded = true;
		}


		public string GetString(string key)
		{
			if (ContainsKey(key))
				return setting[key];

			else
				return String.Empty;
		}

		public int GetInt(string key)
		{
			int result;

			if (Int32.TryParse(GetString(key), out result))
			{
				return result;
			}
			else
			{
				return - 1;
			}
		}

		public bool GetBoolean(string key)
		{
			bool result;

			if (Boolean.TryParse(GetString(key), out result))
			{
				return result;
			}
			else
				return false;
		}

		public Color GetColor(string key)
		{
			string val = GetString(key);

			if (val == String.Empty)
				return Color.Empty;

			return ColorTranslator.FromHtml(val);
		}

		public bool ContainsKey(string key)
		{
			return setting.ContainsKey(key);
		}

		public bool ContainsValue(string key)
		{
			return !String.IsNullOrEmpty(GetString(key));
		}


		/// <summary>
		/// 相対パスを絶対パスに変換する。
		/// </summary>
		/// <param name="url"></param>
		/// <returns></returns>
		private string GetFullPath(string url)
		{
			if (url.StartsWith("http://"))
				return url;

			if (url.StartsWith("/"))
				return "http://" + board.Server + url;

			return board.Url + url;
		}
	}
}
