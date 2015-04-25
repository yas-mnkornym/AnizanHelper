// ThreadParser.cs

namespace Twin.Text
{
	using System;
	using System.Text;

	/// <summary>
	/// スレッドのレスを解析するパーサ
	/// </summary>
	public abstract class ThreadParser : PartialDataParser<ResSet>
	{
		/// <summary>
		/// ThreadParserクラスのインスタンスを初期化
		/// </summary>
		/// <param name="enc"></param>
		public ThreadParser(BbsType bbs, Encoding enc)
			: base(bbs, enc)
		{
			// 
			// TODO: コンストラクタ ロジックをここに追加してください。
			//
		}
	}
}
