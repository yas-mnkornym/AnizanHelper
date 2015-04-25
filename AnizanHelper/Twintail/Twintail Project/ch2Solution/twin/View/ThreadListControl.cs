// ThreadListControl.cs
// #2.0

namespace Twin
{
	using System;
	using System.IO;
	using System.Text;
	using System.Windows.Forms;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.Threading;
	using System.Net;
	using Twin.Bbs;
	using Twin.IO;
	using Twin.Text;

	/// <summary>
	/// スレッド一覧を操作・表示するための基本コントロールクラス
	/// </summary>
	public abstract class ThreadListControl : ClientBaseEx<BoardInfo>
	{
		private Thread thread;					// ネットワークからデータを受信するためのスレッドです。

		private BbsType bbsType;				// 現在開いている掲示板の種類です。
		private bool online;					// true の場合は最新のスレッド一覧を取得し、false の場合はキャッシュされた一覧を読み込みます。

		protected List<ThreadHeader> headerList;	// 受信した最新のスレッド一覧を格納します。

		protected ThreadHeader[] oldItems;			// キャッシュされた前回のスレッド一覧を格納します。

		// 基本となるリーダーです。
		// online の値が true の場合は networkReader、false の場合は offlineReader が格納されます。
		protected ThreadListReader baseReader;

		protected ThreadListReader offlineReader;	// キャッシュされたスレッド一覧を読み込むためのリーダーのインスタンスです。
		protected ThreadListReader networkReader;	// 最新のスレッド一覧を読み込むためのリーダーのインスタンスです。

		protected BoardInfo boardInfo;				// 現在開いている板の情報です。板が開かれていない場合は null です。
		protected bool isOpen;						// 板が開かれていれば true、それ以外は false です。
		protected int bufferSize;					// リーダーの受信バッファサイズです。

		private bool canceled = false;				// スレッド一覧の取得をキャンセルした場合は true です。

		/// <summary>
		/// 現在開いている板の情報を返します。開かれていない場合は null です。
		/// </summary>
		public BoardInfo BoardInfo
		{
			get {
				return boardInfo;
			}
		}

		public override BoardInfo HeaderInfo
		{
			get
			{
				return boardInfo;
			}
		}

		/// <summary>
		/// スレッド一覧を読み込み中であれば true、それ以外は false を返します。
		/// </summary>
		public bool IsReading
		{
			get {
				if (thread == null)
					return false;

				lock (thread)
					return thread.IsAlive;
			}
		}

		/// <summary>
		/// スレッド一覧が開かれていれば true、それ以外は false を返します。
		/// </summary>
		public bool IsOpen
		{
			get {
				return isOpen;
			}
		}

		/// <summary>
		/// このプロパティはサポートしていません。
		/// </summary>
		public bool IsPackageReception
		{
			set {
				//throw new NotSupportedException();
			}
			get {
				return false;
			}
		}

		/// <summary>
		/// 最新のスレッド一覧を取得する場合は true、
		/// 前回キャッシュされたスレッド一覧を読み込む場合は false を設定します。
		/// </summary>
		public bool Online
		{
			set {
				online = value;
			}
			get {
				return online;
			}
		}

		/// <summary>
		/// 現在読み込まれている変更不可なスレッド一覧のコレクションを取得します。
		/// </summary>
		public ReadOnlyCollection<ThreadHeader> Items
		{
			get {
				return new ReadOnlyCollection<ThreadHeader>(headerList);
			}
		}

		/// <summary>
		/// 前回読み込まれたスレッド一覧の配列を取得します。
		/// </summary>
		public ThreadHeader[] OldItems {
			get {
				return oldItems;
			}
		}

		/// <summary>
		/// 選択されているアイテムコレクションを取得します。
		/// </summary>
		public abstract ReadOnlyCollection<ThreadHeader> SelectedItems
		{
			get;
		}

		/// <summary>
		/// 項目が選択されたときに発生します。
		/// </summary>
		public event EventHandler<ThreadListEventArgs> Selected;

		/// <summary>
		/// スレッドが閉じられたときに発生します。
		/// </summary>
		public event EventHandler Closed;

