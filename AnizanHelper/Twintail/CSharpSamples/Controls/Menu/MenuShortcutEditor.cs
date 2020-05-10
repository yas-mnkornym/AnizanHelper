// MenuShortcutEditor.cs

namespace CSharpSamples
{
	using System;
	using System.Collections.Generic;
	using System.Windows.Forms;

	/// <summary>
	/// MenuShortcutEditor の概要の説明です。
	/// </summary>
	public class MenuShortcutEditor : System.Windows.Forms.Form
	{
		private TreeNode nodeSelected;

		#region Designer Fields
		private System.Windows.Forms.TreeView treeView;
		private System.Windows.Forms.Button buttonOK;
		private System.Windows.Forms.Button buttonCancel;
		private System.Windows.Forms.PropertyGrid propertyGrid;
		private System.Windows.Forms.Splitter splitter1;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.ImageList imageList;
		private System.ComponentModel.IContainer components;
		#endregion

		/// <summary>
		/// MenuShortcutEditorクラスのインスタンスを初期化
		/// </summary>
		public MenuShortcutEditor(MenuStrip menu)
		{
			//
			// Windows フォーム デザイナ サポートに必要です。
			//
			this.InitializeComponent();

			//
			// TODO: InitializeComponent 呼び出しの後に、コンストラクタ コードを追加してください。
			//
			this.nodeSelected = null;
			this.AppendRoots(this.treeView.Nodes, menu.Items);
		}

