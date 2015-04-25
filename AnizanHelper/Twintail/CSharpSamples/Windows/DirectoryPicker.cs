// DirectoryPicker.cs

namespace CSharpSamples
{
	using System;
	using System.Windows.Forms.Design;
	using System.Windows.Forms;

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
		public string DirectoryPath {
			get {
				return _folderBrowser.DirectoryPath;
			}
		}

		/// <summary>
		/// 表示されるテキストを取得または設定
		/// </summary>
		public string Text {
			set {
				_folderBrowser.Description = value;
			}
			get {
				return _folderBrowser.Description;
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
			_folderBrowser = new FolderNameEditor.FolderBrowser();
			_folderBrowser.StartLocation = FolderNameEditor.FolderBrowserFolder.Desktop;
			_folderBrowser.Style = FolderNameEditor.FolderBrowserStyles.ShowTextBox;
		}

		~DirectoryPicker()
		{
			_folderBrowser.Dispose();
		}

		/// <summary>
		/// ディレクトリ選択ダイアログを表示
		/// </summary>
		/// <returns>押されたボタン</returns>
		public DialogResult ShowDialog()
		{
			return _folderBrowser.ShowDialog();
		}
	}
}
