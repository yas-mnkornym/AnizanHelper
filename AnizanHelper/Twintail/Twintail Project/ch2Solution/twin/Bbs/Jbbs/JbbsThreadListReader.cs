// JbbsThreadListReader.cs

namespace Twin.Bbs
{
	using System;
	using System.Text;
	using Twin.IO;

	/// <summary>
	/// JbbsThreadListReader の概要の説明です。
	/// </summary>
	public class JbbsThreadListReader : MachiThreadListReader
	{
		/// <summary>
		/// JbbsThreadListReaderクラスのインスタンスを初期化
		/// </summary>
		public JbbsThreadListReader()
			: base(new MachiThreadListParser(BbsType.Jbbs, Encoding.GetEncoding("EUC-JP")))
		{
			// 
			// TODO: コンストラクタ ロジックをここに追加してください。
			//
		}
	}
}
