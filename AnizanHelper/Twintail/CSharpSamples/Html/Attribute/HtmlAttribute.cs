// HtmlAttribute.cs

namespace CSharpSamples.Html
{
	/// <summary>
	/// 属性を表すクラス
	/// </summary>
	public class HtmlAttribute
	{
		private string name;
		private string _value;

		/// <summary>
		/// 属性の名前を取得または設定
		/// </summary>
		public string Name
		{
			set
			{
				this.name = value;
			}
			get
			{
				return this.name;
			}
		}

		/// <summary>
		/// 属性の値を取得または設定
		/// </summary>
		public string Value
		{
			set
			{
				this._value = value;
			}
			get
			{
				return this._value;
			}
		}

		/// <summary>
		/// 属性をHtml形式で取得
		/// </summary>
		public string Html
		{
			get
			{
				return string.Format("{0}=\"{1}\"", this.name, this._value);
			}
		}

		/// <summary>
		/// HtmlAttributeクラスのインスタンスを初期化
		/// </summary>
		/// <param name="name">属性名</param>
		/// <param name="val">属性値</param>
		public HtmlAttribute(string name, string val)
		{
			// 
			// TODO: コンストラクタ ロジックをここに追加してください。
			//
			this.name = name;
			this._value = val;
		}

		/// <summary>
		/// HtmlAttributeクラスのインスタンスを初期化
		/// </summary>
		public HtmlAttribute() : this(string.Empty, string.Empty)
		{
		}

		/// <summary>
		/// このインスタンスを文字列形式に変換
		/// </summary>
		/// <returns>Htmlプロパティの値を返す</returns>
		public override string ToString()
		{
			return this.Html;
		}
	}
}
