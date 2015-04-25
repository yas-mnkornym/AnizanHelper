// HtmlNodeCollection.cs

namespace CSharpSamples.Html
{
	using System;
	using System.Collections;
	using System.Diagnostics;

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
		public int Count {
			get {
				return nodes.Count;
			}
		}

		/// <summary>
		/// 指定したインデックスにあるノードを取得または設定
		/// </summary>
		public HtmlNode this[int index] {
			set {
				RemoveAt(index);
				Insert(index, value);
			}
			get {
				return (HtmlNode)nodes[index];
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
				throw new HtmlException();	// 同一インスタンスを複数登録することは出来ない

			nodes.Add(newNode);
			newNode.SetParent(parent);
		}

		/// <summary>
		/// newNodeを指定した位置に挿入
		/// </summary>
		/// <param name="index"></param>
		/// <param name="newNode"></param>
		public void Insert(int index, HtmlNode newNode)
		{
			if (newNode.Parent != null)
				throw new HtmlException();	// 同一インスタンスを複数登録することは出来ない

			nodes.Insert(index, newNode);
			newNode.SetParent(parent);
		}

		/// <summary>
		/// 指定したノードをコレクションから削除
		/// </summary>
		/// <param name="node"></param>
		public void Remove(HtmlNode node)
		{
			nodes.Remove(node);
			node.SetParent(null);
		}

		/// <summary>
		/// 指定した位置にあるノードをコレクションから削除
		/// </summary>
		/// <param name="index"></param>
		/// <param name="node"></param>
		public void RemoveAt(int index)
		{
			HtmlNode node = (HtmlNode)nodes[index];

			nodes.RemoveAt(index);
			node.SetParent(null);
		}

		/// <summary>
		/// すべてのノードをコレクションから削除
		/// </summary>
		public void RemoveAll()
		{
			while (Count > 0)
				RemoveAt(0);
		}

		/// <summary>
		/// nodeのコレクション内インデックスを返す
		/// </summary>
		/// <param name="node"></param>
		/// <returns></returns>
		public int IndexOf(HtmlNode node)
		{
			return nodes.IndexOf(node);
		}

		/// <summary>
		/// nodeがコレクション内に存在しているかどうかを判断
		/// </summary>
		/// <param name="node"></param>
		/// <returns></returns>
		public bool Contains(HtmlNode node)
		{
			return nodes.Contains(node);
		}

		/// <summary>
		/// HtmlNodeCollectionを反復処理する列挙子を返す
		/// </summary>
		/// <returns></returns>
		public IEnumerator GetEnumerator()
		{
			return nodes.GetEnumerator();
		}
	}
}
