// X2chThreadListReader.cs

namespace Twin.Bbs
{
	using System;
	using System.IO;
	using System.Text;
	using System.Text.RegularExpressions;
	using System.IO.Compression;
	using System.Net;
	using Twin.Tools;
	using Twin.Text;
	using Twin.IO;
	using Twin.Util;
	using System.Threading;

	/// <summary>
	/// ２ちゃんねる (www.2ch.net) のスレッド一覧を読み込む機能を提供
	/// </summary>
	public class X2chThreadListReader : ThreadListReaderBase
	{
		private HttpWebResponse _res = null;

		/// <summary>
		/// パーサを指定して、X2chThreadListReaderクラスのインスタンスを初期化
		/// </summary>
		/// <param name="dataParser">データの解析に使用するパーサー</param>
		public X2chThreadListReader(ThreadListParser dataParser)
			: base(dataParser)
		{
		}

		/// <summary>
		/// X2chThreadListReaderクラスのインスタンスを初期化
		/// </summary>
		public X2chThreadListReader()
			: base(new X2chThreadListParser())
		{
		}

		/// <summary>
		/// X2chThreadListReaderクラスのインスタンスを初期化
		/// </summary>
		/// <param name="board"></param>
		public X2chThreadListReader(BoardInfo board)
			: this()
		{
			// 
			// TODO: コンストラクタ ロジックをここに追加してください。
			//
			Open(board);
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

Redirect:
			// 板の移転チェック
			bool trace = false;

			HttpWebRequest req = (HttpWebRequest)WebRequest.Create(info.Url + "subject.txt");
			req.Timeout = 30000;
			req.UserAgent = UserAgent;
			req.AllowAutoRedirect = false;
			req.Headers.Add("Accept-Encoding", "gzip");

			req.Headers.Add("Pragma", "no-cache");
			req.Headers.Add("Cache-Control", "no-cache");
			
			_res = (HttpWebResponse)req.GetResponse();
			
			//IAsyncResult ar = req.BeginGetResponse(null, null);
			//if (!ar.IsCompleted)
			//{
			//    if (canceled)
			//    {
			//        req.Abort();
			//        return false;
			//    }
			//    Thread.Sleep(100);
			//}

			//_res = (HttpWebResponse)req.EndGetResponse(ar);
			
			baseStream = _res.GetResponseStream();

			boardinfo = info;

			// Subject.txtが取得できてContent-Lengthが0、
			// またはStatusCodeがFoundの場合は板が移転したかも
			if (_res.StatusCode == HttpStatusCode.OK)
			{
				position = 0;
				length = (int)_res.ContentLength;
				isOpen = true;

				if (_res.ContentEncoding.EndsWith("gzip"))
				{
					using (GZipStream gzipInp = new GZipStream(baseStream, CompressionMode.Decompress))
						baseStream = FileUtility.CreateMemoryStream(gzipInp);

					length = (int)baseStream.Length;
				}

				if (length == 0)
					trace = true;
			}
			else
			{
				if (_res.StatusCode == HttpStatusCode.Found)
				{
					trace = true;
				}
				_res.Close();
				_res = null;
			}

			if (trace)
			{
				// 板移転の可能性
				X2chServerTracer tracer = new X2chServerTracer();
				if (tracer.Trace(boardinfo, true))
				{
					BoardInfo newbrd = tracer.Result;
					OnServerChange(new ServerChangeEventArgs(boardinfo, newbrd, tracer.TraceList));

					if (AutoRedirect)
					{
						info = newbrd;

						if (_res != null)
							_res.Close();

						goto Redirect;
					}
				}
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
