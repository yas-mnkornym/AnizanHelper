// PropertyDialog.cs

namespace CSharpSamples
{
	using System;
	using System.ComponentModel;
	using System.Windows.Forms;

	/// <summary>
	/// PropertyDialog の概要の説明です。
	/// </summary>
	public class PropertyDialog : System.Windows.Forms.Form
	{
		private System.Windows.Forms.Button buttonCancel;
		private System.Windows.Forms.Button buttonOK;
		private System.Windows.Forms.TabControl tabControl;
		/// <summary>
		/// 必要なデザイナ変数です。
		/// </summary>
		private System.ComponentModel.Container components = null;

		private Property.PropertyCollection pages;
		private bool modified;

		/// <summary>
		/// データが変更されたかどうかを表す値を取得
		/// </summary>
		[Browsable(false)]
		public bool Modified
		{
			get
			{
				return this.modified;
			}
		}

		/// <summary>
		/// プロパティのページコレクションを取得
		/// </summary>
		public Property.PropertyCollection Pages
		{
			get
			{
				return this.pages;
			}
		}

		public PropertyDialog()
		{
			//
			// Windows フォーム デザイナ サポートに必要です。
			//
			this.InitializeComponent();

			//
			// TODO: InitializeComponent 呼び出しの後に、コンストラクタ コードを追加してください。
			//
			this.modified = false;
			this.pages = new Property.PropertyCollection(this);
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
			this.buttonCancel = new System.Windows.Forms.Button();
			this.buttonOK = new System.Windows.Forms.Button();
			this.tabControl = new System.Windows.Forms.TabControl();
			this.SuspendLayout();
			// 
			// buttonCancel
			// 
			this.buttonCancel.Anchor = (System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right);
			this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.buttonCancel.Location = new System.Drawing.Point(225, 265);
			this.buttonCancel.Name = "buttonCancel";
			this.buttonCancel.Size = new System.Drawing.Size(80, 20);
			this.buttonCancel.TabIndex = 0;
			this.buttonCancel.Text = "キャンセル";
			// 
			// buttonOK
			// 
			this.buttonOK.Anchor = (System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right);
			this.buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.buttonOK.Location = new System.Drawing.Point(140, 265);
			this.buttonOK.Name = "buttonOK";
			this.buttonOK.Size = new System.Drawing.Size(80, 20);
			this.buttonOK.TabIndex = 1;
			this.buttonOK.Text = "OK";
			// 
			// tabControl
			// 
			this.tabControl.Anchor = (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
				| System.Windows.Forms.AnchorStyles.Left)
				| System.Windows.Forms.AnchorStyles.Right);
			this.tabControl.Name = "tabControl";
			this.tabControl.SelectedIndex = 0;
			this.tabControl.Size = new System.Drawing.Size(310, 260);
			this.tabControl.TabIndex = 2;
			// 
			// PropertyDialog
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 12);
			this.CancelButton = this.buttonCancel;
			this.ClientSize = new System.Drawing.Size(307, 288);
			this.Controls.AddRange(new System.Windows.Forms.Control[] {
																		  this.tabControl,
																		  this.buttonOK,
																		  this.buttonCancel});
			this.Name = "PropertyDialog";
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.ResumeLayout(false);

		}
		#endregion

		internal void CreatePage(Property property)
		{
			if (property == null)
			{
				throw new ArgumentNullException("property");
			}

			PropertyGrid grid = new PropertyGrid();
			grid.PropertyValueChanged += new PropertyValueChangedEventHandler(this.propertyGrid_PropertyValueChanged);
			grid.Dock = DockStyle.Fill;
			grid.SelectedObject = property.Data;

			TabPage tab = new TabPage();
			tab.Text = property.Caption;
			tab.Tag = property;
			tab.Controls.Add(grid);

			this.tabControl.TabPages.Add(tab);
		}

		internal void RemovePage(Property property)
		{
			for (int i = 0; i < this.tabControl.TabCount; i++)
			{
				TabPage tab = this.tabControl.TabPages[i];

				if (tab.Tag == property)
				{
					this.tabControl.TabPages.Remove(tab);
					break;
				}
			}
		}

		private void propertyGrid_PropertyValueChanged(object sender, System.Windows.Forms.PropertyValueChangedEventArgs e)
		{
			this.modified = true;
		}
	}
}
