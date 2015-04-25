// JbbsThreadHeader.cs

namespace Twin.Bbs
{
	using System;

	/// <summary>
	/// JbbsThreadHeader の概要の説明です。
	/// </summary>
	public class JbbsThreadHeader : MachiThreadHeader
	{
		/// <summary>
		/// スレッドのURLを取得
		/// </summary>
		public override string Url {
			get {
				return String.Format("http://{0}/bbs/read.cgi/{1}/{2}/",
					BoardInfo.Server, BoardInfo.Path, Key);
			}
		}

		public override string DatUrl
		{
			get
			{
				return String.Format("http://{0}/bbs/rawmode.cgi/{1}/{2}/",
					BoardInfo.Server, BoardInfo.Path, Key);
			}
		}

		/// <summary>
		/// 書き込み可能な最大レス数を取得
		/// </summary>
		public override int UpperLimitResCount {
			get {
				return 10000; // したらばの最大レス数がわかんないので適当に…
			}
		}

		/// <summary>
		/// JbbsThreadHeaderクラスのインスタンスを初期化
		/// </summary>
		public JbbsThreadHeader()
		{
			// 
			// TODO: コンストラクタ ロジックをここに追加してください。
			//
		}
	}
}
