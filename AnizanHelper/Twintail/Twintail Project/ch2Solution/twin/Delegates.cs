// Delegates.cs

namespace Twin
{
	using System;
	using System.Collections.Generic;

	// デリゲートの定義
	internal delegate void WriteResMethodInvoker(ResSetCollection items);
	internal delegate void WriteListMethodInvoker(List<ThreadHeader> items);
	internal delegate void LoadingMethodInvoker(EventArgs e);
	internal delegate void ReceiveMethodInvoker(ReceiveEventArgs e);
	internal delegate void CompleteMethodInvoker(CompleteEventArgs e);
	internal delegate void PositionMethodInvoker(float value);
}