		/// <summary>
		/// ThreadListControlクラスのインスタンスを初期化。
		/// </summary>
		/// <param name="cache"></param>
		protected ThreadListControl(Cache cache) : base(cache)
		{
			headerList = new List<ThreadHeader>();

			// オフライン用のリーダーを作成
			offlineReader = new OfflineThreadListReader(cache);

			oldItems = new ThreadHeader[0];

			bbsType = BbsType.None;
			bufferSize = 4096;

			isOpen = false;
			online = true;
		}

		/// <summary>
		/// 指定した板に対応するリーダーを作成し、開きます。
		/// </summary>
		/// <param name="board"></param>
		/// <returns></returns>
		private bool OpenReader(BoardInfo board)
		{
			if (board.Bbs != bbsType && online)
			{
				bbsType = board.Bbs;
				networkReader = TypeCreator.CreateThreadListReader(bbsType);
				networkReader.ServerChange += new EventHandler<ServerChangeEventArgs>(OnServerChange);
			}

			baseReader = online ?
				new ThreadListReaderRelay(Cache, networkReader) : offlineReader;

			baseReader.BufferSize = bufferSize;
			baseReader.AutoRedirect = true;
			baseReader.Open(board);

			if (online)
			{
				oldItems = ((ThreadListReaderRelay)baseReader).CacheItems.ToArray();
			}

			return baseReader.IsOpen;
		}

		/// <summary>
		/// データを読み込み、進行状態を通知します。
		/// </summary>
		private List<ThreadHeader> Reading()
		{
			List<ThreadHeader> items = new List<ThreadHeader>();
			int read = -1;

			while (read != 0)
			{
				if (canceled)
					break;

				read = baseReader.Read(items);

				OnReceive(new ReceiveEventArgs(
					baseReader.Length, baseReader.Position, read));

				OnStatusTextChanged(
					String.Format("{0}板 受信中 ({1}/{2})",
						boardInfo.Name, baseReader.Position, baseReader.Length));
			}

			return items;
		}

		/// <summary>
		/// 別スレッドとして受信処理を行うメソッドです。
		/// </summary>
		private void OpenInternal()
		{
			// 完了状態を表す
			CompleteStatus status = CompleteStatus.Success;

			List<ThreadHeader> items = null;

			try {
				OnLoading(new EventArgs());
				
				if (OpenReader(boardInfo))
				{
					items = Reading();

					if (canceled)
						return;

					headerList.AddRange(items);
					Invoke(new WriteListMethodInvoker(WriteInternal), new object[] {items});	
				}
			}
			catch (Exception ex) 
			{
				status = CompleteStatus.Error;
				OnStatusTextChanged(ex.Message);
				TwinDll.Output(ex);
			}
			finally {

				if (canceled)
					status = CompleteStatus.Error;
					
				if (baseReader != null)
					baseReader.Close();

				canceled = false;
				
				if (thread != null)
				{
					lock (thread)
						thread = null;
				}

				OnComplete(new CompleteEventArgs(status));

				if (status == CompleteStatus.Success)
				{
					OnStatusTextChanged(
						String.Format("{0}板の読み込みを完了 (総数: {1})",
							boardInfo.Name, headerList.Count));
				}
			}
		}

		private void WriteInternal(List<ThreadHeader> items)
		{
			WriteBegin();

			Write(items);

			WriteEnd();
		}

		/// <summary>
		/// Closedイベントを発生させます。
		/// </summary>
		/// <param name="e"></param>
		protected void OnClosed(EventArgs e)
		{
			if (Closed != null)
				Closed(this, e);
		}

		/// <summary>
		/// スレッド一覧の受信用スレッドを起動します。
		/// </summary>
		protected void ThreadRun()
		{
			thread = new Thread(OpenInternal);
			thread.Name = "TLC_" + this.boardInfo.Path;
			thread.Priority = Priority;
			thread.IsBackground = true;
			thread.Start();
		}

		/// <summary>
		/// Selectedイベントを発生させます。
		/// </summary>
		/// <param name="e"></param>
		protected void OnSelected(ThreadListEventArgs e)
		{
			if (Selected != null)
				Selected(this, e);
		}

		/// <summary>
		/// 板が移転されていた際に呼ばれます。
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected virtual void OnServerChange(object sender, ServerChangeEventArgs e)
		{
		}

		/// <summary>
		/// 指定したヘッダを持つアイテムを更新します。
		/// </summary>
		/// <param name="header"></param>
		public abstract void UpdateItem(ThreadHeader header);

