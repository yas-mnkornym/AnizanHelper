using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AnizanHelper.Models.SettingComponents
{
	public sealed class SettingChangeEventArgs : EventArgs
	{
		public SettingChangeEventArgs(
			string key,
			object oldValue,
			object newValue)
		{
			Key = key;
			OldValue = oldValue;
			NewValue = newValue;
		}

		/// <summary>
		/// 変更された設定のキーを取得する。
		/// </summary>
		public string Key { get; private set; }

		/// <summary>
		/// 変更される前の値を取得する。
		/// </summary>
		public object OldValue { get; private set; }

		/// <summary>
		/// 変更された後の値を取得する。
		/// </summary>
		public object NewValue { get; private set; }
	}
}
