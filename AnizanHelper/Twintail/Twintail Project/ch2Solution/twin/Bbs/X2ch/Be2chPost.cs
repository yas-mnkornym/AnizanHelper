// Be2chPost.cs

using System;
using System.Text;

namespace Twin.Bbs
{
	/// <summary>
	/// Be2chPost の概要の説明です。
	/// </summary>
	public class Be2chPost : X2chPost
	{
		//public override bool CanPostThread{get {return false;}}
		//public override bool CanPostRes{get {return false;}}

		public override bool SendBeID {
			set {}
			get { return true; }
		}

		/// <summary>
		/// Be2chPost クラスのインスタンスを初期化
		/// </summary>
		public Be2chPost() : base(Encoding.GetEncoding("euc-jp"))
		{
			// 
			// TODO: コンストラクタ ロジックをここに追加してください。
			//
		}
	}
}
