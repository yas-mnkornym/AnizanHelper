// ServerTraceDialog.cs

namespace Twin.Tools
{
	using System;
	using System.Drawing;
	using System.Collections;
	using System.ComponentModel;
	using System.Windows.Forms;
	using System.Threading;

	/// <summary>
	/// ServerTraceDialog の概要の説明です。
	/// </summary>
	public class ServerTraceDialog : System.Windows.Forms.Form
	{
		private X2chServerTracer tracer;
		private BoardInfo source;
		private BoardInfo result;

		private Thread thread;
		private bool success;

		#region Designer Fields
		private System.Windows.Forms.ListBox listBoxServ;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Button buttonGo;
		private System.Windows.Forms.LinkLabel labelNewUrl;
		private System.Windows.Forms.Label labelNewName;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label2;
		/// <summary>
		/// 必要なデザイナ変数です。
		/// </summary>
		private System.ComponentModel.Container components = null;
		#endregion

		/// <summary>
		/// 追跡結果を取得
		/// </summary>
		public BoardInfo Result {
			get { return tracer.Result; }
		}

		/// <summary>
		/// 追跡成功したかどうかを示す値を取得
		/// </summary>
		public bool Success {
			get { return success; }
		}

		/// <summary>
		/// ServerTraceDialogクラスのインスタンスを初期化
		/// </summary>
		public ServerTraceDialog(BoardInfo target)
		{
			//
			// Windows フォーム デザイナ サポートに必要です。
			//
			InitializeComponent();

			//
			// TODO: InitializeComponent 呼び出しの後に、コンストラクタ コードを追加してください。
			//
			tracer = new X2chServerTracer();
			tracer.Tracing += new EventHandler<ServerChangeEventArgs>(OnTracing);

			source = target;
			result = null;

			listBoxServ.SelectedIndex = listBoxServ.Items.Add(source);
			success = false;

			thread = new Thread(new ThreadStart(TracingThread));
			thread.Name = "SERVER_TRACE";
			thread.IsBackground = true;
			thread.Start();
		}

		/// <summary>
		/// 使用されているリソースに後処理を実行します。
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// デザイナ サポートに必要なメソッドです。このメソッドの内容を
		/// コード エディタで変更しないでください。
		/// </summary>
		private void InitializeComponent()
		{
			this.buttonGo = new System.Windows.Forms.Button();
			this.listBoxServ = new System.Windows.Forms.ListBox();
			this.label1 = new System.Windows.Forms.Label();
			this.labelNewUrl = new System.Windows.Forms.LinkLabel();
			this.labelNewName = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// buttonGo
			// 
			this.buttonGo.Enabled = false;
			this.buttonGo.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.buttonGo.Location = new System.Drawing.Point(4, 128);
			this.buttonGo.Name = "buttonGo";
			this.buttonGo.Size = new System.Drawing.Size(172, 20);
			this.buttonGo.TabIndex = 0;
			this.buttonGo.Text = "選択されている板に移動";
			this.buttonGo.Click += new System.EventHandler(this.buttonGo_Click);
			// 
			// listBoxServ
			// 
			this.listBoxServ.ItemHeight = 12;
			this.listBoxServ.Location = new System.Drawing.Point(4, 60);
			this.listBoxServ.Name = "listBoxServ";
			this.listBoxServ.Size = new System.Drawing.Size(172, 64);
			this.listBoxServ.TabIndex = 1;
			this.listBoxServ.SelectedIndexChanged += new System.EventHandler(this.listBoxServ_SelectedIndexChanged);
			// 
			// label1
			// 
			this.label1.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.label1.Location = new System.Drawing.Point(4, 44);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(56, 12);
			this.label1.TabIndex = 2;
			this.label1.Text = "移転履歴";
			// 
			// labelNewUrl
			// 
			this.labelNewUrl.Location = new System.Drawing.Point(40, 24);
			this.labelNewUrl.Name = "labelNewUrl";
			this.labelNewUrl.Size = new System.Drawing.Size(136, 12);
			this.labelNewUrl.TabIndex = 10;
			this.labelNewUrl.TabStop = true;
			this.labelNewUrl.Text = "http://";
			// 
			// labelNewName
			// 
			this.labelNewName.Location = new System.Drawing.Point(40, 4);
			this.labelNewName.Name = "labelNewName";
			this.labelNewName.Size = new System.Drawing.Size(128, 12);
			this.labelNewName.TabIndex = 9;
			this.labelNewName.Text = "hoehoe";
			// 
			// label3
			// 
			this.label3.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.label3.Location = new System.Drawing.Point(4, 24);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(28, 16);
			this.label3.TabIndex = 8;
			this.label3.Text = "URL";
			this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// label2
			// 
			this.label2.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.label2.Location = new System.Drawing.Point(4, 4);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(28, 16);
			this.label2.TabIndex = 7;
			this.label2.Text = "板名";
			this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// ServerTraceDialog
			// 
			this.AcceptButton = this.buttonGo;
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 12);
			this.ClientSize = new System.Drawing.Size(180, 151);
			this.ControlBox = false;
			this.Controls.AddRange(new System.Windows.Forms.Control[] {
																		  this.labelNewUrl,
																		  this.labelNewName,
																		  this.label3,
																		  this.label2,
																		  this.label1,
																		  this.listBoxServ,
																		  this.buttonGo});
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Name = "ServerTraceDialog";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "板を追跡";
			this.ResumeLayout(false);

		}
		#endregion

		private void TracingThread()
		{
			try {
				if (tracer.Trace(source, true))
					success = true;
			}
			catch (ThreadAbortException) {}
			catch (Exception ex) { TwinDll.ShowOutput(ex); }
			finally {
				Invoke(new MethodInvoker(OnFinally));
			}
		}

		private void OnTracing(object sender, ServerChangeEventArgs e)
		{
			labelNewName.Text = e.NewBoard.Name;
			labelNewUrl.Text = e.NewBoard.Url;

			listBoxServ.SelectedIndex = listBoxServ.Items.Add(e.NewBoard);
			buttonGo.Enabled = true;
		}

		private void OnFinally()
		{
			if (success)
			{
				// 追跡成功
			}
			buttonGo.Enabled = true;
			thread = null;
		}

		private void labelNewUrl_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
		{
			System.Diagnostics.Process.Start(e.Link.LinkData.ToString());
		}

		private void listBoxServ_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			BoardInfo board = (BoardInfo)listBoxServ.SelectedItem;
			labelNewName.Text = board.Name;
			labelNewUrl.Text = board.Url;
		}

		private void buttonGo_Click(object sender, System.EventArgs e)
		{
			result = (BoardInfo)listBoxServ.SelectedItem;
		}
	}
}
