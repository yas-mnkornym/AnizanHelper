// HtmlDocument.cs

namespace CSharpSamples.Html
{
	using System;
	using System.IO;
	using System.Text;
	using System.Collections;

	/// <summary>
	/// HtmlDocument の概要の説明です。
	/// </summary>
	public class HtmlDocument
	{
		private HtmlNodeCollection root;
		private bool formatted;

		/// <summary>
		/// ルートノードコレクションを取得
		/// </summary>
		public HtmlNodeCollection Root {
			get {
				return root;
			}
		}

		/// <summary>
		/// Htmlドキュメントを取得
		/// </summary>
		public string Html {
			get {
				if (formatted)
				{
					HtmlFormatter f = new HtmlFormatter();
					return f.Format(root);
				}
				else {
					StringBuilder sb = new StringBuilder();

					foreach (HtmlNode node in root)
						sb.Append(node.Html);

					return sb.ToString();
				}
			}
		}

		/// <summary>
		/// フォーマットされたHtmlかどうかを取得または設定
		/// </summary>
		public bool Formatted {
			set {
				if (formatted != value)
					formatted = value;
			}
			get {
				return formatted;
			}
		}

		/// <summary>
		/// HtmlDocumentクラスのインスタンスを初期化
		/// </summary>
		/// <param name="html"></param>
		public HtmlDocument(string html)
		{
			// 
			// TODO: コンストラクタ ロジックをここに追加してください。
			//
			root = null;
			formatted = true;

			LoadHtml(html);
		}

		/// <summary>
		/// HtmlDocumentクラスのインスタンスを初期化
		/// </summary>
		public HtmlDocument() : this(String.Empty)
		{
		}

		/// <summary>
		/// 指定したHtml形式の文字列を読み込む
		/// </summary>
		/// <param name="html"></param>
		public void LoadHtml(string html)
		{
			HtmlParser p = new HtmlParser();
			root = p.Parse(html);
		}

		/// <summary>
		/// 指定したファイルからHtmlを読み込む
		/// </summary>
		/// <param name="filePath"></param>
		/// <param name="encoding"></param>
		public void Load(string filePath, Encoding encoding)
		{
			using (StreamReader sr = new StreamReader(filePath, encoding))
				LoadHtml(sr.ReadToEnd());
		}

		/// <summary>
		/// Htmlをストリームに書き込む
		/// </summary>
		/// <param name="writer"></param>
		public void Save(string filePath)
		{
			using (StreamWriter sw = new StreamWriter(filePath, false, Encoding.Default))
				sw.Write(Html);
		}

		/// <summary>
		/// 指定した名前のタグをすべて取得
		/// </summary>
		/// <param name="tagName"></param>
		/// <returns></returns>
		public HtmlElement[] GetElementsByName(string tagName)
		{
			return HtmlElement.Sta_GetElementsByName(root, tagName);
		}

		/// <summary>
		/// 指定したIDを持つエレメントを取得
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		public HtmlElement GetElementById(string id)
		{
			return HtmlElement.Sta_GetElementById(root, id);
		}

		/// <summary>
		/// このインスタンスをHtml形式の文字列に変換
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			return Html;
		}
	}
}