		/// <summary>
		/// 書き込み開始前に呼ばれる関数です。
		/// </summary>
		protected virtual void WriteBegin()
		{}

		/// <summary>
		/// 書き込み完了時に呼ばれる関数です。
		/// </summary>
		protected virtual void WriteEnd()
		{}

		/// <summary>
		/// 継承先で、items を表示する処理を記述します。
		/// </summary>
		/// <param name="items"></param>
		protected abstract void Write(List<ThreadHeader> items);

		/// <summary>
		/// 指定した板を開き、スレッド一覧を取得します。
		/// </summary>
		/// <param name="board"></param>
		public virtual void Open(BoardInfo board)
		{
			if (board == null) {
				throw new ArgumentNullException("board");
			}
			if (IsReading)
				throw new InvalidOperationException("スレッド一覧を読み込み中です");

			if (IsOpen)
				Close();

			isOpen = true;
			boardInfo = board;

			// 開く処理を行う
			OnStatusTextChanged(board.Name + "板を開いています");

			ThreadRun();
		}

		/// <summary>
		/// スレッド一覧を最新の状態に更新します。
		/// </summary>
		public virtual void Reload()
		{
			if (IsReading)
				return;

			if (boardInfo != null)
				Open(boardInfo);
		}

		/// <summary>
		/// スレッド一覧の読み込みを中止します。
		/// </summary>
		public virtual void Stop()
		{
			if (IsReading)
			{
				canceled = true;
				isOpen = false;

				if (baseReader != null)
					baseReader.Cancel();

				thread = null;
				OnStatusTextChanged(boardInfo.Name + "板の読込を中止");
			}
		}

		/// <summary>
		/// スレッド一覧を閉じます。
		/// </summary>
		public virtual void Close()
		{
			if (IsOpen)
			{
				Stop();

				OnClosed(new EventArgs());
				OnStatusTextChanged(String.Empty);
			}

			boardInfo = null;
			headerList.Clear();

			isOpen = false;
			baseReader = null;

			oldItems = null;
			oldItems = new ThreadHeader[0];
		}

		//-------------------------------

		/// <summary>
		/// 現在のリストにアイテムを追加。
		/// リストを読み込んでいる最中にこのメソッドを呼ぶと例外を投げる。
		/// </summary>
		/// <param name="items"></param>
		public virtual void AddItems(List<ThreadHeader> items)
		{
			if (boardInfo == null) {
				throw new InvalidOperationException("板が開かれていません");
			}
			if (items == null) {
				throw new ArgumentNullException("items");
			}
			if (IsReading) {
				throw new InvalidOperationException("リストを読み込み中です");
			}

			headerList.AddRange(items);
			Write(items);
		}

		/// <summary>
		/// 表示されているリスト一覧から指定されたスレッドを削除
		/// </summary>
		/// <param name="items"></param>
		public virtual void RemoveItems(List<ThreadHeader> items)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// 現在のリストを閉じて、指定したリスト設定。
		/// リストを読み込んでいる最中にこのメソッドを呼ぶと例外を投げる。
		/// </summary>
		/// <param name="items"></param>
		public virtual void SetItems(BoardInfo board, List<ThreadHeader> items)
		{
			if (board == null) {
				throw new ArgumentNullException("board");
			}
			if (items == null) {
				throw new ArgumentNullException("items");
			}
			if (IsReading) {
				throw new InvalidOperationException("リストを読み込み中です");
			}

			if (IsOpen)
				Close();

			isOpen = true;
			boardInfo = board;
			headerList.AddRange(items);

			WriteBegin();
			Write(items);
			WriteEnd();
		}

		/// <summary>
		/// 継承先でオーバーライドされれば、表示中のスレッド一覧を印刷
		/// </summary>
		public virtual void Print()
		{
			throw new NotSupportedException("印刷はサポートされていません");
		}

//		/// <summary>
//		/// 指定した板の書き込み履歴一覧を表示
//		/// </summary>
//		/// <param name="board"></param>
//		public abstract void OpenHistory(BoardInfo board);

		/// <summary>
		/// 検索するためのオブジェクトを返す
		/// </summary>
		/// <returns></returns>
		public abstract AbstractSearcher BeginSearch();
	}
}
