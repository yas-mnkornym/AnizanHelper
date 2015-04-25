// AbstractExtractor.cs

namespace Twin.Text
{
	using System;
	using Twin.Text;
	using CSharpSamples;

	/// <summary>
	/// レス抽出機能を実装する基本クラス
	/// </summary>
	public abstract class AbstractExtractor
	{
		private SearchOptions options;
		private bool newWindow;

		/// <summary>
		/// 検索オプションを取得または設定
		/// </summary>
		public SearchOptions Options {
			set { options = value; }
			get { return options; }
		}

		/// <summary>
		/// 抽出結果を新しいウインドウで表示するかどうか
		/// </summary>
		public bool NewWindow {
			set {
				if (newWindow != value)
					newWindow = value;
			}
			get { return newWindow; }
		}

		/// <summary>
		/// AbstractExtractorクラスのインスタンスを初期化
		/// </summary>
		public AbstractExtractor()
		{
			// 
			// TODO: コンストラクタ ロジックをここに追加してください。
			//
			options = SearchOptions.None;
			newWindow = false;
		}

		
		/// <summary>
		/// 指定したキーワードを含むレスを抽出
		/// </summary>
		/// <param name="keyword">検索キーワード</param>
		/// <param name="element">検索対象の要素</param>
		/// <returns></returns>
		public abstract ResSetCollection Extract(string keyword, ResSetElement element);

		/// <summary>
		/// 指定したキーワードを含むレスを抽出し表示
		/// </summary>
		/// <param name="keyword">検索キーワード</param>
		/// <param name="element">検索対象の要素</param>
		public abstract bool InnerExtract(string keyword, ResSetElement element);

		/// <summary>
		/// すべてのリンクを取得
		/// </summary>
		/// <returns></returns>
		public abstract LinkCollection GetLinks();

		public abstract void Reset();
	}
}
