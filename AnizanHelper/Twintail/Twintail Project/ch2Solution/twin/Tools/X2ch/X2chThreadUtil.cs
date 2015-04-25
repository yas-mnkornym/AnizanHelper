// X2chThreadUtil.cs

namespace Twin
{
	using System;
	using System.Net;
	using System.Text;
	using Twin.Bbs;
	using Twin.Text;
	using Twin.Util;
	using System.IO;

	/// <summary>
	/// X2chThreadUtil の概要の説明です。
	/// </summary>
	public class X2chThreadUtil
	{
		public X2chThreadUtil()
		{
			// 
			// TODO: コンストラクタ ロジックをここに追加してください。
			//
		}

		public static string GetResponseHtml(ThreadHeader header)
		{
			HttpWebRequest req = (HttpWebRequest)WebRequest.Create(header.Url);
			req.UserAgent = String.Empty;

			HttpWebResponse res = (HttpWebResponse)req.GetResponse();
			try
			{

				using (StreamReader r = new StreamReader(res.GetResponseStream(), Encoding.GetEncoding("shift_jis")))
				{
					return r.ReadToEnd();
				}
			}
			finally
			{
				res.Close();
			}
		}

		/// <summary>
		/// 過去ログが存在するかどうかを調べてみる
		/// </summary>
		/// <param name="header"></param>
		/// <returns></returns>
		public static bool KakologIsExist(ThreadHeader header, out bool gzipCompress)
		{
			X2chKakoThreadHeader kako = new X2chKakoThreadHeader();
			header.CopyTo(kako);

			// まず.dat.gzを取得してみて、だめなら.datヲ取得する。
			// それでもだめなら諦める。
			kako.GzipCompress = true;
			bool retried = false;
Retry:
			HttpWebRequest req = (HttpWebRequest)WebRequest.Create(kako.DatUrl);
			req.UserAgent = TwinDll.UserAgent;
			req.Headers.Add("Accept-Encoding", "gzip");
			req.Method = "HEAD";
			req.AllowAutoRedirect = false;

			HttpWebResponse res = (HttpWebResponse)req.GetResponse();
			res.Close();

			if (res.StatusCode == HttpStatusCode.OK)
			{
				gzipCompress = kako.GzipCompress;
				return true;
			}
			else if (!retried)
			{
				kako.GzipCompress = false;
				retried = true;
				goto Retry;
			}

			gzipCompress = false;

			return false;
		}

		/*
		/// <summary>
		/// 指定したスレッドの状態を確認
		/// </summary>
		/// <param name="header"></param>
		/// <returns></returns>
		public static ThreadState CheckState(ThreadHeader header)
		{
			if (header == null)
				throw new ArgumentNullException("header");

			HttpWebRequest req = (HttpWebRequest)WebRequest.Create(header.Url);
			req.UserAgent = TwinDll.IEUserAgent;
			HttpWebResponse res = (HttpWebResponse)req.GetResponse();

			byte[] data = FileUtility.ReadBytes(res.GetResponseStream());
			string html = Encoding.GetEncoding("Shift_Jis").GetString(data);

			if (html.IndexOf("そんな板orスレッドないです") >= 0)
			{
				if (html.IndexOf("隊長! 過去ログ倉庫で") >= 0)
					return ThreadState.Kakolog;

				if (html.IndexOf("過去ログ倉庫にもありませんでした") >= 0)
					return ThreadState.NotExists;
			}
			else if (html.IndexOf("このスレッドは過去ログ倉庫に格納されています") >= 0 ||
					html.IndexOf("もうずっと人大杉") >= 0)
			{
				return ThreadState.Pastlog;
			}

			return ThreadState.None;
		}*/

		/// <summary>
		/// 指定したスレッドのresStartからresEndまでの範囲を取得。
		/// 既得ログに存在すればローカルから読み込む。
		/// </summary>
		/// <param name="cache"></param>
		/// <param name="header"></param>
		/// <param name="resStart"></param>
		/// <param name="resEnd"></param>
		/// <returns></returns>
		public static ResSetCollection GetRange(Cache cache, ThreadHeader header, int resStart, int resEnd)
		{
			if (cache == null)
				throw new ArgumentNullException("cache");

			if (header == null)
				throw new ArgumentNullException("header");

			if (resStart > resEnd)
				throw new ArgumentException("resStartはresEnd以下にしてください", "resStart");

			string address = header.Url + ((resStart == resEnd) ? 
			resStart.ToString() : String.Format("{0}-{1}", resStart, resEnd));

			// サーバーからデータをダウンロード
			WebClient webClient = new WebClient();
			webClient.Headers.Add("User-Agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT; DigExt)");
			
			byte[] data = webClient.DownloadData(address);
			int byteParsed;

			// ResSet[]型に解析
			ThreadParser parser = new X2chHtmlThreadParser(BbsType.X2ch, Encoding.GetEncoding("Shift_Jis"));
			ResSet[] array = parser.Parse(data, data.Length, out byteParsed);

			ResSetCollection items = new ResSetCollection();
			items.AddRange(array);

			return items;
		}
	}

	/// <summary>
	/// スレッドの状態を表す列挙体
	/// </summary>
	public enum ThreadState
	{
		/// <summary>
		/// 指定なし
		/// </summary>
		None,
		/// <summary>
		/// スレッドは存在する
		/// </summary>
		Exists,
		/// <summary>
		/// スレッドが見つからなかった
		/// </summary>
		NotExists,
		/// <summary>
		/// スレッドは過去ログに格納されている
		/// </summary>
		Kakolog,
		/// <summary>
		/// スレッドはdat落ちしている
		/// </summary>
		Pastlog,
	}
}
