// ThumbnailsControl.cs

namespace CSharpSamples
{
	using System;
	using System.IO;
	using System.Drawing;
	using System.Threading;
	using System.Collections.Generic;
	using System.Windows.Forms;
	using System.Diagnostics;
	using CSharpSamples.Winapi;

	/// <summary>
	/// リストビューに画像を縮小表示できる機能を追加するクラス
	/// </summary>
	public class ThumbnailsControl : Control
	{
		private ListView listView;
		private ImageList imageList;

		private Dictionary<string, int> imageIndices; // key はファイル名、value は 作成済みサムネイルの ImageIndex。
		private Queue<ListViewItem> queue; // サムネイル未作成の ListViewItem を格納するキュー。
		private ImageFilter filter;

		private ManualResetEvent resetEvent = new ManualResetEvent(false);
		private Thread thread = null;

		private bool disposed = false;

		/// <summary>
		/// サムネイルの画像サイズを取得または設定
		/// </summary>
		public Size ImageSize
		{
			set
			{
				lock (imageList)
					imageList.ImageSize = value;
			}
			get
			{
				lock (imageList)
					return imageList.ImageSize;
			}
		}

		/// <summary>
		/// 画像にかけるフィルターを取得または設定
		/// </summary>
		public ImageFilter Filter
		{
			set
			{
				if (filter != value)
					filter = value;
			}
			get { return filter; }
		}

		/// <summary>
		/// すべてのサムネイルの元ファイル名を取得
		/// </summary>
		public string[] AllItems
		{
			get
			{
				string[] allItems = new string[imageIndices.Count];
				imageIndices.Keys.CopyTo(allItems, 0);

				return allItems;
			}
		}

		/// <summary>
		/// 選択されているサムネイルの元ファイルのパスを取得
		/// </summary>
		public string[] SelectedItems
		{
			get
			{
				List<string> list = new List<string>();

				foreach (ListViewItem item in listView.SelectedItems)
				{
					list.Add(item.Tag as string);
				}
				return list.ToArray();
			}
		}

		/// <summary>
		/// コンテキストメニューを取得または設定
		/// </summary>
		public override ContextMenu ContextMenu
		{
			set
			{
				listView.ContextMenu = value;
			}
			get
			{
				return listView.ContextMenu;
			}
		}

		/// <summary>
		/// サムネイル画像データを取得
		/// </summary>
		public ImageList Thumbnails
		{
			get
			{
				lock (imageList)
					return imageList;
			}
		}

		/// <summary>
		/// ThumbnailsControlクラスのインスタンスを初期化
		/// </summary>
		public ThumbnailsControl()
		{
			// 
			// TODO: コンストラクタ ロジックをここに追加してください。
			//
			imageList = new ImageList();
			imageList.ColorDepth = ColorDepth.Depth32Bit;
			imageList.TransparentColor = Color.White;
			imageList.ImageSize = new Size(100, 100);

			listView = new ListView();
			listView.Dock = DockStyle.Fill;
			listView.View = View.LargeIcon;
			listView.LargeImageList = listView.SmallImageList = imageList;
			Controls.Add(listView);

			filter = ImageFilter.None;
			queue = new Queue<ListViewItem>();
			imageIndices = new Dictionary<string, int>();

			WinApi.SetWindowTheme(listView.Handle, "explorer", null);  
		}

		/// <summary>
		/// 使用しているリソースを解放
		/// </summary>
		/// <param name="disposing"></param>
		protected override void Dispose(bool disposing)
		{
			if (disposed)
				return;
			if (disposing)
			{
				Abort();

				listView.Items.Clear();
				listView.LargeImageList = null;

				imageList.Dispose();
				imageList = null;

				disposed = true;
			}
			base.Dispose(disposing);
		}

		/// <summary>
		/// 指定したファイルのサムネイルを作成し表示する
		/// </summary>
		/// <param name="fileName">サムネイル表示するファイル名</param>
		public void Add(string fileName)
		{
			if (fileName == null)
				throw new ArgumentNullException("fileName");

			AddRange(new string[] { fileName });
		}

		/// <summary>
		/// 指定したファイルのサムネイルを作成し表示する
		/// </summary>
		/// <param name="fileNames">サムネイル表示するファイル名の配列</param>
		public void AddRange(string[] fileNames)
		{
			if (fileNames == null)
				throw new ArgumentNullException("fileNames");

			List<ListViewItem> list = new List<ListViewItem>();

			// 一端アイテムだけを追加
			foreach (string filename in fileNames)
			{
				ListViewItem item = new ListViewItem(Path.GetFileName(filename));
				item.ImageIndex = -1;
				item.Tag = filename;

				lock (queue)
				{
					queue.Enqueue(item);
				}

				list.Add(item);
			}

			lock (listView)
			{
				listView.Items.AddRange(list.ToArray());
			}

			ThreadRun();
		}

		/// <summary>
		/// 指定したファイル名のサムネイルを削除
		/// </summary>
		/// <param name="filename"></param>
		public void Remove(string filename)
		{
			if (filename == null)
				throw new ArgumentNullException("filename");

			for (int i = listView.Items.Count - 1; i >= 0; i--)
			{
				lock (listView)
				{
					if (filename.Equals((string)listView.Items[i].Tag))
						listView.Items.RemoveAt(i);
				}
			}
		}

