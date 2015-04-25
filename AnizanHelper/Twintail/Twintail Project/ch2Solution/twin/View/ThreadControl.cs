// ThreadControl.cs

namespace Twin
{
	using System;
	using System.IO;
	using System.Text;
	using System.Windows.Forms;
	using System.Threading;
	using System.Net;
//	using System.Collections.Generic;
	using System.Runtime.CompilerServices;
	using Twin.Bbs;
	using Twin.IO;
	using Twin.Text;
using System.Xml;

	/// <summary>
	/// スレッドを操作・表示するための基本コントロールクラス
	/// </summary>
	public abstract class ThreadControl : ClientBaseEx<ThreadHeader>
	{
		private BbsType bbsType;
		private BoardInfo retryServer;

		private bool isPackageReception;
		private bool useGzip;

		protected ResSetCollection resCollection;
		protected SortedValueCollection<int> indicesValues;

		protected Thread thread;
		protected ThreadReader reader;
		protected ThreadStorage storage;
		protected ThreadHeader headerInfo;
		protected bool modeOpen;				// true=Open, false=Reload
		protected int bufferSize;
		protected bool canceled = false;		// 通信処理がキャンセルされた時に true になる
		protected bool retried = false;

		private object syncObject = new object();


		private DateTime lastCompletedDateTime = DateTime.MinValue;
		/// <summary>
		/// 前回処理完了時の時間を取得します。
		/// </summary>
		public DateTime LastCompletedDateTime
		{
			get
			{
				return lastCompletedDateTime;
			}
		}
	

		private bool aboneDetected = false;
		/// <summary>
		/// スレッドの受信中にあぼーんを検知した場合、true を返します。それ以外は常に false です。
		/// </summary>
		public bool AboneDetected
		{
			get
			{
				return aboneDetected;
			}
		}

	
		/// <summary>
		/// 現在開いているスレッドのヘッダ情報を取得
		/// (スレッドが開かれていなければnullを返す)
		/// </summary>
		public override ThreadHeader HeaderInfo
		{
			get
			{
				return headerInfo;
			}
		}

		private bool __isReading = false;
		/// <summary>
		/// スレッドの読み込み処理中の場合は true。この時はスレッドが起動していて、データに変更が加えられている状態。
		/// </summary>
		public bool IsReading
		{
			protected set
			{
				if (value && IsOpen == false)
					throw new InvalidOperationException("まだスレッドが開かれていません");
				__isReading = value;
			}
			get
			{
				return __isReading;
			}
		}

		private bool isWaiting = false;
		/// <summary>
		/// 読み込み処理を待機中かどうかを判断。Openメソッドが呼ばれてから OnLoadメソッドが呼ばれるまでの間は true。それ以外は false。
		/// </summary>
		public bool IsWaiting
		{
			get
			{
				return isWaiting;
			}
		}

		private bool __isOpen = false;
		/// <summary>
		/// スレッドが開かれているかどうかを判断。Openメソッドが呼ばれてから、Closeメソッドが呼ばれるまでの間は true。それ以外は false。
		/// </summary>
		public bool IsOpen
		{
			protected set
			{
#if DEBUG
				if (IsReading)
					throw new InvalidOperationException("IsReading == true の時にこの変数を変更するのおかしいです");
#endif
				__isOpen = value;
			}
			get
			{
				return __isOpen;
			}
		}

		/// <summary>
		/// 一括受信を行うかどうかを取得または設定
		/// </summary>
		public bool IsPackageReception
		{
			set
			{
				if (isPackageReception != value)
					isPackageReception = value;
			}
			get
			{
				return isPackageReception;
			}
		}

		/// <summary>
		/// キャッシュのGzip圧縮するかどうかを取得または設定
		/// </summary>
		public bool UseGzip
		{
			set
			{
				if (useGzip != value)
					useGzip = value;
			}
			get
			{
				return useGzip;
			}
		}

