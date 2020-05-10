// ZetaThreadReader.cs

namespace Twin.Bbs
{
	using System;
	using Twin.IO;

	/// <summary>
	/// Zetabbsのスレッドを読み込むクラス (2ch互換)
	/// </summary>
	public class ZetaThreadReader : X2chThreadReader
	{
		/// <summary>
		/// ZetaThreadReaderクラスのインスタンスを初期化
		/// </summary>
		public ZetaThreadReader() : base(new ZetaThreadParser())
		{
			// 
			// TODO: コンストラクタ ロジックをここに追加してください。
			//
		}
	}
}
