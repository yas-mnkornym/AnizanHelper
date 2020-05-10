// DirectoryPicker.cs

namespace CSharpSamples
{
	using System;
	using System.Windows.Forms;
	using System.Windows.Forms.Design;

	// ----------------------------------------------------------------
	// StartLocation は、Desktop, Favorites, MyComputer, MyDocuments,
	//   MyPictures, NetAndDialUpConnections, NetworkNeighborhood,
	//   Printers, Recent, SendTo, StartMenu, Templates　より選択する。
	//   
	// Styles は、BrowseForComputer, BrowseForEverything, BrowseForPrinter,
	//   RestrictToDomain, RestrictToFilesystem, RestrictToSubfolders,
	//   ShowTextBox 　より選択する。
	// ----------------------------------------------------------------

	/// <summary>
	/// ディレクトリ選択ダイアログ
	/// </summary>
	[Obsolete("System.Windows.Forms.FolderBrowserDialog クラスを使用してください。")]
	public class DirectoryPicker : FolderNameEditor
	{
		private FolderNameEditor.FolderBrowser _folderBrowser;

		/// <summary>
		/// 選択されたディレクトリパスを取得
		/// </summary>
		public string DirectoryPath
		{
			get
			{
				return this._folderBrowser.DirectoryPath;
			}
		}

		/// <summary>
		/// 表示されるテキストを取得または設定
		/// </summary>
		public string Text
		{
			set
			{
				this._folderBrowser.Description = value;
			}
			get
			{
				return this._folderBrowser.Description;
			}
		}

		/// <summary>
		/// DirectoryPickerクラスのインスタンスを初期化
		/// </summary>
		public DirectoryPicker()
		{
			// 
			// TODO: コンストラクタ ロジックをここに追加してください。
			//
			this._folderBrowser = new FolderNameEditor.FolderBrowser();
			this._folderBrowser.StartLocation = FolderNameEditor.FolderBrowserFolder.Desktop;
			this._folderBrowser.Style = FolderNameEditor.FolderBrowserStyles.ShowTextBox;
		}

		~DirectoryPicker()
		{
			this._folderBrowser.Dispose();
		}

		/// <summary>
		/// ディレクトリ選択ダイアログを表示
		/// </summary>
		/// <returns>押されたボタン</returns>
		public DialogResult ShowDialog()
		{
			return this._folderBrowser.ShowDialog();
		}
	}
}