		/// <summary>
		/// 過去ログ取得失敗時に再取得を試みるサーバー情報を取得または設定
		/// </summary>
		public BoardInfo RetryServer
		{
			set
			{
				retryServer = value;
			}
			get
			{
				return retryServer;
			}
		}

		/// <summary>
		/// 読み取り専用のレスコレクションを取得
		/// </summary>
		public ReadOnlyResSetCollection ResSets
		{
			get
			{
				return new ReadOnlyResSetCollection(resCollection);
			}
		}

		/// <summary>
		/// 文字サイズを取得または設定
		/// </summary>
		public abstract FontSize FontSize
		{
			set;
			get;
		}

		/// <summary>
		/// 選択されている文字列を取得
		/// </summary>
		public abstract string SelectedText
		{
			get;
		}

		/// <summary>
		/// 新着までスクロールがOnなら true、Offなら false を表す。
		/// </summary>
		public abstract bool ScrollToNewRes
		{
			set;
			get;
		}

		/// <summary>
		/// オートスクロールが有効かどうかを取得または設定
		/// </summary>
		public abstract bool AutoScroll
		{
			set;
			get;
		}

		/// <summary>
		/// オートリロードが有効かどうかを取得または設定
		/// </summary>
		public abstract bool AutoReload
		{
			set;
			get;
		}

		/// <summary>
		/// 表示レス数を取得または設定
		/// </summary>
		public abstract int ViewResCount
		{
			set;
			get;
		}

		/// <summary>
		/// レス番号がクリックされたときに発生
		/// </summary>
		public event NumberClickEventHandler NumberClick;

		/// <summary>
		/// URIがクリックされたときに発生
		/// </summary>
		public event UriClickEventHandler UriClick;

		/// <summary>
		/// スレッドが閉じられたときに発生
		/// </summary>
		public event EventHandler Closed;

		/// <summary>
		/// ThreadControlクラスのインスタンスを初期化
		/// </summary>
		/// <param name="cache"></param>
		protected ThreadControl(Cache cache)
			: base(cache)
		{
			// 
			// TODO: コンストラクタ ロジックをここに追加してください。
			//
			resCollection = new ResSetCollection();
			indicesValues = new SortedValueCollection<int>();
			bbsType = BbsType.None;
			bufferSize = 4096;
			useGzip = false;
			isPackageReception = false;
		}

		#region Privateメソッド
		/// <summary>
		/// あぼーんが発生したときに呼ばれる
		/// </summary>
		private void OnABoneInternal(object sender, EventArgs e)
		{
			Invoke(new MethodInvoker(OnABone));
		}

		/// <summary>
		/// dat落ち時
		/// </summary>
		private void OnPastlogInternal(object sender, PastlogEventArgs e)
		{
			MethodInvoker m = delegate
			{
				OnPastlog(e);
			};

			Invoke(m);

			if (e.Retry)
			{
				this.retried = true;
			}
		}

		/// <summary>
		/// newbbsに対応したリーダーを作成
		/// (既に作成済みであれば何もしない)
		/// </summary>
		/// <param name="newbbs"></param>
		private ThreadReader CreateBaseReader(BbsType newbbs)
		{
			if (newbbs != bbsType)
			{
				bbsType = newbbs;

				reader = TypeCreator.CreateThreadReader(newbbs);

				if (reader is X2chKakoThreadReader)
				{
					X2chKakoThreadReader kako = (X2chKakoThreadReader)reader;
					kako.RetryServers = new BoardInfo[] { retryServer };
				}

				reader.ABone += new EventHandler(OnABoneInternal);
				reader.Pastlog += new EventHandler<PastlogEventArgs>(OnPastlogInternal);
			}
			return reader;
		}

