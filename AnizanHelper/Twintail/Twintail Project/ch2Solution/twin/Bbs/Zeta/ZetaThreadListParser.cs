// ZetaThreadListParser.cs

namespace Twin.Bbs
{
	using System;
	using System.Text;
	using Twin.Text;

	/// <summary>
	/// Zetabbsのスレッド一覧を解析 (2ch互換)
	/// </summary>
	public class ZetaThreadListParser : X2chThreadListParser
	{
		/// <summary>
		/// ZetaThreadListParserクラスのインスタンスを初期化
		/// </summary>
		public ZetaThreadListParser()
			: base(BbsType.Zeta, Encoding.GetEncoding("Shift_Jis"))
		{
			// 
			// TODO: コンストラクタ ロジックをここに追加してください。
			//
		}
	}
}
