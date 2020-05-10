// ErrorWriter.cs

namespace Twin.Util
{
	using System;
	using System.IO;
	using System.Text;
	using System.Diagnostics;
	using System.Windows.Forms;

	/// <summary>
	/// エラーをファイルに書き込む
	/// </summary>
	public class ErrorWriter
	{
		private int[] ticks;
		private string[] names;
		private int position;

		private readonly string ListenerName = DateTime.Now.Millisecond.ToString();
		private TraceListener listener;
		private string fileName;

		/// <summary>
		/// エラーを保存するファイル名
		/// </summary>
		public string FileName {
			set {
				SetListener(value);
			}
			get {
				return fileName;
			}
		}

		public ErrorWriter() : this(Application.StartupPath + "\\error.log")
		{
		}

		public ErrorWriter(string fileName)
		{
			position = 0;
			ticks = new int[256];
			names = new string[256];

			listener = null;
			SetListener(fileName);
		}

		~ErrorWriter()
		{
			Close();
		}

		private void SetListener(string fileName)
		{
			if (fileName == null) {
				throw new ArgumentNullException("fileName", "fileNameがnull参照です");
			}

			Close();

			this.fileName = fileName;
			listener = new TextWriterTraceListener(
				new StreamWriter(fileName, true, TwinDll.DefaultEncoding), ListenerName);

			Trace.Listeners.Add(listener);
		}

		[Conditional("DEBUG")]
		public void CountStart(string name)
		{
			lock (this)
			{
				ticks[position] = Environment.TickCount;
				names[position] = name;
				position++;
			}
		}

		[Conditional("DEBUG")]
		public void CountStop(bool msgBox)
		{
			lock (this)
			{
				position--;

				int count = Environment.TickCount - ticks[position];
				Write("{0}\t{1}ms", names[position], count);

				if (msgBox)
					MessageBox.Show(count.ToString() + "ms");
			}
		}

		/// <summary>
		/// 閉じる
		/// </summary>
		public void Close()
		{
			if (listener != null) {
				listener.Close();
				listener = null;
				Trace.Listeners.Remove(ListenerName);
			}
		}

		/// <summary>
		/// メッセージボックスを表示
		/// </summary>
		/// <param name="owner">オーナーウインドウ</param>
		/// <param name="message">表示するメッセージ</param>
		/// <param name="caption">キャプション</param>
		/// <param name="buttons">ボタン</param>
		/// <param name="icon">アイコン</param>
		/// <returns>押されたボタンの種類</returns>
		public DialogResult Show(IWin32Window owner, 
										string message, 
										string caption, 
										MessageBoxButtons buttons, 
										MessageBoxIcon icon)
		{
			Write(message);

			return MessageBox.Show(owner, message,
				caption, buttons, icon);
		}

		/// <summary>
		/// メッセージボックスを表示
		/// </summary>
		/// <param name="message">表示するメッセージ</param>
		public void Show(string message)
		{
			Show(null, message, "アプリケーションエラー",
				MessageBoxButtons.OK, MessageBoxIcon.Warning);
		}

		/// <summary>
		/// メッセージを書式化して表示
		/// </summary>
		/// <param name="format">書式</param>
		/// <param name="arguments">引数</param>
		public void Show(string format, params object[] arguments)
		{
			string text = String.Format(format, arguments);
			Show(text);
		}

		/// <summary>
		/// メッセージボックスを表示
		/// </summary>
		/// <param name="owner">オーナーウインドウ</param>
		/// <param name="message">表示するメッセージ</param>
		public void Show(IWin32Window owner, string message)
		{
			Show(owner, message, "アプリケーションエラー",
				MessageBoxButtons.OK, MessageBoxIcon.Warning);
		}

		/// <summary>
		/// メッセージボックスで例外を表示
		/// </summary>
		/// <param name="ex">表示する例外</param>
		public void Show(Exception ex)
		{
			Show(null, ex);
		}

		/// <summary>
		/// メッセージボックスを表示
		/// </summary>
		/// <param name="owner">オーナーウインドウ</param>
		/// <param name="ex">表示する例外</param>
		public void Show(IWin32Window owner, Exception ex)
		{
			Write(ex.ToString());

			MessageBox.Show(owner, ex.Message, "例外が発生しました",
				MessageBoxButtons.OK, MessageBoxIcon.Warning);
		}

		/// <summary>
		/// メッセージをファイルに書き込む
		/// </summary>
		/// <param name="message">書き込むメッセージ</param>
		public void Write(string message)
		{
			listener.WriteLine(String.Format("ver{0} ({1})", TwinDll.Version, DateTime.Now));
			listener.WriteLine(String.Format("{0} CLR {1}", Environment.OSVersion, Environment.Version));
			listener.WriteLine(message);
			listener.WriteLine(String.Empty);
		}

		/// <summary>
		/// 文字列を書式化して書き込む
		/// </summary>
		/// <param name="format">書式</param>
		/// <param name="arguments">引数</param>
		public void Write(string format, params object[] arguments)
		{
			Write(String.Format(format, arguments));
		}

		/// <summary>
		/// オブジェクトを文字列に変換して書き込む
		/// </summary>
		/// <param name="obj">書き込むオブジェクト</param>
		public void Write(object obj)
		{
			Write(obj.ToString());
		}
	}
}
