// X2chKakoThreadReader.cs

namespace Twin.Bbs
{
	using System;
	using System.IO;
	using System.Text;
	using System.Collections;
	using System.Net;
	using System.Diagnostics;
	using System.IO.Compression;
	using Twin.Text;
	using Twin.Util;
	using Twin.IO;

	/// <summary>
	/// ２ちゃんの過去ログを読み込む
	/// </summary>
	public class X2chKakoThreadReader : X2chThreadReader
	{
		private HttpWebResponse _res = null;
		private BoardInfo[] retryServers = null;
		private int retryCount = 0;

		/// <summary>
		/// 過去ログを取得できなかった場合、再試行を行うサーバを取得または設定
		/// </summary>
		public BoardInfo[] RetryServers {
			set {
				retryServers = value;
			}
			get {
				return retryServers;
			}
		}

		/// <summary>
		/// X2chThreadReaderクラスのインスタンスを初期化
		/// </summary>
		public X2chKakoThreadReader() : base()
		{
		}

		/// <summary>
		/// スレッドを開く
		/// </summary>
		/// <param name="th"></param>
		public override bool Open(ThreadHeader header)
		{
			if (header == null) {
				throw new ArgumentNullException("header");
			}
			if (IsOpen) {
				throw new InvalidOperationException("既にストリームが開かれています");
			}

			X2chKakoThreadHeader kakoheader = header as X2chKakoThreadHeader;
			bool retried = false;

			if (kakoheader != null)
				kakoheader.GzipCompress = true;

Retry:
			// ネットワークストリームを初期化
			HttpWebRequest req = (HttpWebRequest)WebRequest.Create(header.DatUrl);
			req.Timeout = 30000;
			req.AllowAutoRedirect = false;
			req.Referer = header.Url;
			req.UserAgent = UserAgent;
			req.Headers.Add("Accept-Encoding", "gzip");

			req.Headers.Add("Pragma", "no-cache");
			req.Headers.Add("Cache-Control", "no-cache");
			
			if (header.GotByteCount > 0)
				req.AddRange(header.GotByteCount-1);

			if (header.ETag != String.Empty)
				req.Headers.Add("If-None-Match", header.ETag);

			_res = (HttpWebResponse)req.GetResponse();
			baseStream = _res.GetResponseStream();
			headerInfo = header;

			// OK
			if (_res.StatusCode == HttpStatusCode.OK ||
				_res.StatusCode == HttpStatusCode.PartialContent)
			{
				bool encGzip = _res.ContentEncoding.EndsWith("gzip");

				// Gzipを使用する場合はすべて読み込む
				if (encGzip)
				{
					using (GZipStream gzipInp = new GZipStream(_res.GetResponseStream(), CompressionMode.Decompress))
						baseStream = FileUtility.CreateMemoryStream(gzipInp);

					baseStream.Position = 0;
					length = (int)baseStream.Length;
				}
				else {
					length = aboneCheck ?
						(int)_res.ContentLength - 1 : (int)_res.ContentLength;
				}

				headerInfo.LastModified = _res.LastModified;
				headerInfo.ETag = _res.Headers["ETag"];

				index = header.GotResCount + 1;
				position = 0;
				isOpen = true;
			}
			else if (!retried)
			{
				_res.Close();
				_res = null;

				if (kakoheader != null)
					kakoheader.GzipCompress = !kakoheader.GzipCompress;
				retried = true;
				goto Retry;
			}
			else if (_res.StatusCode == HttpStatusCode.Found)
			{
				if (retryServers != null && retryCount < retryServers.Length)
				{
					BoardInfo retryBoard = retryServers[retryCount++];
					_res.Close();
					_res = null;
				
					if (retryBoard != null)
						throw new X2chRetryKakologException(retryBoard);
				}
			}

			// 過去ログなのでdat落ちに設定
			//0324 headerInfo.Pastlog = true;

			retryCount = 0;

			return isOpen;
		}

		/// <summary>
		/// レスを読み込む
		/// </summary>
		/// <param name="resSets"></param>
		/// <param name="byteParsed"></param>
		/// <returns></returns>
		public override int Read(ResSetCollection resSets)
		{
			int temp;
			return Read(resSets, out temp);
		}

		/// <summary>
		/// レスを読み込む
		/// </summary>
		/// <param name="resSets"></param>
		/// <param name="byteParsed"></param>
		/// <returns></returns>
		public override int Read(ResSetCollection resSets, out int byteParsed)
		{
			if (resSets == null) {
				throw new ArgumentNullException("resSets");
			}
			if (!isOpen) {
				throw new InvalidOperationException("ストリームが開かれていません");
			}

			// バッファにデータを読み込む
			int byteCount = baseStream.Read(buffer, 0, buffer.Length);

			// 解析してコレクションに格納
			ICollection collect = dataParser.Parse(buffer, byteCount, out byteParsed);

			foreach (ResSet resSet in collect)
			{
				ResSet res = resSet;
				res.Index = index++;
				resSets.Add(res);

				if (res.Index == 1 && res.Tag != null)
					headerInfo.Subject = (string)res.Tag;
			}

			// 実際に読み込まれたバイト数を計算
			position += byteCount;

			return byteCount;
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
