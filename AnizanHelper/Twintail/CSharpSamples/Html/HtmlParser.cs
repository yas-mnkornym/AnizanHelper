// HtmlParser.cs

namespace CSharpSamples.Html
{
	using System;
	using System.Collections;
	using System.Text;
	using System.Text.RegularExpressions;

	/// <summary>
	/// HtmlParser の概要の説明です。
	/// </summary>
	public class HtmlParser
	{
		private readonly Regex rexTagName = new Regex("[a-zA-Z0-9]+", RegexOptions.Compiled);
		private readonly char[] QuoteChars = ("\"\'").ToCharArray();

		/// <summary>
		/// HtmlParserクラスのインスタンスを初期化
		/// </summary>
		public HtmlParser()
		{
			// 
			// TODO: コンストラクタ ロジックをここに追加してください。
			//
		}

		/// <summary>
		/// Htmlを解析してノードコレクションを生成
		/// </summary>
		/// <param name="html"></param>
		/// <returns></returns>
		public HtmlNodeCollection Parse(string html)
		{
			if (html == null)
			{
				throw new ArgumentNullException("html");
			}

			// 改行文字などを削除
			html = this.RemoveWhiteSpace(html);

			HtmlNodeCollection root = new HtmlNodeCollection(null);
			int index = 0;

			while (index < html.Length)
			{
				// コメントは無視する
				if (this.is_match(html, index, "<!--"))
				{
					index += 4;

					// コメントが終わるまでindexを進める
					while (index < html.Length)
					{
						if (this.is_match(html, index, "-->"))
						{
							index += 3;
							break;
						}
						index++;
					}
				}
				// 閉じるタグの場合
				else if (this.is_match(html, index, "</"))
				{
					index += 2;

					// 空白読み飛ばし
					this.SkipWhiteSpace(html, ref index);

					// タグ名を取得
					Match m = this.rexTagName.Match(html, index);
					if (m.Success)
					{
						// 同じ名前の開始タグを取得
						int nodeidx = this.BackFindOpenElement(root, m.Value);
						if (nodeidx != -1)
						{
							this.NodesDown(root, nodeidx + 1, (HtmlElement)root[nodeidx]);
						}
						else
						{
							// throw new HtmlException();
						}
						index += m.Length;
					}

					// 終了タグが来るまでスキップ
					this.SkipChars(html, ref index, "<>".ToCharArray());

					if (this.is_match(html, index, ">"))
					{
						index++;
					}
				}
				// 開始タグの場合
				else if (this.is_match(html, index, "<"))
				{
					index += 1;

					// 空白読み飛ばし
					this.SkipWhiteSpace(html, ref index);

					// タグ名を取得
					Match m = this.rexTagName.Match(html, index);
					if (m.Success)
					{
						HtmlElement e = new HtmlElement(m.Value.ToLower());
						root.Add(e);

						index = m.Index + m.Length;

						// 空白読み飛ばし
						this.SkipWhiteSpace(html, ref index);

						// 属性の読み込み
						if (this.not_match(html, index, "/>") &&
							this.not_match(html, index, ">"))
						{
							this.ParseAttributes(html, ref index, e.Attributes);
						}

						// １つで完結するタグの処理
						if (this.is_match(html, index, "/>"))
						{
							e.IsEmptyElementTag = true;
							index += 2;
						}
						// 終了タグの場合
						else if (this.is_match(html, index, ">"))
						{
							index++;
						}
					}
				}
				// テキスト処理
				else
				{

					// 開始タグを検索し、そこまでをテキストとする
					int next = html.IndexOf("<", index);
					HtmlText text;

					if (next == -1)
					{
						text = new HtmlText(html.Substring(index));
						index = html.Length;
					}
					else
					{
						text = new HtmlText(html.Substring(index, next - index));
						index = next;
					}

					root.Add(text);
				}
			}

			return root;
		}

		/// <summary>
		/// 指定した名前と同じタグ要素を検索
		/// </summary>
		/// <param name="nodes"></param>
		/// <param name="name"></param>
		/// <returns></returns>
		private int BackFindOpenElement(HtmlNodeCollection nodes, string name)
		{
			for (int i = nodes.Count - 1; i >= 0; i--)
			{
				HtmlElement e = nodes[i] as HtmlElement;

				if (e != null && !e.IsTerminated)
				{
					// 大文字小文字は区別しない
					if (e.Name.ToLower() == name.ToLower())
					{
						return i;
					}
				}
			}
			return -1;
		}

