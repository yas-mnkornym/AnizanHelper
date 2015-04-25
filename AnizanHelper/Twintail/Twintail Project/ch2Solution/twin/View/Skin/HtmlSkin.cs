// HtmlSkin.cs

namespace Twin
{
	using System;
	using System.IO;
	using System.Text;
	using System.Text.RegularExpressions;
	using System.Collections;
	using System.Diagnostics;
	using Twin.Util;
	using Twin.Text;

	/// <summary>
	/// 2chのhtml形式の変換処理を行う
	/// </summary>
	public class HtmlSkin : ThreadSkinBase
	{
		protected string headerSkin;
		protected string footerSkin;
		protected string resSkin;
		private string skinPath;

		private string baseUri;

		/// <summary>
		/// レス参照の基本となるURLを取得または設定
		/// </summary>
		public override string BaseUri {
			set {
				if (value == null)
					throw new ArgumentNullException("BaseUri");

				baseUri = value;

				if (!baseUri.EndsWith("/"))
					baseUri += "/";
			}
			get { return baseUri; }
		}

		// スキン名を取得
		public override string Name {
			get { return "htmlSkin"; }
		}

		/// <summary>
		/// HtmlSkinクラスのインスタンスを初期化
		/// </summary>
		public HtmlSkin()
		{
			// 
			// TODO: コンストラクタ ロジックをここに追加してください。
			//
			headerSkin = "<html><head>" +
						"<meta http-equiv=\"Content-Type\" content=\"text/html; charset=Shift_JIS\">" +
						"<title><THREADNAME/></title>" +
						"</head>" +
						"<body bgcolor=#efefef text=black link=blue alink=red vlink=#660099>" +
						"<p><font size=+1 color=red><THREADNAME/></font>" +
						"<dl>";

			resSkin = "<dt><a name=\"<PLAINNUMBER/>\"><PLAINNUMBER/></a> <MAILNAME/> ：<DATE/></dt><dd><MESSAGE/><br><br></dd>";
			footerSkin = "</dl></body></html>";
			baseUri = "#";
			skinPath = "";
		}

		/// <summary>
		/// スキンを読み込む
		/// </summary>
		/// <param name="skinFolder"></param>
		public override void Load(string skinFolder)
		{
			headerSkin = FileUtility.ReadToEnd(
				Path.Combine(skinFolder, "Header.html"));

			footerSkin = FileUtility.ReadToEnd(
				Path.Combine(skinFolder, "Footer.html"));

			resSkin = FileUtility.ReadToEnd(
				Path.Combine(skinFolder, "Res.html"));

			this.skinPath = skinFolder;

			// 最後がスラッシュ記号で終わっていなければ付加する
			if (!skinPath.EndsWith("\\"))
				skinPath += "\\";
		}

		public override void Reset()
		{
		}

		/// <summary>
		/// ヘッダーとフッター共通の置き換え関数
		/// </summary>
		/// <param name="skinhtml"></param>
		/// <param name="obj"></param>
		/// <returns></returns>
		protected virtual string ReplaceHeaderFooter(string skinhtml, ThreadHeader header)
		{
			ThreadHeader h = header;
			BoardInfo b = h.BoardInfo;
			string result = skinhtml;

			StringBuilder sb = new StringBuilder(512);
			sb.Append(result);
			sb.Replace("<BOARDNAME/>", b.Name);
			sb.Replace("<BOARDURL/>", b.Url);
			sb.Replace("<THREADNAME/>", h.Subject);
			sb.Replace("<THREADURL/>", h.Url);
			sb.Replace("<ALLRESCOUNT/>", h.ResCount.ToString());
			sb.Replace("<NEWRESCOUNT/>", h.NewResCount.ToString());
			sb.Replace("<GETRESCOUNT/>", (h.GotByteCount - h.NewResCount).ToString());
			sb.Replace("<SKINPATH/>", skinPath);
			sb.Replace("<LASTMODIFIED/>", h.LastModified.ToString());
			sb.Replace("<SIZEKB/>", (h.GotByteCount / 1024).ToString());
			sb.Replace("<SIZE/>", h.GotByteCount.ToString());
			result = sb.ToString();

			return result;
		}

		/// <summary>
		/// ヘッダースキンを取得
		/// </summary>
		/// <param name="header"></param>
		/// <returns></returns>
		public override string GetHeader(ThreadHeader header)
		{
			return ReplaceHeaderFooter(headerSkin, header);
		}

