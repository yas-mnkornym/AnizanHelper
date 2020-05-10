// ThreadSkinBase.cs.cs

namespace Twin
{
	using System;

	/// <summary>
	/// スキンの基本クラス
	/// </summary>
	public abstract class ThreadSkinBase
	{
		/// <summary>
		/// レス参照の基本となるURLを取得または設定
		/// </summary>
		public abstract string BaseUri {
			set;
			get;
		}
	
		// スキン名を取得
		public abstract string Name {
			get;
		}

		/// <summary>
		/// ヘッダーを取得
		/// </summary>
		/// <param name="header"></param>
		/// <returns></returns>
		public abstract string GetHeader(ThreadHeader header);

		/// <summary>
		/// フッターを取得
		/// </summary>
		/// <param name="header"></param>
		/// <returns></returns>
		public abstract string GetFooter(ThreadHeader header);

		/// <summary>
		/// ResSetをスキンを利用して文字列に変換
		/// </summary>
		/// <param name="resSet"></param>
		/// <returns></returns>
		public abstract string Convert(ResSet resSet);

		/// <summary>
		/// ResSetを一度に変換
		/// </summary>
		/// <param name="resSetCollection"></param>
		/// <returns></returns>
		public abstract string Convert(ResSetCollection resSetCollection);

		/// <summary>
		/// スレッドが開かれた時、一度だけ呼ばれます。
		/// </summary>
		public abstract void Reset();

		/// <summary>
		/// スキンを読み込む
		/// </summary>
		/// <param name="skinFolder"></param>
		public abstract void Load(string skinFolder);
	}
}
