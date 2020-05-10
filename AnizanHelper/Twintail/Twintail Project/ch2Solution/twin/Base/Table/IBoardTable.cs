// IBoardTable.cs
// #2.0

namespace Twin
{
	using System;
	using System.Collections.Generic;
	using System.Net;

	/// <summary>
	/// 板一覧を管理するインターフェース
	/// </summary>
	public interface IBoardTable
	{
		/// <summary>
		/// 登録されているすべての Category を格納した List を返します。
		/// </summary>
		List<Category> Items
		{
			get;
		}

		/// <summary>
		/// オンラインで最新の板一覧に更新します。
		/// </summary>
		/// <param name="url">更新先URL</param>
		/// <param name="callback">板が移転していた場合に呼ばれるコールバック</param>
		void OnlineUpdate(string url, BoardUpdateEventHandler callback);

		/// <summary>
		/// 指定した板一覧を現在のテーブルに追加します。
		/// </summary>
		/// <param name="table">追加する IBoardTable。</param>
		void Add(IBoardTable table);

		/// <summary>
		/// 板一覧をすべて削除し、空の状態にします。
		/// </summary>
		void Clear();

		/// <summary>
		/// 板一覧を指定したファイルに保存します。
		/// </summary>
		/// <param name="fileName">保存ファイル名。</param>
		void SaveTable(string fileName);

		/// <summary>
		/// 指定したファイルから板一覧を読み込みます。
		/// </summary>
		/// <param name="fileName">読み込むファイル名。</param>
		void LoadTable(string fileName);

		/// <summary>
		/// 板を別の板へ置き換える
		/// </summary>
		/// <param name="oldBoard">古い板</param>
		/// <param name="newBoard">新しい板</param>
		void Replace(BoardInfo oldBoard, BoardInfo newBoard);

		/// <summary>
		/// board が現在の板一覧内に存在するかどうかを判断します。
		/// </summary>
		/// <param name="board">検索する板</param>
		/// <returns>board が板一覧に存在すれば true、それ以外は false です。</returns>
		bool Contains(BoardInfo board);

		/// <summary>
		/// 現在の板一覧から指定した URL を含む最初に見つかった BoardInfo を返します。
		/// </summary>
		BoardInfo FromUrl(string url);

		/// <summary>
		/// 現在の板一覧から指定した板名およびドメインパスを検索し、最初に見つかった BoardInfo を返します。
		/// </summary>
		BoardInfo FromName(string name, string domainPath);

		/// <summary>
		/// すべての板情報を BoardInfo[] 型に変換して返します。
		/// </summary>
		BoardInfo[] ToArray();
	}
}
