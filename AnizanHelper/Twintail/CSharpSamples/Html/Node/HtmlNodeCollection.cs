// HtmlNodeCollection.cs

namespace CSharpSamples.Html
{
	using System.Collections;

	/// <summary>
	/// HtmlNodeCollection の概要の説明です。
	/// </summary>
	public class HtmlNodeCollection
	{
		private ArrayList nodes;
		private HtmlElement parent;

		/// <summary>
		/// コレクション内のノード数を取得
		/// </summary>
		public int Count
		{
			get
			{
				return this.nodes.Count;
			}
		}

		/// <summary>
		/// 指定したインデックスにあるノードを取得または設定
		/// </summary>
		public HtmlNode this[int index]
		{
			set
			{
				this.RemoveAt(index);
				this.Insert(index, value);
			}
			get
			{
				return (HtmlNode)this.nodes[index];
			}
		}

		/// <summary>
		/// HtmlNodeCollectionクラスのインスタンスを初期化
		/// </summary>
		/// <param name="parent"></param>
		public HtmlNodeCollection(HtmlElement parent)
		{
			// 
			// TODO: コンストラクタ ロジックをここに追加してください。
			//
			this.nodes = new ArrayList();
			this.parent = parent;
		}

		/// <summary>
		/// newNodeをコレクションの末尾に追加
		/// </summary>
		/// <param name="newNode"></param>
		public void Add(HtmlNode newNode)
		{
			if (newNode.Parent != null)
			{
				throw new HtmlException();  // 同一インスタンスを複数登録することは出来ない
			}

			this.nodes.Add(newNode);
			newNode.SetParent(this.parent);
		}

		/// <summary>
		/// newNodeを指定した位置に挿入
		/// </summary>
		/// <param name="index"></param>
		/// <param name="newNode"></param>
		public void Insert(int index, HtmlNode newNode)
		{
			if (newNode.Parent != null)
			{
				throw new HtmlException();  // 同一インスタンスを複数登録することは出来ない
			}

			this.nodes.Insert(index, newNode);
			newNode.SetParent(this.parent);
		}

		/// <summary>
		/// 指定したノードをコレクションから削除
		/// </summary>
		/// <param name="node"></param>
		public void Remove(HtmlNode node)
		{
			this.nodes.Remove(node);
			node.SetParent(null);
		}

		/// <summary>
		/// 指定した位置にあるノードをコレクションから削除
		/// </summary>
		/// <param name="index"></param>
		/// <param name="node"></param>
		public void RemoveAt(int index)
		{
			HtmlNode node = (HtmlNode)this.nodes[index];

			this.nodes.RemoveAt(index);
			node.SetParent(null);
		}

		/// <summary>
		/// すべてのノードをコレクションから削除
		/// </summary>
		public void RemoveAll()
		{
			while (this.Count > 0)
			{
				this.RemoveAt(0);
			}
		}

		/// <summary>
		/// nodeのコレクション内インデックスを返す
		/// </summary>
		/// <param name="node"></param>
		/// <returns></returns>
		public int IndexOf(HtmlNode node)
		{
			return this.nodes.IndexOf(node);
		}

		/// <summary>
		/// nodeがコレクション内に存在しているかどうかを判断
		/// </summary>
		/// <param name="node"></param>
		/// <returns></returns>
		public bool Contains(HtmlNode node)
		{
			return this.nodes.Contains(node);
		}

		/// <summary>
		/// HtmlNodeCollectionを反復処理する列挙子を返す
		/// </summary>
		/// <returns></returns>
		public IEnumerator GetEnumerator()
		{
			return this.nodes.GetEnumerator();
		}
	}
}