		/// <summary>
		/// サムネイル生成スレッドを起動
		/// </summary>
		private void ThreadRun()
		{
			resetEvent.Set();
			if (thread == null)
			{
				thread = new Thread(new ThreadStart(__GenerateThread));
				thread.Name = "THUMB_CTRL";
				thread.IsBackground = true;
				thread.Priority = ThreadPriority.Lowest;
				thread.Start();
			}
		}

		/// <summary>
		/// サムネイルの生成を中止する
		/// </summary>
		private void Abort()
		{
			if (thread != null && thread.IsAlive)
				thread.Abort();
			thread = null;
		}

		/// <summary>
		/// サムネイルをクリア
		/// </summary>
		public void Clear()
		{
			resetEvent.Reset();
			listView.Items.Clear();
			lock (imageIndices) imageIndices.Clear();
			lock (imageList) imageList.Images.Clear();
		}

		private void __GenerateThread()
		{
			while (true)
			{
				resetEvent.WaitOne();

				if (queue.Count > 0)
				{
					ListViewItem item;

					lock (queue)
						item = (ListViewItem)queue.Dequeue();

					string filename = (string)item.Tag;

					int imageIndex;
					lock (imageIndices)
					{
						// 既にサムネイルが作成されていないかどうかをチェック
						if (imageIndices.ContainsKey(filename))
						{
							imageIndex = imageIndices[filename];
						}
						// 新規にサムネイルを作成し、作成されたサムネイルのインデックスを取得。
						else
						{
							imageIndex = CreateThumbnail(filename);
						}
						// 作成済みサムネイルのインデックス番号を保存
						imageIndices[filename] = imageIndex;
					}

					// ImageIndex を設定
					MethodInvoker m = delegate { item.ImageIndex = imageIndex; };
					Invoke(m);
				}
				else
				{
					resetEvent.Reset();
				}
			}
		}

		/// <summary>
		/// 指定したファイルのサムネイル画像を作成
		/// </summary>
		/// <param name="fileName"></param>
		/// <returns></returns>
		private int CreateThumbnail(string fileName)
		{
			int imageIndex = -1;

			byte[] bytes = LoadData(fileName);
			if (bytes == null)
			{
				lock (imageList)
				{
					MethodInvoker m = delegate
					{
						imageIndex = imageList.Images.Add(GetErrorImage(),
							imageList.TransparentColor);
					};
					Invoke(m);
				}
				return imageIndex;
			}

			MemoryStream mem = new MemoryStream(bytes);

			using (Image source = new Bitmap(mem))
			{
				float width = (float)ImageSize.Width / source.Width;
				float height = (float)ImageSize.Height / source.Height;
				float percent = Math.Min(width, height);

				width = (source.Width * percent);
				height = (source.Height * percent);

				Rectangle rect = new Rectangle(
					(int)((ImageSize.Width - width) / 2),
					(int)((ImageSize.Height - height) / 2),
					(int)width, (int)height);

				Image buffer = new Bitmap(ImageSize.Width, ImageSize.Height);

				using (Graphics g = Graphics.FromImage(buffer))
				{
					using (Image thumb = source.GetThumbnailImage((int)width, (int)height,
							delegate { return false; }, IntPtr.Zero))
					{
						_SetFilter(thumb as Bitmap);

						lock (imageList)
						{
							g.Clear(imageList.TransparentColor);
						}

						g.DrawImage(thumb, rect);
					}
				}

				lock (imageList)
				{
					MethodInvoker m = delegate
					{
						imageIndex = imageList.Images.Add(buffer,
							imageList.TransparentColor);
					};
					Invoke(m);
				}
			}

			return imageIndex;
		}

		private Image GetErrorImage()
		{
			Image image = new Bitmap(ImageSize.Width, ImageSize.Height);
			using (Graphics g = Graphics.FromImage(image))
			{
				g.DrawLine(Pens.Red, 0, 0, ImageSize.Width, ImageSize.Height);
				g.DrawLine(Pens.Red, ImageSize.Width, 0, 0, ImageSize.Height);
			}
			return image;
		}

		/// <summary>
		/// 指定した画像にフィルタをかける
		/// </summary>
		/// <param name="image"></param>
		private bool _SetFilter(Bitmap image)
		{
			if (image == null)
				throw new ArgumentNullException("image");

			switch (filter)
			{
				case ImageFilter.Alpha:
					return BitmapFilter.Brightness(image, 100);

				case ImageFilter.Mosaic:
					return BitmapFilter.Pixelate(image, 3, false);

				case ImageFilter.GrayScale:
					return BitmapFilter.GrayScale(image);

				default:
					return false;
			}
		}

		protected virtual byte[] LoadData(string uri)
		{
			return File.ReadAllBytes(uri);
		}
	}

	/// <summary>
	/// 画像にかけるフィルターを表す列挙体
	/// </summary>
	public enum ImageFilter
	{
		/// <summary>指定なし</summary>
		None,
		/// <summary>アルファ合成</summary>
		Alpha,
		/// <summary>モザイク</summary>
		Mosaic,
		/// <summary>グレースケール</summary>
		GrayScale,
	}
}
