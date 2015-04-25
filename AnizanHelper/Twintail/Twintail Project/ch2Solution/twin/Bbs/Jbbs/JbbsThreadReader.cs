// JbbsThreadReader.cs

namespace Twin.Bbs
{
	using System;
	using System.Text;
	using System.Net;
	using Twin.IO;

	/// <summary>
	/// JbbsThreadReader の概要の説明です。
	/// </summary>
	public class JbbsThreadReader : MachiThreadReader
	{
		private HttpWebResponse _res = null;

		private JbbsErrorStatus _errorStatus = JbbsErrorStatus.None;
		/// <summary>
		/// JBBS特有のエラーメッセージを取得します。
		/// </summary>
		public JbbsErrorStatus ErrorStatus
		{
			get
			{
				return _errorStatus;
			}
		}
	
		/// <summary>
		/// JbbsThreadReaderクラスのインスタンスを初期化
		/// </summary>
		public JbbsThreadReader()
			: base(new JbbsThreadParser())
		{
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

			string url = header.DatUrl;

			if (header.GotByteCount > 0)
			{
				// 差分取得用にURLに修正を加える
				url += String.Format("{0}-n", header.GotResCount + 1);
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

			this._errorStatus = ParseErrorStatus(_res.Headers["ERROR"]);

			// OK
			if (_res.StatusCode == HttpStatusCode.OK)
			{
				position = 0;
				length = (int)_res.ContentLength;
				index = header.GotResCount + 1;

				headerInfo.LastModified = _res.LastModified;
				isOpen = true;
			}
			else
			{
				if (_errorStatus == JbbsErrorStatus.StorageIn)
				{
					// dat落ちした予感
					headerInfo.Pastlog = true;
				}
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

		/// <summary>
		/// したらばが返すエラーステータスを解析して、JbbsErrorStatus 列挙体に変換します。
		/// </summary>
		/// <param name="errorStatusString"></param>
		/// <returns></returns>
		protected virtual JbbsErrorStatus ParseErrorStatus(string errorStatusString)
		{
			if (!String.IsNullOrEmpty(errorStatusString))
			{
				errorStatusString = errorStatusString.Replace(" ", String.Empty);

				try
				{
					return (JbbsErrorStatus)Enum.Parse(
						typeof(JbbsErrorStatus), errorStatusString, true);
				}
				// 定義されていない値の場合は無視
				catch (ArgumentException)
				{
				}
			}
			return JbbsErrorStatus.None;
		}
	}

	public enum JbbsErrorStatus
	{
		/// <summary>何もありません</summary>
		None,
		/// <summary>掲示板番号が不正か、またはパラメータが間違っています</summary>
		BBSNotFound,
		/// <summary>スレッド番号が不正か、またはパラメータが間違っています</summary>
		KeyNotFound,
		/// <summary>URLが間違っているか 過去ログに移動せずに削除されています</summary>
		ThreadNotFound,
		/// <summary>該当のスレッドは、過去ログ倉庫に移動されています</summary>
		StorageIn,
	}
}

