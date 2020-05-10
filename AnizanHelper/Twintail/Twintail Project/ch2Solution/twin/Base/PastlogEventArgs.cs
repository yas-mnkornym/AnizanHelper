using System;
using System.Collections.Generic;
using System.Text;

namespace Twin
{
	public class PastlogEventArgs : EventArgs
	{
		private ThreadHeader headerInfo;
		/// <summary>
		/// スレッドのヘッダー情報を取得します。
		/// </summary>
		public ThreadHeader HeaderInfo
		{
			get
			{
				return headerInfo;
			}
		}
	
		private bool retry = false;
		/// <summary>
		/// 再度取得を試みるかどうかを示す値を取得または設定します。
		/// </summary>
		public bool Retry
		{
			get
			{
				return retry;
			}
			set
			{
				retry = value;
			}
		}

	

		public PastlogEventArgs(ThreadHeader header)
		{
			this.headerInfo = header;
		}
	}
}
