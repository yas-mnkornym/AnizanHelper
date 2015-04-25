// PlaySound.cs

namespace CSharpSamples
{
	using System;
	using System.Runtime.InteropServices;

	// PlaySound関数
	// ms-help://MS.VSCC/MS.MSDNVS.1041/jpmltimd/html/_win32_playsound.htm

	/// /// <summary>
	/// サウンドを再生するクラス
	/// </summary>
	[Obsolete("System.Media.SoundPlayer クラスを使用してください。")]
	public class SoundPlayer
	{
		/// /// <summary>
		/// サウンドを再生
		/// </summary>
		/// <param name="fileName">再生するサウンドのファイル名</param>
		/// <param name="flags">再生フラグ</param>
		/// <returns>再生に成功したらtrue、失敗したらfalse</returns>
		public static bool Play(string fileName, SoundFlags flags)
		{
			return PlaySound(fileName, IntPtr.Zero, (ulong)flags) != 0 ? true : false;
		}

		[DllImport("winmm.dll")]
		private static extern int PlaySound(string pszSound, IntPtr handle, ulong fdwSound);
	}

	/// /// <summary>
	/// PlaySoundのフラグ
	/// </summary>
	[Flags]
	public enum SoundFlags : ulong
	{
		/// <summary> サウンドイベントを同期再生します。PlaySound 関数は、サウンドの再生が完了した後で制御を返します。 (default) </summary>
		Sync = 0x0000, 
		/// <summary> サウンドを非同期再生し、サウンドが開始されると、PlaySound 関数は即座に制御を返します。非同期再生されているサウンドを停止するには、pszSound パラメータで NULL を指定して PlaySound 関数を呼び出してください。 </summary>
		Async = 0x0001,  
		/// <summary>既定のサウンドイベントを使いません。指定されたサウンドが見つからなかった場合、PlaySound 関数は、既定のサウンド（一般の警告音）を再生せずに静かに制御を返します。 </summary>
		NoDefault = 0x0002, 
		/// <summary> サウンドイベントのファイルは、メモリ内に既にロードされています。pszSound パラメータは、メモリ内のサウンドイメージへのポインタを表します。  </summary>
		Memory = 0x0004,  
		/// <summary> サウンドを繰り返し再生します。pszSound パラメータで NULL を指定して PlaySound 関数を呼び出すと、サウンドが停止します。サウンドイベントを非同期再生するよう指示するために、SND_ASYNC と同時に指定しなければなりません。  </summary>
		Loop = 0x0008,  
		/// <summary> 既にほかのサウンドが再生されている場合、指定されたサウンドを再生しません。指定されたサウンドを再生するために必要なリソースが、ほかのサウンドを再生していてビジーであり、指定されたサウンドを再生できない場合、この関数は指定されたサウンドを再生せずに、即座に FALSE を返します。  </summary>
		NoStop = 0x0010,  
		/// <summary> 呼び出し側タスクに関係するサウンドの再生を停止します。pszSound パラメータが NULL ではない場合、指定したサウンドのすべてのインスタンスを停止します。pszSound パラメータが NULL の場合、呼び出し側タスクに関係するすべてのサウンドを停止します。  </summary>
		Purge = 0x0040,  
		/// <summary> アプリケーション特有の関連付けを使ってサウンドを再生します。 </summary>
		Application = 0x0080,  
		/// <summary> ドライバがビジー状態の場合、指定されたサウンドを再生せずに即座に制御を返します。  </summary>
		NoWait = 0x00002000L, 
		/// <summary> pszSound パラメータは、レジストリまたは WIN.INI ファイルに記述されているシステムイベントの別名（エイリアス）です。SND_FILENAME や SND_RESOURCE と同時に指定することはできません。  </summary>
		Alias = 0x00010000L,
		/// <summary> pszSound パラメータは、定義済みのサウンド識別子（"SystemStart"、"SystemExit" など）です。  </summary>
		AliasID = 0x00110000L, 
		/// <summary> pszSound パラメータは、ファイル名を表します </summary>
		FileName = 0x00020000L, 
		/// <summary> パラメータで指定したサウンドイベントを停止させる場合は、hmod パラメータでインスタンスハンドルも指定しなければなりません。   </summary>
		Resource = 0x00040004L,
	}
}