		private void ReadCache(ResSetCollection buff)
		{
			// 新規に開く場合のみキャッシュを読み込む
			if (modeOpen)
			{
				if (ThreadIndexer.Exists(Cache, headerInfo))
				{
					ThreadIndexer.Read(Cache, headerInfo);

					try
					{
						storage = new LocalThreadStorage(Cache, headerInfo, StorageMode.Read);
						storage.BufferSize = bufferSize;

						// すべてのレスを読み込み表示
						while (storage.Read(buff) != 0)
							;
					}
					finally
					{
						if (storage != null)
						{
							storage.Close();
							storage = null;
						}
					}

					buff.IsNew = false;
				}
			}
		}

		/// <summary>
		/// リーダーを開く
		/// </summary>
		private bool OpenReader()
		{
			aboneDetected = false;
			retried = false;

			// 未取得スレッドなら現在の設定を反映させる
			if (!ThreadIndexer.Exists(Cache, headerInfo))
			{
				headerInfo.UseGzip = useGzip;
			}
			// 基本リーダーを作成
			reader = CreateBaseReader(headerInfo.BoardInfo.Bbs);

			// リーダーを開く
			reader.BufferSize = bufferSize;
			reader.Open(headerInfo);

			return reader.IsOpen;
		}

		/// <summary>
		/// データを読み込む＆書き込む
		/// </summary>
		private void Reading()
		{
			ResSetCollection items = new ResSetCollection(),
				buffer = new ResSetCollection();
			int read = -1, byteParsed, totalByteCount = 0;

			while (read != 0)
			{
				if (canceled)
					return;

				read = reader.Read(buffer, out byteParsed);

				// あぼーんを検知した場合、処理を中止。
				if (read == -1)
				{
					aboneDetected = true;
					return;
				}

				totalByteCount += byteParsed;

				items.AddRange(buffer);

				// 逐次受信の場合はビューアに書き込む
				if (!isPackageReception)
				{
					if (canceled)
						return;

					Invoke(new WriteResMethodInvoker(WriteInternal), new object[] { buffer });
				}
				buffer.Clear();

				OnReceive(new ReceiveEventArgs(
					reader.Length, reader.Position, read));

				OnStatusTextChanged(
					String.Format("{0} 受信中 ({1}/{2})",
						headerInfo.Subject, reader.Position, reader.Length));
			}

			// 一括受信の場合はここで一気にフラッシュ
			if (isPackageReception)
			{
				if (canceled)
					return;

				Invoke(new WriteResMethodInvoker(WriteInternal), new object[] { items });
			}

			try
			{
				// スレッドのインデックス情報を保存
				storage = new LocalThreadStorage(Cache, headerInfo, StorageMode.Write);
				storage.BufferSize = bufferSize;
				storage.Write(items);

				headerInfo.GotByteCount += totalByteCount;
				headerInfo.GotResCount += items.Count;
				headerInfo.NewResCount = items.Count;
				ThreadIndexer.Write(Cache, headerInfo);
			}
			catch (Exception ex)
			{
				TwinDll.Output(ex);
			}
			finally
			{
				storage.Close();
			}

			SaveThreadListIndices();
		}

