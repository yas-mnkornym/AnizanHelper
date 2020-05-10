// MachiThreadListReader.cs

namespace Twin.Bbs
{
	using System;
	using System.Net;
	using System.Text;
	using Twin.Text;
	using Twin.IO;

	/// <summary>
	/// まちBBS (www.machi.to) のスレッド一覧を読み込む機能を提供
	/// </summary>
	public class MachiThreadListReader : ThreadListReaderBase
	{
		private HttpWebResponse _res = null;

		/// <summary>
		/// パーサを指定してMachiThreadListReaderクラスのインスタンスを初期化
		/// </summary>
		public MachiThreadListReader(ThreadListParser dataParser) 
			: base(dataParser)
		{
		}

		/// <summary>
		/// MachiThreadListReaderクラスのインスタンスを初期化
		/// </summary>
		public MachiThreadListReader() : this(new MachiThreadListParser())
		{
		}

		/// <summary>
		/// 板を開く
		/// </summary>
		/// <param name="info"></param>
		public override bool Open(BoardInfo info)
		{
			if (info == null) {
				throw new ArgumentNullException("info");
			}
			if (isOpen) {
				throw new InvalidOperationException("既にストリームが開かれています");
			}

			// ネットワークストリームを初期化
			HttpWebRequest req = (HttpWebRequest)WebRequest.Create(info.Url + "subject.txt");
			req.Timeout = 30000;
			req.UserAgent = UserAgent;
			req.Referer = info.Url;

			req.Headers.Add("Pragma", "no-cache");
			req.Headers.Add("Cache-Control", "no-cache");
			
			_res = (HttpWebResponse)req.GetResponse();
			baseStream = _res.GetResponseStream();

			if (_res.StatusCode == HttpStatusCode.OK)
			{
				position = 0;
				length = (int)_res.ContentLength;

				boardinfo = info;
				isOpen = true;
			}
			else {
				_res.Close();
				_res = null;
			}

			return isOpen;
		}

		public override void Close()
		{
			base.Close();
			if (_res != null)
			{
				_res.Close();
				_res = null;
			}
		}
	}
}
