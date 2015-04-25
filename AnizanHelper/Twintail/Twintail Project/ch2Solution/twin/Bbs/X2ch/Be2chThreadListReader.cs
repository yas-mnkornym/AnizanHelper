// Be2chThreadListReader.cs

using System;
using System.Text;

namespace Twin.Bbs
{
	/// <summary>
	/// Be2chThreadListReader の概要の説明です。
	/// </summary>
	public class Be2chThreadListReader : X2chThreadListReader
	{
		/// <summary>
		/// Be2chThreadListReader クラスのインスタンスを初期化
		/// </summary>
		public Be2chThreadListReader()
			: base(new X2chThreadListParser(BbsType.Be2ch, Encoding.GetEncoding("euc-jp")))
		{
			// 
			// TODO: コンストラクタ ロジックをここに追加してください。
			//
		}
	}
}
