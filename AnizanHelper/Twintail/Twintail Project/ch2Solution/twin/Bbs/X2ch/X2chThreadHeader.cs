// X2chThreadHeader.cs

namespace Twin.Bbs
{
	using System;
	using System.Text;
	using System.Web;

	/// <summary>
	/// ２ちゃんねるのスレッドヘッダ情報を表す
	/// </summary>
	public class X2chThreadHeader : ThreadHeader
	{
		/// <summary>
		/// datファイルの存在するURLを取得
		/// </summary>
		public override string DatUrl {
			get {
				return String.Format("http://{0}/{1}/dat/{2}.dat",
					BoardInfo.Server, BoardInfo.Path, Key);
			}
		}

		/// <summary>
		/// スレッドのURLを取得
		/// </summary>
		public override string Url {
			get {
				return String.Format("http://{0}/test/read.cgi/{1}/{2}/",
					BoardInfo.Server, BoardInfo.Path, Key);
			}
		}

		/// <summary>
		/// 認証使用時のスレッドのURLを取得
		/// </summary>
		public string AuthenticateUrl
		{
			get
			{
				// ** 水玉さんによる提供 **
				// 2013/09/10 ●dat取得仕様変更
				// http://qb5.2ch.net/test/read.cgi/operate/1366640919/87-88 
				// http://stream.bbspink.com/update.txt 
				//　　http://rokka.<SITENAME>.<COM or NET>/<SERVER NAME>/<BOARD NAME>/<DAT NUMBER>/<OPTIONS>?raw=0.0&sid=<SID>
				//return AddSessionId( String.Format( "http://{0}/test/offlaw.cgi/{1}/{2}/" ,
				//　　BoardInfo.Server , BoardInfo.Path , Key ) );
				var site = BoardInfo.DomainPath.Substring(0, BoardInfo.DomainPath.IndexOf("/"));
				return AddSessionId(string.Format("http://rokka.{3}/{0}/{1}/{2}/",
				  BoardInfo.ServerName, BoardInfo.Path, Key, site));
			}
		}

		private string AddSessionId(string url)
		{
			StringBuilder urlOutput = new StringBuilder();
			urlOutput.Append(url);

			X2chAuthenticator authenticator = X2chAuthenticator.GetInstance();
			if (authenticator.HasSession)
			{
				urlOutput.AppendFormat("?raw=0.0&sid={0}", HttpUtility.UrlEncode(authenticator.SessionId, Encoding.GetEncoding("shift_jis")));
			}
			return urlOutput.ToString();
		}

		/// <summary>
		/// 書き込み可能な最大レス数を取得
		/// </summary>
		public override int UpperLimitResCount {
			get {
				return 1000;
			}
		}

		/// <summary>
		/// X2chThreadHeaderクラスのインスタンスを初期化
		/// </summary>
		public X2chThreadHeader()
		{
			// 
			// TODO: コンストラクタ ロジックをここに追加してください。
			//
		}

		public X2chThreadHeader(X2chThreadHeader source)
			: base(source.BoardInfo, source.Key, source.Subject)
		{
			// 
			// TODO: コンストラクタ ロジックをここに追加してください。
			//
		}
	}
}
