// AaContextMenu.cs

namespace Twin.Tools
{
	using System;
	using System.IO;
	using System.Windows.Forms;
	using System.Drawing;
	using Twin.Aa;
	using System.ComponentModel;

	/// <summary>
	/// AA入力支援のコンテキストメニュー
	/// </summary>
	public sealed class AaContextMenu : IDisposable
	{
		private ContextMenu context;
		private AaHeaderCollection headerCollection;
		private AaHeader activeHeader;
		private string folderPath;
		private Control senderControl;

		private ToolTip tooltip;
		private Point aaPreviewBasePoint;

		//private Container components;
		private bool disposed = false;

		public ContextMenu Context
		{
			get
			{
				return context;
			}
		}


		/// <summary>
		/// AAアイテムが選択されたときに発生
		/// </summary>
		public event AaItemEventHandler Selected;

		/// <summary>
		/// AaContextMenuクラスのインスタンスを初期化
		/// </summary>
		/// <param name="aafolder">AAが存在するフォルダパス</param>
		public AaContextMenu(string aafolder)
		{
			if (aafolder == null)
			{
				throw new ArgumentNullException("aafolder");
			}
			// 
			// TODO: コンストラクタ ロジック
			//
			this.folderPath = aafolder;
			this.headerCollection = new AaHeaderCollection();

			context = new ContextMenu();
			context.Popup += new EventHandler(OnContextPopup);
			context.Collapse += new EventHandler(OnContextCollapse);

			tooltip = new ToolTip();
			tooltip.ShowAlways = true;
			tooltip.InitialDelay = 500;
			tooltip.BackColor = Color.FromArgb(239, 239, 239);
			
			Load();
		}

		~AaContextMenu()
		{
			Dispose(false);
		}

		private void Dispose(bool disposing)
		{
			if (!disposed && disposing)
			{
				tooltip.Dispose();
			}
			senderControl = null;
			disposed = true;
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		void OnContextCollapse(object sender, EventArgs e)
		{
			//tooltip.Hide(senderControl);
		}

		private void OnContextPopup(object sender, EventArgs e)
		{
			for (int i = 0; i < context.MenuItems.Count; i++)
				context.MenuItems[i].Dispose();

			context.MenuItems.Clear();
			
			foreach (AaHeader header in headerCollection)
			{
				MenuItem child = new MenuItem();
				child.Popup += new EventHandler(OnAaHeaderPopup);
				child.Text = Path.GetFileNameWithoutExtension(header.FileName);
				child.MenuItems.Add(new MenuItem("dummy"));
				child.Tag = header;
				context.MenuItems.Add(child);
			}
		}

		private void OnAaHeaderPopup(object sender, EventArgs e)
		{
			MenuItem parent = (MenuItem)sender;

			for (int i = 0; i < parent.MenuItems.Count; i++)
				parent.MenuItems[i].Dispose();

			parent.MenuItems.Clear();

			AaHeader header = (AaHeader)parent.Tag;
			header.Load();

			activeHeader = header;

			foreach (AaItem item in header.Items)
			{
				MenuItem child = new MenuItem();
				child.Select += new EventHandler(aaItem_Select);
				child.Click += new EventHandler(aaItem_Click);
				child.Text = item.Text;
				child.Tag = item;

				parent.MenuItems.Add(child);
			}
		}

		void aaItem_Select(object sender, EventArgs e)
		{
			MenuItem item = (MenuItem)sender;
			AaItem aa = (AaItem)item.Tag;

			if (aa.Single)
			{
				//tooltip.Hide(senderControl);
			}
			else
			{
				//tooltip.Show(aa.Data, senderControl, aaPreviewBasePoint);
			}
		}

		private void aaItem_Click(object sender, EventArgs e)
		{
			MenuItem menu = (MenuItem)sender;
			AaItem item = (AaItem)menu.Tag;

			OnSelected(new AaItemEventArgs(item));
		}

		private void OnSelected(AaItemEventArgs e)
		{
			if (Selected != null)
				Selected(senderControl, e);
		}

		/// <summary>
		/// コンテキストメニューを表示
		/// </summary>
		/// <param name="control"></param>
		/// <param name="location"></param>
		public void Show(Control control, Point location, Point aaPreviewBasePoint)
		{
			this.senderControl = control;
			this.aaPreviewBasePoint = aaPreviewBasePoint;

			if (control != null)
			{
				context.Show(control, location);
			}
		}

		/// <summary>
		/// AAファイルをすべて読み込む
		/// </summary>
		public void Load()
		{
			string[] aafiles = Directory.GetFiles(folderPath, "*.aa");

			headerCollection.Clear();
			context.MenuItems.Clear();

			foreach (string fileName in aafiles)
			{
				AaHeader header = new AaHeader(fileName);
				headerCollection.Add(header);
			}

			context.MenuItems.Add(new MenuItem("dummy"));
		}
	}
}
