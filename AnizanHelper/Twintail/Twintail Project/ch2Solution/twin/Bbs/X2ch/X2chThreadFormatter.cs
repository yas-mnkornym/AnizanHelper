// X2chThreadFormatter.cs

namespace Twin.Bbs
{
	using System;
	using System.Text;
	using Twin.Text;

	/// <summary>
	/// ResSetを共通のDat形式に変換する機能を提供
	/// </summary>
	public class X2chThreadFormatter : ThreadFormatter
	{
		/// <summary>
		/// X2chThreadFormatterクラスのインスタンスを初期化
		/// </summary>
		public X2chThreadFormatter()
		{
			// 
			// TODO: コンストラクタ ロジックをここに追加してください。
			//
		}

		/// <summary>
		/// resSetを書式化して文字列に変換
		/// </summary>
		/// <param name="resSet"></param>
		/// <returns></returns>
		public override string Format(ResSet resSet)
		{
			StringBuilder sb = new StringBuilder(512);
			sb.Append(resSet.Name);
			sb.Append("<>");
			sb.Append(resSet.Email);
			sb.Append("<>");
			sb.Append(resSet.DateString);
			sb.Append("<>");
			sb.Append(resSet.Body);
			sb.Append("<>");
			sb.Append(resSet.Tag is String ? (String)resSet.Tag : String.Empty);
			sb.Append("\n");

			return sb.ToString();
		}
	
		/// <summary>
		/// resCollectionを書式化して文字列に変換
		/// </summary>
		/// <param name="resCollection"></param>
		/// <returns></returns>
		public override string Format(ResSetCollection resCollection)
		{
			if (resCollection == null) {
				throw new ArgumentNullException("resCollection");
			}

			StringBuilder sb =
				new StringBuilder(512 * resCollection.Count);

			foreach (ResSet res in resCollection)
			{
				string result = Format(res);
				sb.Append(result);
			}

			return sb.ToString();
		}
	}
}