		/// <summary>
		/// 属性を解析しattributes変数に格納
		/// </summary>
		/// <param name="html"></param>
		/// <param name="index"></param>
		/// <param name="attributes"></param>
		private void ParseAttributes(string html, ref int index,
			HtmlAttributeCollection attributes)
		{
			while (index < html.Length)
			{
				// 空白読み飛ばし
				this.SkipWhiteSpace(html, ref index);

				// タグの終わりなら終了
				if (this.is_match(html, index, "/>") || this.is_match(html, index, ">"))
				{
					break;
				}

				int attrSt = index;

				// 属性名を検索
				if (this.SkipChars(html, ref index, "=".ToCharArray()))
				{
					string attrName, attrVal = string.Empty;

					attrName = html.Substring(attrSt, index - attrSt).ToLower();
					index++;

					// 空白読み飛ばし
					this.SkipWhiteSpace(html, ref index);

					// クオーテーションから始まる場合は、同じクオーテーションが出るまでを値とする
					if (index < html.Length && this.IsQuote(html[index]))
					{
						char quote = html[index];
						int st = index++;

						this.SkipChars(html, ref index, new char[] { quote, '>' });

						attrVal = html.Substring(st, index - st).Trim(this.QuoteChars);

						if (this.is_match(html, index, quote.ToString()))
						{
							index++;
						}
					}
					// すぐに値が始まっている場合
					else
					{
						// 空白かタグの終わりが来るまでindexを進める
						for (int i = index; i < html.Length; i++)
						{
							if (this.is_match(html, i, " ") ||
								this.is_match(html, i, ">") ||
								this.is_match(html, i, "/>"))
							{
								attrVal = html.Substring(index, i - index);
								index = i;
								break;
							}
						}
					}

					// 値を検索
					HtmlAttribute attr = new HtmlAttribute(attrName, attrVal);
					attributes.Add(attr);
				}
				else
				{
					break;
				}
			}
		}

		/// <summary>
		/// 指定した文字がクオーテーションかどうかを判断
		/// </summary>
		/// <param name="ch"></param>
		/// <returns></returns>
		private bool IsQuote(char ch)
		{
			foreach (char quote in this.QuoteChars)
			{
				if (quote == ch)
				{
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// inputのindexからstopCharsのどれかが来るまでindexをインクリメント
		/// </summary>
		/// <param name="input"></param>
		/// <param name="index"></param>
		/// <param name="stopChars"></param>
		/// <returns></returns>
		private bool SkipChars(string input, ref int index, char[] stopChars)
		{
			if (index < 0 || index >= input.Length)
			{
				throw new ArgumentOutOfRangeException("index");
			}

			int r = input.IndexOfAny(stopChars, index);
			if (r != -1)
			{
				index = r;
			}

			return (r != -1) ? true : false;
		}

		/// <summary>
		/// 空白文字が無くなるまでindexをインクリメント
		/// </summary>
		/// <param name="input"></param>
		/// <param name="index"></param>
		private void SkipWhiteSpace(string input, ref int index)
		{
			while (index < input.Length)
			{
				if (!char.IsWhiteSpace(input[index]))
				{
					break;
				}

				index++;
			}
		}

		/// <summary>
		/// element以降のノードをelementの子に設定する
		/// </summary>
		/// <param name="element"></param>
		private void NodesDown(HtmlNodeCollection nodes, int index, HtmlElement parent)
		{
			ArrayList children = new ArrayList();

			while (index < nodes.Count)
			{
				children.Add(nodes[index]);
				nodes.RemoveAt(index);
			}

			foreach (HtmlNode node in children)
			{
				parent.Nodes.Add(node);
			}

			parent.IsTerminated = true;
		}

		/// <summary>
		/// 空白文字を削除したHtmlを返す
		/// </summary>
		/// <param name="html"></param>
		/// <returns></returns>
		private string RemoveWhiteSpace(string html)
		{
			StringBuilder sb = new StringBuilder(html);
			sb.Replace("\r", "");
			sb.Replace("\n", "");
			sb.Replace("\t", " ");

			return sb.ToString();
		}

		/// <summary>
		/// inputのindexからkeyが一致するかどうかを判断
		/// </summary>
		/// <param name="input"></param>
		/// <param name="index"></param>
		/// <param name="key"></param>
		/// <returns></returns>
		private bool is_match(string input, int index, string key)
		{
			if (index < 0 || index >= input.Length)
			{
				throw new ArgumentOutOfRangeException("index");
			}

			if ((input.Length - index) < key.Length)
			{
				return false;
			}

			string s = input.Substring(index, key.Length);
			return s.Equals(key);
		}

		/// <summary>
		/// inputのindexからkeyが一致しないかどうかを判断
		/// </summary>
		/// <param name="input"></param>
		/// <param name="index"></param>
		/// <param name="key"></param>
		/// <returns></returns>
		private bool not_match(string input, int index, string key)
		{
			return !this.is_match(input, index, key);
		}
	}
}
