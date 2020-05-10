// HtmlElement.cs

namespace CSharpSamples.Html
{
	using System;
	using System.Collections;
	using System.Text;

	/// <summary>
	/// HtmlElement の概要の説明です。
	/// </summary>
	public class HtmlElement : HtmlNode
	{
		private HtmlNodeCollection nodes;
		private HtmlAttributeCollection attributes;

		private bool isTerminated;
		private bool isEmptyElementTag;
		private string name;

		/// <summary>
		/// 子ノードコレクションを取得
		/// </summary>
		public HtmlNodeCollection Nodes
		{
			get
			{
				return this.nodes;
			}
		}

		/// <summary>
		/// 属性コレクションを取得
		/// </summary>
		public HtmlAttributeCollection Attributes
		{
			get
			{
				return this.attributes;
			}
		}

		/// <summary>
		/// １つのタグで完結するタグかどうかを取得または設定
		/// </summary>
		public bool IsEmptyElementTag
		{
			set
			{
				this.isEmptyElementTag = value;
			}
			get
			{
				return this.isEmptyElementTag && this.nodes.Count == 0;
			}
		}

		/// <summary>
		/// 挟む形式のタグかどうかを取得または設定
		/// </summary>
		public bool IsTerminated
		{
			set
			{
				this.isTerminated = value;
			}
			get
			{
				return this.isTerminated || this.Nodes.Count > 0;
			}
		}

		/// <summary>
		/// タグの名前を取得または設定
		/// </summary>
		public string Name
		{
			set
			{
				if (value == null)
				{
					throw new ArgumentNullException("Name");
				}

				this.name = value;
			}
			get
			{
				return this.name;
			}
		}

		/// <summary>
		/// このノードを子ノードも含めHtml形式に変換
		/// </summary>
		public override string Html
		{
			get
			{
				StringBuilder sb = new StringBuilder();

				sb.Append("<").Append(this.Name);

				foreach (HtmlAttribute attr in this.attributes)
				{
					sb.Append(" ").Append(attr.Html);
				}

				if (this.nodes.Count > 0)
				{
					sb.Append(">");

					foreach (HtmlNode child in this.nodes)
					{
						sb.Append(child.Html);
					}

					sb.Append("</").Append(this.Name).Append(">");
				}
				else
				{
					if (this.IsEmptyElementTag)
					{
						sb.Append("/>");
					}
					else if (this.IsTerminated)
					{
						sb.Append("></").Append(this.Name).Append(">");
					}
					else
					{
						sb.Append(">");
					}
				}

				return sb.ToString();
			}
		}

		/// <summary>
		/// このノードの内部Htmlを取得
		/// </summary>
		public override string InnerHtml
		{
			get
			{
				StringBuilder sb = new StringBuilder();

				foreach (HtmlNode child in this.nodes)
				{
					sb.Append(child.InnerHtml);
				}

				return sb.ToString();
			}
		}

		/// <summary>
		/// このノードの内部テキストを取得
		/// </summary>
		public override string InnerText
		{
			get
			{
				StringBuilder sb = new StringBuilder();

				foreach (HtmlNode child in this.nodes)
				{
					sb.Append(child.InnerText);
				}

				return sb.ToString();
			}
		}

		/// <summary>
		/// HtmlElementクラスのインスタンスを初期化
		/// </summary>
		/// <param name="name"></param>
		public HtmlElement(string name)
		{
			// 
			// TODO: コンストラクタ ロジックをここに追加してください。
			//
			this.nodes = new HtmlNodeCollection(this);
			this.attributes = new HtmlAttributeCollection(this);
			this.isTerminated = false;
			this.isEmptyElementTag = false;
			this.name = name;
		}

		/// <summary>
		/// HtmlElementクラスのインスタンスを初期化
		/// </summary>
		public HtmlElement() : this(string.Empty)
		{
		}

		/// <summary>
		/// このエレメントがnodeの親かどうかを判断
		/// </summary>
		/// <param name="node"></param>
		/// <returns></returns>
		public bool IsAncestor(HtmlNode node)
		{
			while (node != null)
			{
				if (this == node.Parent)
				{
					return true;
				}

				node = node.Parent;
			}
			return false;
		}

		/// <summary>
		/// このノードがnodeの子孫かどうかを判断
		/// </summary>
		/// <param name="node"></param>
		/// <returns></returns>
		public bool IsDescendant(HtmlNode node)
		{
			HtmlElement e = node as HtmlElement;
			return e != null ? e.IsAncestor(this) : false;
		}

		/// <summary>
		/// 指定した名前のタグをすべて取得
		/// </summary>
		/// <param name="tagName"></param>
		/// <returns></returns>
		public HtmlElement[] GetElementsByName(string tagName)
		{
			return Sta_GetElementsByName(this.nodes, tagName);
		}

		/// <summary>
		/// 指定したIDを持つエレメントを取得
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		public HtmlElement GetElementById(string id)
		{
			return Sta_GetElementById(this.nodes, id);
		}

		/// <summary>
		/// ノードコレクション内の指定したタグ名前を持つすべてのエレメントを取得
		/// </summary>
		/// <param name="nodes"></param>
		/// <param name="tagName"></param>
		/// <returns></returns>
		internal static HtmlElement[] Sta_GetElementsByName(HtmlNodeCollection nodes, string tagName)
		{
			ArrayList arrayList = new ArrayList();

			foreach (HtmlNode node in nodes)
			{
				HtmlElement elem = node as HtmlElement;

				if (elem != null)
				{
					if (string.Compare(elem.name, tagName, true) == 0)
					{
						arrayList.Add(elem);
					}

					HtmlElement[] array = elem.GetElementsByName(tagName);
					arrayList.AddRange(array);
				}
			}

			return (HtmlElement[])arrayList.ToArray(typeof(HtmlElement));
		}

		/// <summary>
		/// ノードコレクション内の指定したIDを持つエレメントを取得
		/// </summary>
		/// <param name="nodes"></param>
		/// <param name="id"></param>
		/// <returns></returns>
		internal static HtmlElement Sta_GetElementById(HtmlNodeCollection nodes, string id)
		{
			foreach (HtmlNode node in nodes)
			{
				HtmlElement element = node as HtmlElement;

				if (element != null)
				{
					string val = element.Attributes["id"];

					if (string.Compare(id, val, true) == 0)
					{
						return element;
					}

					HtmlElement sub = element.GetElementById(id);
					if (sub != null)
					{
						return sub;
					}
				}
			}

			return null;
		}
	}
}
