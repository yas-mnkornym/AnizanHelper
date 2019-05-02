using Studiotaiha.Toolkit;
using System;
using System.Text;
using System.Windows;

namespace AnizanHelper.ViewModels
{
	public class ViewModelBase : NotificationObjectWithPropertyBag
	{
		public ViewModelBase(IDispatcher dispatcher = null) : base(dispatcher)
		{
		}

		public ViewModelBase(IDispatcher dispatcher, bool enableAutoDispatch) : base(dispatcher, enableAutoDispatch)
		{
		}

		protected void ShowErrorMessage(string message, Exception ex = null)
		{
			var sb = new StringBuilder();
			sb.AppendLine("クリップボードからの情報取得に失敗しました。");
			sb.AppendLine(ex.Message);

			if (ex != null)
			{
				sb.AppendLine();
				sb.AppendLine("【例外情報】");
				sb.AppendLine(ex.ToString());
			}

			MessageBox.Show(sb.ToString(), "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
		}
	}
}