		// スレッド一覧のインデックスを保存。壊れていることがたまにあるので、再生成できるようにする
		private void SaveThreadListIndices()
		{
			try
			{
				GotThreadListIndexer.Write(Cache, headerInfo);
			}
			catch (XmlException ex)
			{
				if (ex.Message.IndexOf("ルート要素が見つかりません") >= 0)
				{
					DialogResult r = MessageBox.Show(
						headerInfo.BoardInfo.Name + "板のインデックスが壊れています。今すぐ再生成しますか？\r\n(やたら時間かかる場合があります)",
						"インデックスなんだよ～", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

					if (r == DialogResult.Yes)
					{
						try
						{
							ClientBase.Stopper.Reset();
							ThreadIndexer.Indexing(Cache, headerInfo.BoardInfo);
						}
						finally
						{
							ClientBase.Stopper.Set();
						}
					}
				}
			}
		}

		private void WriteInternal(ResSetCollection items)
		{
			resCollection.AddRange(items);
			Write(items);
		}

		private void OpenInternal()
		{
			CompleteStatus status = CompleteStatus.Success;
			try
			{
				try
				{
					isWaiting = true;
					OnLoading(new EventArgs());
				}
				finally
				{
					isWaiting = false;
				}
				if (canceled)
					return;

				// 開く処理を行う
				if (modeOpen)
					Invoke(new MethodInvoker(Opening));

				// キャッシュを読み込み表示
				ReadCache(resCollection);

				if (canceled)
					return;

				Invoke(new MethodInvoker(WriteBegin));

				if (canceled)
					return;

				if (modeOpen)
				{
					Invoke(new WriteResMethodInvoker(Write), new object[] { resCollection });

					// スクロール位置を復元 (値が0の場合は復元しない)
					if (modeOpen && headerInfo.Position != 0.0f)
					{
						Invoke(new PositionMethodInvoker(SetScrollPosition),
							new object[] { headerInfo.Position });
					}
				}

Retry:
				try
				{
					// サーバーに接続
					if (OpenReader())
					{
						if (canceled)
							return;

						Reading();

						// あぼーんを検知した場合
						if (aboneDetected)
						{
						}
					}
					else
					{
						headerInfo.NewResCount = 0;
						ThreadIndexer.Write(Cache, headerInfo);
					}
				}
				finally
				{
					if (reader != null)
						reader.Close();
				}

				// 再試行が要求された場合、最初から
				if (retried)
					goto Retry;

				if (canceled)
					return;


				Invoke(new MethodInvoker(WriteEnd));
			}
			catch (Exception ex)
			{
				status = CompleteStatus.Error;
	//			isOpen = false; #10/15
				TwinDll.Output(ex);
				OnStatusTextChanged(ex.Message);
			}
			finally
			{

				// 中止された場合はETagをリセット
				if (canceled)
					headerInfo.ETag = String.Empty;

				indicesValues.Clear();

				canceled = false;

				lock (syncObject)
					thread = null;

				if (status == CompleteStatus.Success)
				{
					OnStatusTextChanged(
						String.Format("{0}の読み込みを完了 (新着: {1}件)",
							headerInfo.Subject, headerInfo.NewResCount));
				}
				/** 9/26 追加 **/
				else if (reader is X2chAuthenticateThreadReader)
				{
					X2chRokkaResponseState rokkaState = ((X2chAuthenticateThreadReader)reader).RokkaResponseState;
					if (rokkaState != X2chRokkaResponseState.Success) TwinDll.Output("RokkaResponseState: {0}, URL: {1}, ", rokkaState, headerInfo.Url);
					OnStatusTextChanged(String.Format("RokkaResponseState: {0}", rokkaState));
				}
				/****/

				IsReading = false;
				lastCompletedDateTime = DateTime.Now;

				OnComplete(new CompleteEventArgs(status));
			}
		}
		#endregion

		#region Protectedメソッド
		/// <summary>
		/// Closedイベントを発生させる
		/// </summary>
		/// <param name="e"></param>
		protected void OnClosed(EventArgs e)
		{
			if (Closed != null)
				Closed(this, e);
		}

		/// <summary>
		/// 読み込みスレッドを開始
		/// </summary>
		protected void ThreadRun()
		{
			if (IsOpen == false || IsReading == false)
				throw new InvalidOperationException("IsOpen == false || IsReading == false");

			canceled = false;
			thread = new Thread(OpenInternal);
			thread.Name = "TC_" + this.headerInfo.Key;
			thread.Priority = Priority;
			thread.IsBackground = true;
			thread.Start();
		}

		/// <summary>
		/// NumberClickイベントを発生させる
		/// </summary>
		/// <param name="e"></param>
		protected void OnNumberClick(NumberClickEventArgs e)
		{
			if (NumberClick != null)
				NumberClick(this, e);
		}

		/// <summary>
		/// UriClickイベントを発生させる
		/// </summary>
		/// <param name="e"></param>
		protected void OnUriClick(UriClickEventArgs e)
		{
			if (UriClick != null)
				UriClick(this, e);
		}

		/// <summary>
		/// スクロール位置を設定を行う
		/// </summary>
		/// <param name="value"></param>
		protected virtual void SetScrollPosition(float value)
		{
		}

		/// <summary>
		/// あぼーんが発生したときに呼ばれる
		/// </summary>
		protected virtual void OnABone()
		{
		}

		/// <summary>
		/// スレッドがdat落ちしているときに呼ばれる
		/// </summary>
		protected virtual void OnPastlog(PastlogEventArgs e)
		{
		}

		/// <summary>
		/// リーダー初期化後に呼ばれる関数 (Openメソッドでスレッドが開かれたときのみ)
		/// </summary>
		protected virtual void Opening()
		{
		}

		/// <summary>
		/// 書き込み開始時に呼ばれる
		/// </summary>
		protected virtual void WriteBegin()
		{
		}

		/// <summary>
		/// 書き込み完了時に呼ばれる
		/// </summary>
		protected virtual void WriteEnd()
		{
		}

		/// <summary>
		/// itemsを書き込む
		/// </summary>
		/// <param name="items"></param>
		protected abstract void Write(ResSetCollection items);
		#endregion

		/// <summary>
		/// スレッドを開く
		/// </summary>
		/// <param name="header"></param>
		public virtual void Open(ThreadHeader header, int[] indices)
		{
			indicesValues.Clear();
			if (indices != null)
				indicesValues.AddRange(indices);

			Open(header);
		}

		/// <summary>
		/// スレッドを開く
		/// </summary>
		/// <param name="header"></param>
		public virtual void Open(ThreadHeader header)
		{
			if (header == null)
			{
				throw new ArgumentNullException("header");
			}
			if (IsReading)
				throw new InvalidOperationException("スレッドを読み込み中です");

			Close();

			// 各フラグを設定
			IsOpen = true;
			IsReading = true;

			modeOpen = true;
			headerInfo = header;

			// インデックス情報を読み込む
			if (ThreadIndexer.Exists(Cache, header))
				ThreadIndexer.Read(Cache, header);

			string subj = (header.Subject != String.Empty) ? header.Subject : "[スレッド名不明]";
			OnStatusTextChanged(subj + "を開いています");

			ThreadRun();
		}

		/// <summary>
		/// スレッドを更新 (スレッドを読み込み中なら何もしない)
		/// </summary>
		public virtual void Reload()
		{
			if (IsReading)
				return;

			if (IsOpen)
			{
				IsReading = true;
				modeOpen = false;	// 更新する場合はfalse
				OnStatusTextChanged(headerInfo.Subject + "を更新します");

				ThreadRun();
			}
			else if (headerInfo != null)
			{
				Open(headerInfo);
			}
		}

		/// <summary>
		/// スレッドの読み込みを中止 (読み込み中でなければ何もしない)
		/// </summary>
		public virtual void Stop()
		{
			if (IsReading && !canceled)
			{
				canceled = true;

				if (reader != null)
					reader.Cancel();

				//lock (syncObject)
				//	thread = null;

				if (headerInfo != null)
					OnStatusTextChanged(headerInfo.Subject + "の読み込みを中止");
			}
			IsReading = false;
		}

		/// <summary>
		/// スレッドを閉じる
		/// </summary>
		public virtual void Close()
		{
			if (IsOpen)
			{
				Stop();

				ThreadIndexer.IncrementRefCount(Cache, headerInfo);
				OnClosed(new EventArgs());
			}

			IsOpen = false;
			headerInfo = null;
			resCollection.Clear();
		}

		/// <summary>
		/// 指定した1から始まるレス番号の配列をポップアップで表示
		/// </summary>
		/// <param name="indices"></param>
		public abstract void Popup(int[] indices);

		/// <summary>
		/// 指定したレスコレクションをポップアップで表示
		/// </summary>
		/// <param name="resSets"></param>
		public abstract void Popup(ResSetCollection resSets);

		/// <summary>
		/// 指定した index 番号を参照しているレスをポップアップで表示。
		/// </summary>
		/// <param name="index"></param>
		public abstract void PopupBackReferences(int index);

		/// <summary>
		/// 表示中のスレッドを印刷するように継承先でオーバーライド
		/// </summary>
		public abstract void Print();

		/// <summary>
		/// 指定した位置にしおりを挟むように継承先でオーバーライド。
		/// 既に同じ番号にしおりが設定されていたら、しおりを解除。
		/// </summary>
		/// <param name="shiroi"></param>
		public abstract void Bookmark(int shiroi);

		/// <summary>
		/// しおりを開く
		/// </summary>
		public abstract void OpenBookmark();

		/// <summary>
		/// 指定した sirusi 番号のレスに印を付ける。
		/// 同じ番号に印すると、印が解除される。
		/// </summary>
		/// <param name="sirusi"></param>
		public abstract void Sirusi(int sirusi, bool redraw);

		/// <summary>
		/// 印されたレスを表示
		/// </summary>
		public abstract void OpenSirusi();

		/// <summary>
		/// 指定した位置にスクロールするように継承先でオーバーライド
		/// </summary>
		/// <param name="position"></param>
		public abstract void ScrollTo(ScrollPosition position);

		/// <summary>
		/// 指定したレス番号までスクロールするように継承先でオーバーライド
		/// </summary>
		/// <param name="resNumber"></param>
		public abstract void ScrollTo(int resNumber);

		/// <summary>
		/// 指定した範囲のみを表示するように継承先でオーバーライド
		/// </summary>
		/// <param name="begin"></param>
		/// <param name="end"></param>
		public abstract void Range(int begin, int end);

		/// <summary>
		/// 指定した位置に移動するように継承先でオーバーライド
		/// </summary>
		/// <param name="movement"></param>
		public abstract void Range(RangeMovement movement);

		/// <summary>
		/// 表示を一端クリアして指定したitemsを書き込む
		/// </summary>
		/// <param name="items"></param>
		public abstract void WriteResColl(ResSetCollection items);

		/// <summary>
		/// 指定した文字列を書き込む
		/// </summary>
		/// <param name="html"></param>
		public abstract void WriteText(string text);

		/// <summary>
		/// 表示をクリア
		/// </summary>
		public abstract void Clear();

		/// <summary>
		/// スレッドを開きレスを抽出
		/// </summary>
		/// <param name="hezder"></param>
		/// <param name="info"></param>
		public abstract int OpenExtract(ThreadHeader hezder, ThreadExtractInfo info);

		/// <summary>
		/// 検索するためのクラスを初期化
		/// </summary>
		/// <param name="keyword"></param>
		/// <param name="flags"></param>
		/// <returns></returns>
		public abstract AbstractSearcher BeginSearch();

		/// <summary>
		/// 抽出するためのクラスを初期化
		/// </summary>
		/// <param name="keyword"></param>
		/// <param name="flags"></param>
		/// <param name="modePopup"></param>
		/// <returns></returns>
		public abstract AbstractExtractor BeginExtract();
	}

	/// <summary>
	/// 範囲設定時の位置を表す
	/// </summary>
	public enum RangeMovement
	{
		/// <summary>後ろへ移動</summary>
		Back,
		/// <summary>前へ移動</summary>
		Forward,
	}

	/// <summary>
	/// スクロールする位置を表す
	/// </summary>
	public enum ScrollPosition
	{
		/// <summary>一番上へスクロール</summary>
		Top,
		/// <summary>一番下へスクロール</summary>
		Bottom,
		/// <summary>一つ前の位置に戻る</summary>
		Prev,
	}
}
