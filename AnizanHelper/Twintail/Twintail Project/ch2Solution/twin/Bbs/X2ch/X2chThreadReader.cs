// X2chThreadReader.cs

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
	using Twin.Tools;
	using System.Threading;

	/// <summary>
	/// ２ちゃんねる (www.2ch.net) のスレッドを読み込む機能を提供
	/// </summary>
	public class X2chThreadReader : ThreadReaderBase
	{
		protected ThreadHeader headerInfo;
		protected bool aboneCheck;
		protected bool getGzip;

		private HttpWebResponse _res = null;

		/// <summary>
		/// 指定したパーサを使用してX2chThreadReaderクラスのインスタンスを初期化。
		/// </summary>
		/// <param name="dataParser"></param>
		public X2chThreadReader(ThreadParser dataParser)
			: base(dataParser)
		{
			getGzip = true;
		}

		/// <summary>
		/// X2chThreadReaderクラスのインスタンスを初期化
		/// </summary>
		public X2chThreadReader()
			: this(new X2chThreadParser())
		{
		}

		/// <summary>
		/// X2chThreadReaderクラスのインスタンスを初期化と同時に、
		/// 指定したスレッドを開く。
		/// </summary>
		/// <param name="header">初期化と同時に開くスレッドの情報</param>
		public X2chThreadReader(ThreadHeader header)
			: this()
		{
			// 
			// TODO: コンストラクタ ロジックをここに追加してください。
			//
			Open(header);
		}

		/// <summary>
		/// スレッドを開く
		/// </summary>
		/// <param name="th"></param>
		public override bool Open(ThreadHeader header)
		{
			if (header == null)
			{
				throw new ArgumentNullException("header");
			}
			if (IsOpen)
			{
				throw new InvalidOperationException("既にストリームが開かれています");
			}

			// 差分取得かどうか
			aboneCheck = (header.GotByteCount > 0) ? true : false;
//		Retry:
			_res = null;

			try
			{

				HttpWebRequest req = (HttpWebRequest)WebRequest.Create(header.DatUrl);
				req.Timeout = 60000;
				req.AllowAutoRedirect = false;
				req.UserAgent = UserAgent;

				//req.Headers.Add("Pragma", "no-cache");
				//req.Headers.Add("Cache-Control", "no-cache");

				if (!String.IsNullOrEmpty(header.ETag))
					req.Headers.Add("If-None-Match", header.ETag);

				if (header.GotByteCount > 0)
					req.AddRange(header.GotByteCount - 1);

				//if (!aboneCheck && getGzip)
				//	req.Headers.Add("Accept-Encoding", "gzip");

				_res = (HttpWebResponse)req.GetResponse();

				baseStream = _res.GetResponseStream();
				headerInfo = header;

				bool encGzip = _res.ContentEncoding.EndsWith("gzip");

				if (encGzip)
				{

					using (GZipStream gzipInp = new GZipStream(baseStream, CompressionMode.Decompress))
						baseStream = FileUtility.CreateMemoryStream(gzipInp);

					length = (int)baseStream.Length;
				}
				else
				{
					length = aboneCheck ?
						(int)_res.ContentLength - 1 : (int)_res.ContentLength;
				}

				// OK
				if (_res.StatusCode == HttpStatusCode.OK ||
					_res.StatusCode == HttpStatusCode.PartialContent)
				{
					headerInfo.LastModified = _res.LastModified;
					headerInfo.ETag = _res.Headers["ETag"];

					index = header.GotResCount + 1;
					position = 0;
					isOpen = true;
				}
				// dat落ちした予感
				else
				{
					_res.Close();

					if (_res.StatusCode == HttpStatusCode.Found)
					{
						// 10/05 移転追尾をすると過去ログ倉庫が読めなくなってしまう事がわかったので、一時的に外してみる
						//if (IsServerChanged(headerInfo))
						//{
						//    // サーバーが移転したら新しい板情報でリトライ。
						//    goto Retry;
						//}
						//else
						{
							// そうでなければ dat落ちとして判断
							//0324 headerInfo.Pastlog = true;

							PastlogEventArgs argument = new PastlogEventArgs(headerInfo);
							OnPastlog(argument);
						}
					}

					_res = null;
				}
			}
			catch (WebException ex)
			{
				HttpWebResponse res = (HttpWebResponse)ex.Response;

				// あぼーんの予感
				if (res != null &&
					res.StatusCode == HttpStatusCode.RequestedRangeNotSatisfiable)
				{
					OnABone();
				}
				else
				{
					throw ex;
				}
			}

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
			if (resSets == null)
			{
				throw new ArgumentNullException("resSets");
			}
			if (!isOpen)
			{
				throw new InvalidOperationException("ストリームが開かれていません");
			}

			// バッファにデータを読み込む
			int byteCount = baseStream.Read(buffer, 0, buffer.Length);

			// あぼーんチェック
			if (aboneCheck && byteCount > 0)
			{
				if (buffer[0] != '\n')
				{
					OnABone();
					byteParsed = 0;

					headerInfo.ETag = String.Empty;
					headerInfo.LastModified = DateTime.MinValue;

					return -1;
				}

				buffer = RemoveHeader(buffer, byteCount, 1);
				byteCount -= 1;
				aboneCheck = false;
			}

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

		/// <summary>
		/// bufferの先頭文字を取り除く
		/// </summary>
		/// <param name="buffer"></param>
		/// <param name="count"></param>
		/// <returns></returns>
		private static byte[] RemoveHeader(byte[] buffer, int length, int count)
		{
			byte[] result = new byte[length - count];
			Array.Copy(buffer, count, result, 0, result.Length);

			return result;
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



		private bool IsServerChanged(ThreadHeader h)
		{
			X2chServerTracer server = new X2chServerTracer();

			if (server.Trace(h.BoardInfo, true))
			{
				h.BoardInfo = server.Result;
				return true;
			}


			return false;
		}
	}

}
