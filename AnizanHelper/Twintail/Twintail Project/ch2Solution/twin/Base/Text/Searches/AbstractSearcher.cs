// AbstractSearcher.cs

namespace Twin.Text
{
	using System;
	using CSharpSamples;

	/// <summary>
	/// 検索機能を実装する基本クラス
	/// </summary>
	public abstract class AbstractSearcher
	{
		private SearchOptions options;

		/// <summary>
		/// 検索オプションを取得または設定
		/// </summary>
		public SearchOptions Options {
			set {
				options = value;
			}
			get {
				return options;
			}
		}

		/// <summary>
		/// AbstractSearcherクラスのインスタンスを初期化
		/// </summary>
		protected AbstractSearcher()
		{
			// 
			// TODO: コンストラクタ ロジックをここに追加してください。
			//
			options = SearchOptions.None;
		}
		/// <summary>
		/// 検索をリセット
		/// </summary>
		public abstract void Reset();

		/// <summary>
		/// 検索開始
		/// </summary>
		/// <param name="keyword"></param>
		public abstract bool Search(string keyword);

		/// <summary>
		/// 単語をすべてハイライト表示
		/// </summary>
		public abstract void Highlights(string keyword);
	}
}
