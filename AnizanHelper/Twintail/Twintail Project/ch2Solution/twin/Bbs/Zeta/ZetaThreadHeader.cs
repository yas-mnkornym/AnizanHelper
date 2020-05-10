// ZetaThreadHeader.cs

namespace Twin.Bbs
{
	using System;

	/// <summary>
	/// Zetabbs用のスレッド情報を格納するクラス
	/// </summary>
	public class ZetaThreadHeader : ThreadHeader
	{
		/// <summary>
		/// datの存在するパスを取得
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
				return String.Format("http://{0}/cgi-bin/test/read.cgi/{1}/{2}/",
					BoardInfo.Server, BoardInfo.Path, Key);
			}
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
		/// ZetaThreadHeaderクラスのインスタンスを初期化
		/// </summary>
		public ZetaThreadHeader()
		{
		}
	}
}
