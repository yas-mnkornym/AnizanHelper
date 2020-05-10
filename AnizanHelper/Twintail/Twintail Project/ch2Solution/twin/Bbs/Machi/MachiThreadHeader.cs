// MachiThreadHeader.cs

namespace Twin.Bbs
{
	using System;

	/// <summary>
	/// まちBBSのスレッドヘッダ情報を表す
	/// </summary>
	public class MachiThreadHeader : ThreadHeader
	{
		/// <summary>
		/// datファイルの存在するURLを取得
		/// </summary>
		public override string DatUrl {
			get {
				throw new NotSupportedException("まちBBSはdat読みをサポートしていません");
			}
		}

		/// <summary>
		/// スレッドのURLを取得
		/// </summary>
		public override string Url {
			get {
				return String.Format("http://{0}/bbs/read.pl?BBS={1}&KEY={2}",
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
		/// MachiThreadHeaderクラスのインスタンスを初期化
		/// </summary>
		public MachiThreadHeader() : base()
		{
			// 
			// TODO: コンストラクタ ロジックをここに追加してください。
			//
		}
	}
}
