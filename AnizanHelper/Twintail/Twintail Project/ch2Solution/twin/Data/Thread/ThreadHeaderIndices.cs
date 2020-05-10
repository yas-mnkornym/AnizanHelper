// ThreadHeaderManager.cs

namespace Twin
{
	using System;
	using System.IO;
	using System.Text;
	using System.Collections.Generic;
	using System.Windows.Forms;
	using Twin;
	using CSharpSamples.Winapi;
	using Twin.Text;

	/// <summary>
	/// インデックスファイルの一覧を管理
	/// </summary>
	public class ThreadHeaderIndices
	{
		private Cache cache;
		private List<ThreadHeader> items;
		private string fileName;

		/// <summary>
		/// ThreadHeaderのコレクションを取得
		/// </summary>
		public List<ThreadHeader> Items {
			get { return items; }
		}

		/// <summary>
		/// ThreadHeaderIndicesクラスのインスタンスを初期化
		/// </summary>
		/// <param name="cache"></param>
		/// <param name="fileName"></param>
		public ThreadHeaderIndices(Cache cache, string fileName)
		{
			// 
			// TODO: コンストラクタ ロジックをここに追加してください。
			//
			this.cache = cache;
			this.fileName = fileName;
			this.items = new List<ThreadHeader>();
		}

		/// <summary>
		/// インデックス一覧を読み込む
		/// </summary>
		public void Load()
		{
			items.Clear();

			if (File.Exists(fileName))
			{
				StreamReader sr = null;
				string text;

				try {
					sr = new StreamReader(fileName, TwinDll.DefaultEncoding);
					while ((text = sr.ReadLine()) != null)
					{
						string filePath = Path.Combine(Application.StartupPath, text);
						ThreadHeader header = ThreadIndexer.Read(filePath);

						if (header != null)
							items.Add(header);
					}
				}
				finally {
					if (sr != null)
						sr.Close();
				}
			}
		}

		/// <summary>
		/// インデックス一覧をファイルに保存
		/// </summary>
		public void Save()
		{
			StreamWriter sw = null;
			try {
				sw = new StreamWriter(fileName, false, TwinDll.DefaultEncoding);
				foreach (ThreadHeader header in items)
				{
					if (ThreadIndexer.Exists(cache, header))
					{
						// 相対パスに変換
						string relative = Shlwapi.GetRelativePath(
							Application.StartupPath, cache.GetIndexPath(header));

						sw.WriteLine(relative);
					}
				}
			}
			finally {
				if (sw != null)
					sw.Close();
			}
		}
	}
}
