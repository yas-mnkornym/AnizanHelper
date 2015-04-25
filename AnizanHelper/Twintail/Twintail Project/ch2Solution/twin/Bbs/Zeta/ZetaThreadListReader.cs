// ZetaThreadListReader.cs

namespace Twin.Bbs
{
	using System;
	using Twin.IO;

	/// <summary>
	/// Zetabbsのスレッド一覧を読み込むクラス (2ch互換)
	/// </summary>
	public class ZetaThreadListReader : X2chThreadListReader
	{
		/// <summary>
		/// ZetaThreadListReaderクラスのインスタンスを初期化
		/// </summary>
		public ZetaThreadListReader() : base(new ZetaThreadListParser())
		{
			// 
			// TODO: コンストラクタ ロジックをここに追加してください。
			//
		}
	}
}
