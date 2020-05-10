// ThreadListParser.cs

namespace Twin.Text
{
	using System;
	using System.Text;

	/// <summary>
	/// スレッド一覧を解析するパーサ
	/// </summary>
	public abstract class ThreadListParser : PartialDataParser<ThreadHeader>
	{
		/// <summary>
		/// ThreadListParserクラスのインスタンスを初期化
		/// </summary>
		protected ThreadListParser(BbsType bbs, Encoding encoding)
			: base(bbs, encoding)
		{
			// 
			// TODO: コンストラクタ ロジックをここに追加してください。
			//
		}
	}
}
