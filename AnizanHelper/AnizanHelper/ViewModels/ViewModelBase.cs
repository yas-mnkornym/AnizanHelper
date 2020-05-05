using System;
using System.Text;
using System.Windows;
using Studiotaiha.LazyProperty;
using Studiotaiha.Toolkit;

namespace AnizanHelper.ViewModels
{
	public class ViewModelBase : NotificationObjectWithPropertyBag, ILazyPropertyHolder
	{
		public ViewModelBase(IDispatcher dispatcher = null) : base(dispatcher)
		{
		}

		public ViewModelBase(IDispatcher dispatcher, bool enableAutoDispatch) : base(dispatcher, enableAutoDispatch)
		{
		}

		public TValue GetOrCreateValue<TValue>(string propertyName, Func<TValue> valueProvider)
		{
			if (propertyName == null) { throw new ArgumentNullException(nameof(propertyName)); }
			if (valueProvider == null) { throw new ArgumentNullException(nameof(valueProvider)); }

			if (!(this.PropertyBag.TryGetValue(propertyName, out var value) && value is TValue result))
			{
				this.PropertyBag[propertyName] = result = valueProvider.Invoke();
			}

			return result;
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
