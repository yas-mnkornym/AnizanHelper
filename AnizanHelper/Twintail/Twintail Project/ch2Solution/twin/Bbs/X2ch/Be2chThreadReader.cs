// Be2chThreadReader.cs

using System;
using System.Text;

namespace Twin.Bbs
{
	/// <summary>
	/// Be2chThreadReader の概要の説明です。
	/// </summary>
	public class Be2chThreadReader : X2chThreadReader
	{
		/// <summary>
		/// Be2chThreadReader クラスのインスタンスを初期化
		/// </summary>
		public Be2chThreadReader()
			: base(new X2chThreadParser(BbsType.Be2ch, Encoding.GetEncoding("euc-jp")))
		{
			// 
			// TODO: コンストラクタ ロジックをここに追加してください。
			//
		}
	}
}
