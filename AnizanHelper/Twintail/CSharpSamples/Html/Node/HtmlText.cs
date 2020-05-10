// HtmlText.cs

namespace CSharpSamples.Html
{
	/// <summary>
	/// Html内のテキストを表す
	/// </summary>
	public class HtmlText : HtmlNode
	{
		private string text;

		/// <summary>
		/// テキストの内容を取得または設定
		/// </summary>
		public string Content
		{
			set
			{
				this.text = value;
			}
			get
			{
				return this.text;
			}
		}

		/// <summary>
		/// このプロパティはContentプロパティと同じ値を返す
		/// </summary>
		public override string Html
		{
			get
			{
				return this.text;
			}
		}

		/// <summary>
		/// このプロパティはContentプロパティと同じ値を返す
		/// </summary>
		public override string InnerHtml
		{
			get
			{
				return this.text;
			}
		}

		/// <summary>
		/// このプロパティはContentプロパティと同じ値を返す
		/// </summary>
		public override string InnerText
		{
			get
			{
				return this.text;
			}
		}

		/// <summary>
		/// HtmlTextクラスのインスタンスを初期化
		/// </summary>
		/// <param name="text"></param>
		public HtmlText(string text)
		{
			this.text = text;
		}

		/// <summary>
		/// HtmlTextクラスのインスタンスを初期化
		/// </summary>
		public HtmlText() : this(string.Empty)
		{
			// 
			// TODO: コンストラクタ ロジックをここに追加してください。
			//
		}
	}
}
