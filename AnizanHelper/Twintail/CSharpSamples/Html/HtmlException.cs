// HtmlException.cs

namespace CSharpSamples.Html
{
	using System;

	/// <summary>
	/// HtmlException の概要の説明です。
	/// </summary>
	public class HtmlException : ApplicationException
	{
		/// <summary>
		/// HtmlExceptionクラスのインスタンスを初期化
		/// </summary>
		public HtmlException() : base()
		{
			// 
			// TODO: コンストラクタ ロジックをここに追加してください。
			//
		}

		/// <summary>
		/// HtmlExceptionクラスのインスタンスを初期化
		/// </summary>
		/// <param name="message"></param>
		public HtmlException(string message) : base(message)
		{
		}

		/// <summary>
		/// HtmlExceptionクラスのインスタンスを初期化
		/// </summary>
		/// <param name="message"></param>
		/// <param name="exception"></param>
		public HtmlException(string message, Exception exception)
			: base(message, exception)
		{
		}
	}
}
