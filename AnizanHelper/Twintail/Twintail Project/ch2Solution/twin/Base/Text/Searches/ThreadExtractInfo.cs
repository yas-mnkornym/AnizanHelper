// ThreadExtractInfo.cs

namespace Twin.Text
{
	using System;
	using CSharpSamples;

	/// <summary>
	/// ThreadExtractInfo の概要の説明です。
	/// </summary>
	public class ThreadExtractInfo
	{
		private string keyword;
		private SearchOptions options;

		/// <summary>
		/// 抽出キーワードを取得
		/// </summary>
		public string Keyword {
			get { return keyword; }
		}

		/// <summary>
		/// 抽出キーワードを取得
		/// </summary>
		public SearchOptions Options {
			get { return options; }
		}


		/// <summary>
		/// ThreadExtractInfoクラスのインスタンスを初期化
		/// </summary>
		/// <param name="keyword"></param>
		public ThreadExtractInfo(string keyword, SearchOptions options)
		{
			// 
			// TODO: コンストラクタ ロジックをここに追加してください。
			//
			this.keyword = keyword;
			this.options = options;
		}
	}
}
