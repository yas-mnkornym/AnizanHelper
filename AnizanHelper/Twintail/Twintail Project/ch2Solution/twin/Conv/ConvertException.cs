// ConvertException.cs

namespace Twin.Conv
{
	using System;

	/// <summary>
	/// ログの変換に失敗したときに発生する例外
	/// </summary>
	public class ConvertException : ApplicationException
	{
		/// <summary>
		/// ConvertExceptionクラスのインスタンスを初期化
		/// </summary>
		public ConvertException() : base()
		{
			// 
			// TODO: コンストラクタ ロジックをここに追加してください。
			//
		}

		/// <summary>
		/// ConvertExceptionクラスのインスタンスを初期化
		/// </summary>
		public ConvertException(string message) : base(message)
		{
			// 
			// TODO: コンストラクタ ロジックをここに追加してください。
			//
		}

		/// <summary>
		/// ConvertExceptionクラスのインスタンスを初期化
		/// </summary>
		public ConvertException(string message, Exception innerException) 
			: base(message, innerException)
		{
			// 
			// TODO: コンストラクタ ロジックをここに追加してください。
			//
		} 
	}
}
