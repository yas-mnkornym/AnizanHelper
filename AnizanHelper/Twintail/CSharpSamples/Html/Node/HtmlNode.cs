// HtmlNode.cs

namespace CSharpSamples.Html
{
	/// <summary>
	/// HtmlNode の概要の説明です。
	/// </summary>
	public abstract class HtmlNode
	{
		private HtmlElement parent;

		/// <summary>
		/// このノードが最上層かどうかを取得
		/// </summary>
		public bool IsRoot
		{
			get
			{
				return (this.parent == null);
			}
		}

		/// <summary>
		/// このノードが親ノードかどうかを取得
		/// </summary>
		public bool IsParent
		{
			get
			{
				HtmlElement e = this as HtmlElement;
				return (e != null && e.Nodes.Count > 0) ? true : false;
			}
		}

		/// <summary>
		/// このノードが子ノードかどうかを取得
		/// </summary>
		public bool IsChild
		{
			get
			{
				return (this.parent != null);
			}
		}

		/// <summary>
		/// 親ノードを取得
		/// </summary>
		public HtmlElement Parent
		{
			get
			{
				return this.parent;
			}
		}

		/// <summary>
		/// 前の兄弟ノードを取得
		/// </summary>
		public HtmlNode Prev
		{
			get
			{
				if (this.Index != -1 && this.Parent != null)
				{
					if (this.Index > 0)
					{
						return this.Parent.Nodes[this.Index - 1];
					}
				}
				return null;
			}
		}

		/// <summary>
		/// 次の兄弟ノードを取得
		/// </summary>
		public HtmlNode Next
		{
			get
			{
				if (this.Index != -1 && this.Parent != null)
				{
					if (this.Index + 1 < this.Parent.Nodes.Count)
					{
						return this.Parent.Nodes[this.Index + 1];
					}
				}
				return null;
			}
		}

		/// <summary>
		/// 最初の子ノードを取得 (子ノードが存在しなければnullを返す)
		/// </summary>
		public HtmlNode FirstChild
		{
			get
			{
				HtmlElement e = this as HtmlElement;

				if (e == null || e.Nodes.Count == 0)
				{
					return null;
				}
				else
				{
					return e.Nodes[0];
				}
			}
		}

		/// <summary>
		/// 最後の子ノードを取得 (子ノードが存在しなければnullを返す)
		/// </summary>
		public HtmlNode LastChild
		{
			get
			{
				HtmlElement e = this as HtmlElement;

				if (e == null || e.Nodes.Count == 0)
				{
					return null;
				}
				else
				{
					return e.Nodes[e.Nodes.Count - 1];
				}
			}
		}

		/// <summary>
		/// ノードコレクション内の位置を取得
		/// </summary>
		public int Index
		{
			get
			{
				return (this.Parent != null) ?
					this.Parent.Nodes.IndexOf(this) : -1;
			}
		}

		/// <summary>
		/// このノードをHtml形式の文字列で取得
		/// </summary>
		public abstract string Html
		{
			get;
		}

		/// <summary>
		/// このノードの内部Htmlを取得
		/// </summary>
		public abstract string InnerHtml
		{
			get;
		}

		/// <summary>
		/// このノードの内部テキストを取得
		/// </summary>
		public abstract string InnerText
		{
			get;
		}

		/// <summary>
		/// HtmlNodeクラスのインスタンスを初期化
		/// </summary>
		protected HtmlNode()
		{
			// 
			// TODO: コンストラクタ ロジックをここに追加してください。
			//
			this.parent = null;
		}

		/// <summary>
		/// このノードに新しい親を設定
		/// </summary>
		/// <param name="newParent"></param>
		internal void SetParent(HtmlElement newParent)
		{
			this.parent = newParent;
		}

		/// <summary>
		/// このインスタンスを親ノードから削除
		/// </summary>
		public void Remove()
		{
			if (this.Parent != null)
			{
				this.Parent.Nodes.Remove(this);
			}
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
