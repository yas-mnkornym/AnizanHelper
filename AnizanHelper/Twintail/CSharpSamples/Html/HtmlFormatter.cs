// HtmlFormatter.cs

namespace CSharpSamples.Html
{
	using System;
	using System.Text;

	/// <summary>
	/// HtmlFormatter の概要の説明です。
	/// </summary>
	public class HtmlFormatter
	{
		private string newline;
		private char indentChar;
		private int indentCount;

		private int indent; // 現在のインデント数を表す

		/// <summary>
		/// 改行コードを取得または設定
		/// </summary>
		public string NewLine
		{
			set
			{
				this.newline = value;
			}
			get
			{
				return this.newline;
			}
		}

		/// <summary>
		/// インデントに使用する文字を取得または設定
		/// </summary>
		public char IndentChar
		{
			set
			{
				this.indentChar = value;
			}
			get
			{
				return this.indentChar;
			}
		}

		/// <summary>
		/// インデント数を取得または設定
		/// </summary>
		public int IndentCount
		{
			set
			{
				if (value < 1)
				{
					throw new ArgumentOutOfRangeException("IndentCount");
				}

				this.indentCount = value;
			}
			get
			{
				return this.indentCount;
			}
		}

		/// <summary>
		/// HtmlFormatterクラスのインスタンスを初期化
		/// </summary>
		/// <param name="elem"></param>
		public HtmlFormatter()
		{
			// 
			// TODO: コンストラクタ ロジックをここに追加してください。
			//
			this.newline = Environment.NewLine;
			this.indentChar = ' ';
			this.indentCount = 2;
			this.indent = 0;
		}

		/// <summary>
		/// 指定したノードコレクションのフォーマット済みHtmlを返す
		/// </summary>
		/// <param name="nodeCollection"></param>
		/// <returns></returns>
		public virtual string Format(HtmlNodeCollection nodeCollection)
		{
			if (nodeCollection == null)
			{
				throw new ArgumentNullException("nodeCollection");
			}

			StringBuilder sb = new StringBuilder();

			foreach (HtmlNode node in nodeCollection)
			{
				if (node is HtmlText)
				{
					sb.Append(((HtmlText)node).Content);
				}
				else
				{
					string html = this.Format((HtmlElement)node);
					sb.Append(html);
				}
			}

			return sb.ToString();
		}

		/// <summary>
		/// 指定したノードのフォーマット済みHtmlを返す
		/// </summary>
		/// <param name="element"></param>
		/// <returns></returns>
		public virtual string Format(HtmlElement element)
		{
			if (element == null)
			{
				throw new ArgumentNullException("element");
			}

			StringBuilder sb = new StringBuilder();
			bool format = false;

			// 開始タグ
			sb.Append("<").Append(element.Name);

			// 属性を付加
			foreach (HtmlAttribute attr in element.Attributes)
			{
				sb.Append(" ").Append(attr.Html);
			}

			if (element.Nodes.Count > 0)
			{
				sb.Append(">");

				this.indent++;

				// 子ノードのHtmlを生成
				foreach (HtmlNode child in element.Nodes)
				{
					if (child is HtmlText)
					{
						HtmlText text = (HtmlText)child;
						sb.Append(text.Content);

						format = false;
					}
					else
					{
						HtmlElement childElem = (HtmlElement)child;

						if (childElem.IsTerminated)
						{
							format = true;
						}

						if (format)
						{
							sb.Append(this.newline);
							this.InsertSpace(sb, this.indent);
						}

						string html = this.Format(childElem);
						sb.Append(html);

						format = true;
					}
				}

				--this.indent;

				if (format)
				{
					sb.Append(this.newline);
					this.InsertSpace(sb, this.indent);
				}

				sb.Append("</").Append(element.Name).Append(">");
			}
			else
			{
				if (element.IsEmptyElementTag)
				{
					sb.Append("/>");
				}
				else if (element.IsTerminated)
				{
					sb.Append("></").Append(element.Name).Append(">");
				}
				else
				{
					sb.Append(">");
				}
			}

			return sb.ToString();
		}

		/// <summary>
		/// 指定した数だけインデント
		/// </summary>
		/// <param name="sb"></param>
		/// <param name="count"></param>
		private StringBuilder InsertSpace(StringBuilder sb, int indent)
		{
			return sb.Append(new string(this.indentChar, indent * this.indentCount));
		}
	}
}
