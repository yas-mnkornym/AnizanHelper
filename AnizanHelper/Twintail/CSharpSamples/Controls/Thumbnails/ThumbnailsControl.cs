// ThumbnailsControl.cs

namespace CSharpSamples
{
	using System;
	using System.Collections.Generic;
	using System.Drawing;
	using System.IO;
	using System.Threading;
	using System.Windows.Forms;
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
				lock (this.imageList)
				{
					this.imageList.ImageSize = value;
				}
			}
			get
			{
				lock (this.imageList)
				{
					return this.imageList.ImageSize;
				}
			}
		}

		/// <summary>
		/// 画像にかけるフィルターを取得または設定
		/// </summary>
		public ImageFilter Filter
		{
			set
			{
				if (this.filter != value)
				{
					this.filter = value;
				}
			}
			get { return this.filter; }
		}

		/// <summary>
		/// すべてのサムネイルの元ファイル名を取得
		/// </summary>
		public string[] AllItems
		{
			get
			{
				string[] allItems = new string[this.imageIndices.Count];
				this.imageIndices.Keys.CopyTo(allItems, 0);

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

				foreach (ListViewItem item in this.listView.SelectedItems)
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
				this.listView.ContextMenu = value;
			}
			get
			{
				return this.listView.ContextMenu;
			}
		}

		/// <summary>
		/// サムネイル画像データを取得
		/// </summary>
		public ImageList Thumbnails
		{
			get
			{
				lock (this.imageList)
				{
					return this.imageList;
				}
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
			this.imageList = new ImageList();
			this.imageList.ColorDepth = ColorDepth.Depth32Bit;
			this.imageList.TransparentColor = Color.White;
			this.imageList.ImageSize = new Size(100, 100);

			this.listView = new ListView();
			this.listView.Dock = DockStyle.Fill;
			this.listView.View = View.LargeIcon;
			this.listView.LargeImageList = this.listView.SmallImageList = this.imageList;
			this.Controls.Add(this.listView);

			this.filter = ImageFilter.None;
			this.queue = new Queue<ListViewItem>();
			this.imageIndices = new Dictionary<string, int>();

			WinApi.SetWindowTheme(this.listView.Handle, "explorer", null);
		}

		/// <summary>
		/// 使用しているリソースを解放
		/// </summary>
		/// <param name="disposing"></param>
		protected override void Dispose(bool disposing)
		{
			if (this.disposed)
			{
				return;
			}

			if (disposing)
			{
				this.Abort();

				this.listView.Items.Clear();
				this.listView.LargeImageList = null;

				this.imageList.Dispose();
				this.imageList = null;

				this.disposed = true;
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
			{
				throw new ArgumentNullException("fileName");
			}

			this.AddRange(new string[] { fileName });
		}

		/// <summary>
		/// 指定したファイルのサムネイルを作成し表示する
		/// </summary>
		/// <param name="fileNames">サムネイル表示するファイル名の配列</param>
		public void AddRange(string[] fileNames)
		{
			if (fileNames == null)
			{
				throw new ArgumentNullException("fileNames");
			}

			List<ListViewItem> list = new List<ListViewItem>();

			// 一端アイテムだけを追加
			foreach (string filename in fileNames)
			{
				ListViewItem item = new ListViewItem(Path.GetFileName(filename));
				item.ImageIndex = -1;
				item.Tag = filename;

				lock (this.queue)
				{
					this.queue.Enqueue(item);
				}

				list.Add(item);
			}

			lock (this.listView)
			{
				this.listView.Items.AddRange(list.ToArray());
			}

			this.ThreadRun();
		}

		/// <summary>
		/// 指定したファイル名のサムネイルを削除
		/// </summary>
		/// <param name="filename"></param>
		public void Remove(string filename)
		{
			if (filename == null)
			{
				throw new ArgumentNullException("filename");
			}

			for (int i = this.listView.Items.Count - 1; i >= 0; i--)
			{
				lock (this.listView)
				{
					if (filename.Equals((string)this.listView.Items[i].Tag))
					{
						this.listView.Items.RemoveAt(i);
					}
				}
			}
		}

		/// <summary>
		/// サムネイル生成スレッドを起動
		/// </summary>
		private void ThreadRun()
		{
			this.resetEvent.Set();
			if (this.thread == null)
			{
				this.thread = new Thread(new ThreadStart(this.__GenerateThread));
				this.thread.Name = "THUMB_CTRL";
				this.thread.IsBackground = true;
				this.thread.Priority = ThreadPriority.Lowest;
				this.thread.Start();
			}
		}

		/// <summary>
		/// サムネイルの生成を中止する
		/// </summary>
		private void Abort()
		{
			if (this.thread != null && this.thread.IsAlive)
			{
				this.thread.Abort();
			}

			this.thread = null;
		}

		/// <summary>
		/// サムネイルをクリア
		/// </summary>
		public void Clear()
		{
			this.resetEvent.Reset();
			this.listView.Items.Clear();
			lock (this.imageIndices)
			{
				this.imageIndices.Clear();
			}

			lock (this.imageList)
			{
				this.imageList.Images.Clear();
			}
		}

		private void __GenerateThread()
		{
			while (true)
			{
				this.resetEvent.WaitOne();

				if (this.queue.Count > 0)
				{
					ListViewItem item;

					lock (this.queue)
					{
						item = (ListViewItem)this.queue.Dequeue();
					}

					string filename = (string)item.Tag;

					int imageIndex;
					lock (this.imageIndices)
					{
						// 既にサムネイルが作成されていないかどうかをチェック
						if (this.imageIndices.ContainsKey(filename))
						{
							imageIndex = this.imageIndices[filename];
						}
						// 新規にサムネイルを作成し、作成されたサムネイルのインデックスを取得。
						else
						{
							imageIndex = this.CreateThumbnail(filename);
						}
						// 作成済みサムネイルのインデックス番号を保存
						this.imageIndices[filename] = imageIndex;
					}

					// ImageIndex を設定
					MethodInvoker m = delegate { item.ImageIndex = imageIndex; };
					this.Invoke(m);
				}
				else
				{
					this.resetEvent.Reset();
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

			byte[] bytes = this.LoadData(fileName);
			if (bytes == null)
			{
				lock (this.imageList)
				{
					MethodInvoker m = delegate
					{
						imageIndex = this.imageList.Images.Add(this.GetErrorImage(),
							this.imageList.TransparentColor);
					};
					this.Invoke(m);
				}
				return imageIndex;
			}

			MemoryStream mem = new MemoryStream(bytes);

			using (Image source = new Bitmap(mem))
			{
				float width = (float)this.ImageSize.Width / source.Width;
				float height = (float)this.ImageSize.Height / source.Height;
				float percent = Math.Min(width, height);

				width = (source.Width * percent);
				height = (source.Height * percent);

				Rectangle rect = new Rectangle(
					(int)((this.ImageSize.Width - width) / 2),
					(int)((this.ImageSize.Height - height) / 2),
					(int)width, (int)height);

				Image buffer = new Bitmap(this.ImageSize.Width, this.ImageSize.Height);

				using (Graphics g = Graphics.FromImage(buffer))
				{
					using (Image thumb = source.GetThumbnailImage((int)width, (int)height,
							delegate { return false; }, IntPtr.Zero))
					{
						this._SetFilter(thumb as Bitmap);

						lock (this.imageList)
						{
							g.Clear(this.imageList.TransparentColor);
						}

						g.DrawImage(thumb, rect);
					}
				}

				lock (this.imageList)
				{
					MethodInvoker m = delegate
					{
						imageIndex = this.imageList.Images.Add(buffer,
							this.imageList.TransparentColor);
					};
					this.Invoke(m);
				}
			}

			return imageIndex;
		}

		private Image GetErrorImage()
		{
			Image image = new Bitmap(this.ImageSize.Width, this.ImageSize.Height);
			using (Graphics g = Graphics.FromImage(image))
			{
				g.DrawLine(Pens.Red, 0, 0, this.ImageSize.Width, this.ImageSize.Height);
				g.DrawLine(Pens.Red, this.ImageSize.Width, 0, 0, this.ImageSize.Height);
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
			{
				throw new ArgumentNullException("image");
			}

			switch (this.filter)
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