		/// <summary>
		/// 使用されているリソースに後処理を実行します。
		/// </summary>
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (this.components != null)
				{
					this.components.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// デザイナ サポートに必要なメソッドです。このメソッドの内容を
		/// コード エディタで変更しないでください。
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(MenuShortcutEditor));
			this.treeView = new System.Windows.Forms.TreeView();
			this.imageList = new System.Windows.Forms.ImageList(this.components);
			this.buttonOK = new System.Windows.Forms.Button();
			this.buttonCancel = new System.Windows.Forms.Button();
			this.propertyGrid = new System.Windows.Forms.PropertyGrid();
			this.splitter1 = new System.Windows.Forms.Splitter();
			this.panel1 = new System.Windows.Forms.Panel();
			this.panel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// treeView
			// 
			this.treeView.Dock = System.Windows.Forms.DockStyle.Fill;
			this.treeView.FullRowSelect = true;
			this.treeView.HideSelection = false;
			this.treeView.ImageList = this.imageList;
			this.treeView.Name = "treeView";
			this.treeView.ShowLines = false;
			this.treeView.Size = new System.Drawing.Size(201, 304);
			this.treeView.TabIndex = 0;
			this.treeView.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeView_AfterSelect);
			this.treeView.BeforeExpand += new System.Windows.Forms.TreeViewCancelEventHandler(this.treeView_BeforeExpandCollapse);
			// 
			// imageList
			// 
			this.imageList.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
			this.imageList.ImageSize = new System.Drawing.Size(16, 16);
			this.imageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList.ImageStream")));
			this.imageList.TransparentColor = System.Drawing.Color.Transparent;
			// 
			// buttonOK
			// 
			this.buttonOK.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
			this.buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.buttonOK.Location = new System.Drawing.Point(26, 284);
			this.buttonOK.Name = "buttonOK";
			this.buttonOK.Size = new System.Drawing.Size(80, 20);
			this.buttonOK.TabIndex = 7;
			this.buttonOK.Text = "OK";
			// 
			// buttonCancel
			// 
			this.buttonCancel.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
			this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.buttonCancel.Location = new System.Drawing.Point(114, 284);
			this.buttonCancel.Name = "buttonCancel";
			this.buttonCancel.Size = new System.Drawing.Size(80, 20);
			this.buttonCancel.TabIndex = 8;
			this.buttonCancel.Text = "Cancel";
			// 
			// propertyGrid
			// 
			this.propertyGrid.Anchor = (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
				| System.Windows.Forms.AnchorStyles.Left)
				| System.Windows.Forms.AnchorStyles.Right);
			this.propertyGrid.CommandsVisibleIfAvailable = true;
			this.propertyGrid.LargeButtons = false;
			this.propertyGrid.LineColor = System.Drawing.SystemColors.ScrollBar;
			this.propertyGrid.Name = "propertyGrid";
			this.propertyGrid.Size = new System.Drawing.Size(220, 280);
			this.propertyGrid.TabIndex = 9;
			this.propertyGrid.Text = "propertyGrid1";
			this.propertyGrid.ViewBackColor = System.Drawing.SystemColors.Window;
			this.propertyGrid.ViewForeColor = System.Drawing.SystemColors.WindowText;
			this.propertyGrid.PropertyValueChanged += new System.Windows.Forms.PropertyValueChangedEventHandler(this.propertyGrid_PropertyValueChanged);
			// 
			// splitter1
			// 
			this.splitter1.Dock = System.Windows.Forms.DockStyle.Right;
			this.splitter1.Location = new System.Drawing.Point(201, 0);
			this.splitter1.Name = "splitter1";
			this.splitter1.Size = new System.Drawing.Size(3, 304);
			this.splitter1.TabIndex = 10;
			this.splitter1.TabStop = false;
			// 
			// panel1
			// 
			this.panel1.Controls.AddRange(new System.Windows.Forms.Control[] {
																				 this.buttonOK,
																				 this.buttonCancel,
																				 this.propertyGrid});
			this.panel1.Dock = System.Windows.Forms.DockStyle.Right;
			this.panel1.Location = new System.Drawing.Point(204, 0);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(220, 304);
			this.panel1.TabIndex = 11;
			// 
			// MenuShortcutEditor
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 12);
			this.ClientSize = new System.Drawing.Size(424, 304);
			this.Controls.AddRange(new System.Windows.Forms.Control[] {
																		  this.treeView,
																		  this.splitter1,
																		  this.panel1});
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "MenuShortcutEditor";
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "MenuShortcutEditor";
			this.panel1.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion

		private void AppendRoots(TreeNodeCollection nodes, ToolStripItemCollection items)
		{
			List<TreeNode> list = new List<TreeNode>();

			foreach (ToolStripItem item in items)
			{
				ToolStripMenuItem submenu = item as ToolStripMenuItem;

				if (submenu != null)
				{
					TreeNode node = new TreeNode();
					node.Text = submenu.Text;
					node.Tag = submenu;
					node.ImageIndex = node.SelectedImageIndex = submenu.HasDropDownItems ? 0 : 1;

					if (submenu.HasDropDownItems)
					{
						node.Nodes.Add(new TreeNode("dummy"));
					}

					list.Add(node);
				}
			}

			nodes.AddRange(list.ToArray());
		}

		private void AppendChilds(TreeNode node)
		{
			node.Nodes.Clear();

			ToolStripMenuItem menu = (ToolStripMenuItem)node.Tag;
			this.AppendRoots(node.Nodes, menu.DropDownItems);
		}

		private void treeView_AfterSelect(object sender, System.Windows.Forms.TreeViewEventArgs e)
		{
			ToolStripMenuItem menu = (ToolStripMenuItem)e.Node.Tag;

			this.propertyGrid.SelectedObject = menu;
			this.nodeSelected = e.Node;
		}

		private void treeView_BeforeExpandCollapse(object sender, System.Windows.Forms.TreeViewCancelEventArgs e)
		{
			if (e.Action == TreeViewAction.Collapse)
			{
				e.Node.Nodes.Clear();
				e.Node.Nodes.Add(new TreeNode("dummy"));
			}
			else
			{
				this.AppendChilds(e.Node);
			}
		}

		private void propertyGrid_PropertyValueChanged(object s, System.Windows.Forms.PropertyValueChangedEventArgs e)
		{
			if (this.nodeSelected == null)
			{
				throw new NullReferenceException("nodeSelected is null");
			}

			this.nodeSelected.Text = ((ToolStripMenuItem)this.nodeSelected.Tag).Text;
		}
	}
}