		/// <summary>
		/// フッタースキンを取得
		/// </summary>
		/// <param name="header"></param>
		/// <returns></returns>
		public override string GetFooter(ThreadHeader header)
		{
			return ReplaceHeaderFooter(footerSkin, header);
		}

		/// <summary>
		/// 指定したResSetを
		/// 設定されているスキンを使用して文字列形式に変換
		/// </summary>
		/// <param name="resSet"></param>
		/// <returns></returns>
		protected virtual string Convert(string skinhtml, ResSet resSet)
		{
			StringBuilder sb = new StringBuilder(2048);
			string name;
			string mailname;
			string dateonly, dateString;
			string body;

			#region 名前の作成
			sb.Append("<b>");
			sb.Append(resSet.Name);
			sb.Append("</b>");
			name = sb.ToString();
			sb.Remove(0, sb.Length);
			#endregion

			#region Email付き名前の作成
			if (resSet.Email != String.Empty)
			{
				sb.Append("<a href=\"mailto:");
				sb.Append(resSet.Email);
				sb.Append("\">");
				sb.Append(name);
				sb.Append("</a>");
				mailname = sb.ToString();
				sb.Remove(0, sb.Length);
			}
			else
			{
				mailname = name;
			}
			#endregion

			#region 日付とIDを作成
			dateString = resSet.DateString;
			dateonly = resSet.DateString;
			Match m = Regex.Match(resSet.DateString, "( ID:)|(\\[)");

			if (m.Success)
			{
				dateonly = resSet.DateString.Substring(0, m.Index);
			}
			#endregion

			#region Be2chIDのリンクを貼る
			// BE:0123456-# または <BE:0123456:0> 形式の二つあるみたい
			dateString =
				Regex.Replace(dateString, @"BE:(?<id>\d+)\-(?<rank>.+)",
				"<a href=\"http://be.2ch.net/test/p.php?i=${id}\" target=\"_blank\">?${rank}</a>", RegexOptions.IgnoreCase);

			dateString =// 面白ネタnews形式
				Regex.Replace(dateString, @"<BE:(?<id>\d+):(?<rank>.+)>",
				"<a href=\"http://be.2ch.net/test/p.php?i=${id}\" target=\"_blank\">Lv.${rank}</a>", RegexOptions.IgnoreCase);
			#endregion

			#region 本分を作成
			body = HtmlTextUtility.RemoveTag(resSet.Body, "a");
			body = HtmlTextUtility.Linking(body);
			#endregion

			#region レス参照を作成
			body = HtmlTextUtility.RefRegex.Replace(body, "<a href=\"" + baseUri + "${num}\" target=\"_blank\">${ref}</a>");
			body = HtmlTextUtility.ExRefRegex.Replace(body, "<a href=\"" + baseUri + "${num}\" target=\"_blank\">${num}</a>");
			#endregion

			sb.Remove(0, sb.Length);
			sb.Append(skinhtml);
			sb.Replace("<PLAINNUMBER/>", resSet.Index.ToString());
			sb.Replace("<NUMBER/>", resSet.Index.ToString());
			sb.Replace("<MAILNAME/>", mailname);
			sb.Replace("<ID/>", resSet.ID);
			sb.Replace("<BE/>", resSet.BeLink);
			sb.Replace("<NAME/>", name);
			sb.Replace("<MAIL/>", resSet.Email);
			sb.Replace("<DATE/>", dateString);
			sb.Replace("<DATEONLY/>", dateonly);
			sb.Replace("<MESSAGE/>", body);
			sb.Replace("<SKINPATH/>", skinPath);

			skinhtml = sb.ToString();

			return skinhtml;
		}

		/// <summary>
		/// 指定したResSetを
		/// 設定されているスキンを使用して文字列形式に変換
		/// </summary>
		/// <param name="resSet"></param>
		/// <returns></returns>
		public override string Convert(ResSet resSet)
		{
			return Convert(resSkin, resSet);
		}

		/// <summary>
		/// 指定したResSetコレクションを
		/// 設定されているスキンを使用して文字列形式に変換
		/// </summary>
		/// <param name="resSetCollection"></param>
		/// <returns></returns>
		public override string Convert(ResSetCollection resSetCollection)
		{
			if (resSetCollection == null) {
				throw new ArgumentNullException("resSetCollection");
			}

			// 指定したレス分の初期配列を割り当てる
			StringBuilder sb = new StringBuilder(512 * resSetCollection.Count);

			foreach (ResSet resSet in resSetCollection)
			{
				if (resSet.Visible)
				{
					string result = Convert(resSet);
					sb.Append(result);
				}
			}

			return sb.ToString();
		}
	}
}
