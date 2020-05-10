// TraceOutput.cs

namespace Twin.Util
{
	using System;
	using System.IO;
	using System.Text;
	using System.Diagnostics;

	/// <summary>
	/// TraceやDebugをファイルに出力するための機能を提供
	/// </summary>
	public class TraceOutput : IDisposable
	{
		private TraceListener listener;

		/// <summary>
		/// TraceOutputクラスのインスタンスを初期化
		/// </summary>
		/// <param name="fileName"></param>
		public TraceOutput(string fileName)
		{
			if (fileName == null) {
				throw new ArgumentNullException("fileName");
			}

			listener = new TextWriterTraceListener(
				new StreamWriter(fileName, true, TwinDll.DefaultEncoding), typeof(TraceOutput).FullName);

			Trace.Listeners.Add(listener);
		}

		/// <summary>
		/// 使用しているリソースを解放
		/// </summary>
		public void Dispose()
		{
			listener.Close();
			listener.Dispose();
		}

		/// <summary>
		/// ストリームと閉じてトレース出力を受信しないようにする
		/// </summary>
		public void Close()
		{
			Dispose();
		}
	}
}
