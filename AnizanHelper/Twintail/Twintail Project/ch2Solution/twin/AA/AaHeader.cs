// AaHeader.cs

namespace Twin.Aa
{
	using System;
	using System.Text;
	using System.IO;

	/// <summary>
	/// *.aaファイルの情報を表す
	/// </summary>
	public class AaHeader
	{
		private AaItemCollection  items;
		private string fileName;

		/// <summary>
		/// このヘッダー情報が保存されているファイル名を取得
		/// </summary>
		public string FileName {
			get { return fileName; }
		}

		/// <summary>
		/// AaItemが格納されているコレクションを取得
		/// </summary>
		public AaItemCollection Items {
			get { return items; }
		}

		/// <summary>
		/// AaHeaderクラスのインスタンスを初期化
		/// </summary>
		/// <param name="filename">作成するaaファイルへのパス</param>
		public AaHeader(string filename)
		{
			if (filename == null) {
				throw new ArgumentNullException("filename");
			}
			// 
			// TODO: コンストラクタ ロジックをここに追加してください。
			//
			items = new AaItemCollection();
			items.ItemSet += new AaItemSetEventHandler(OnItemSet);
			fileName = filename;
		}

		/// <summary>
		/// ヘッダー情報を読み込む。ファイルが存在しない場合は例外を出す
		/// </summary>
		public void Load()
		{
			StreamReader sr = null;
			string data = null;

			try {
				sr = new StreamReader(fileName, TwinDll.DefaultEncoding);
				items.Clear();

				while ((data = sr.ReadLine()) != null)
				{
					bool single = !data.StartsWith("#");
					string text = single ? data : data.Substring(1);

					AaItem aa = new AaItem(text, single);
					items.Add(aa);
				}
			}
			finally {
				if (sr != null)
					sr.Close();
			}
		}

		/// <summary>
		/// 現在のヘッダー情報をファイルに保存
		/// </summary>
		public void Save()
		{
			StreamWriter sw = null;

			try {
				sw = new StreamWriter(fileName, false, TwinDll.DefaultEncoding);

				foreach (AaItem aa in items)
				{
					sw.WriteLine(aa.ToString());
				}
			}
			finally {
				if (sw != null)
					sw.Close();
			}
		}

		/// <summary>
		/// コレクションに追加されたアイテムの親を自分に設定する
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnItemSet(object sender, AaItemSetEventArgs e)
		{
			e.Item.parent = this;
		}

		/// <summary>
		/// このインスタンスを文字列形式に変換
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			return Path.GetFileNameWithoutExtension(fileName);
		}
	}
}
