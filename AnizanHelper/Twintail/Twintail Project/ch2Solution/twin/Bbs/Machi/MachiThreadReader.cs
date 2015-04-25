// MachiThreadReader.cs

namespace Twin.Bbs
{
	using System;
	using System.IO;
	using System.Text;
	using System.Collections;
	using System.Net;
	using System.Diagnostics;
	using Twin.Text;
	using Twin.Util;
	using Twin.IO;

	/// <summary>
	/// まちBBS (www.machi.to) のスレッドを読み込む機能を提供
	/// </summary>
	public class MachiThreadReader : ThreadReaderBase
	{
		protected ThreadHeader headerInfo;
		private HttpWebResponse _res = null;
		private int prevIndex = -1;

		/// <summary>
		/// パーサを指定してMachiThreadReaderクラスのインスタンスを初期化
		/// </summary>
		public MachiThreadReader(ThreadParser dataParser)
			: base(dataParser)
		{
		}

		/// <summary>
		/// MachiThreadReaderクラスのインスタンスを初期化
		/// </summary>
		public MachiThreadReader()
			: base(new MachiThreadParser())
		{
		}

		/// <summary>
		/// MachiThreadReaderクラスのインスタンスを初期化
		/// </summary>
		/// <param name="header"></param>
		public MachiThreadReader(ThreadHeader header) : this()
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
			if (header == null) {
				throw new ArgumentNullException("header");
			}
			if (IsOpen) {
				throw new InvalidOperationException("既にストリームが開かれています");
			}

			string url = header.Url;

			if (header.GotByteCount > 0)
			{
				// 差分取得用にURLに修正を加える
				url += String.Format("&START={0}&NOFIRST=TRUE", header.GotResCount + 1);
			}

			// ネットワークストリームを初期化
			HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
			req.Timeout = 15000;
			req.IfModifiedSince = header.LastModified;
			req.Referer = url;
			req.UserAgent = UserAgent;

			req.Headers.Add("Pragma", "no-cache");
			req.Headers.Add("Cache-Control", "no-cache");

			_res = (HttpWebResponse)req.GetResponse();
			baseStream = _res.GetResponseStream();
			headerInfo = header;

			// OK
			if (_res.StatusCode == HttpStatusCode.OK)
			{
				position = 0;
				length = (int)_res.ContentLength;
				index = header.GotResCount + 1;

				// 開始インデックスを設定
				((MachiThreadParser)dataParser).StartIndex = index;

				headerInfo.LastModified = _res.LastModified;
				isOpen = true;
			}
			// dat落ちした予感
			else {
				headerInfo.Pastlog = true;
				_res.Close();
				_res = null;
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
				if (res.Index <= 0)
					res.Index = index++;

				// JBBSあぼーん対策
				//int aboneCount = (res.Index - prevIndex) - 1; // 直前のレス番号と比較し、飛んでいるレス数を求める
				//if (prevIndex != -1 && aboneCount > 1)
				//{
				//    // まちBBS、JBBSではレスをdat自体から削除するあぼーんがあるため、
				//    // 解析の際にレス番号が飛んでしまいレス番号が狂ってしまう。
				//    // なのでダミーのあぼーんレスを挿入しておく
				//    for (int i = 0; i < aboneCount; i++)
				//        resSets.Add(ResSet.ABoneResSet);
				//}
				prevIndex = res.Index;

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

			prevIndex = -1;
			if (_res != null)
			{
				_res.Close();
				_res = null;
			}
		}
	}
}
