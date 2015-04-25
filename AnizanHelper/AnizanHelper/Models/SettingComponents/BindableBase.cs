using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AnizanHelper.Models.SettingComponents
{
	public class BindableBase : NotificationObject
	{
		public BindableBase(IDispatcher dispatcher = null)
			: base(dispatcher)
		{ }

		/// <summary>
		/// プロパティの値を設定する。
		/// </summary>
		/// <typeparam name="T">変数の型</typeparam>
		/// <param name="dst">設定する対象の変数への参照</param>
		/// <param name="value">設定する値</param>
		/// <param name="propertyName">プロパティ名</param>
		protected virtual bool SetValue<T>(ref T dst, T value, string propertyName)
		{
			bool isChanged = false;
			if (dst == null) {
				isChanged = (value != null);
			}
			else {
				isChanged = !dst.Equals(value);
			}

			if (isChanged) {
				RaisePropertyChanging(propertyName);
				dst = value;
				RaisePropertyChanged(propertyName);
				return true;
			}
			return false;
		}

		/// <summary>
		/// プロパティの値を設定する。
		/// </summary>
		/// <typeparam name="T">変数の型</typeparam>
		/// <param name="dst">設定する対象の変数への参照</param>
		/// <param name="value">設定する値</param>
		/// <param name="actBeforeChange">値が変更される前に実行する処理</param>
		/// <param name="actAfterChange">値が変更された後に実行される処理</param>
		/// <param name="propertyName">プロパティ名</param>
		/// <returns>値が変更されたらtrue, されなければfalse</returns>
		/// <remarks>actBeforeChange, actAfterChangeは、値が変更される時のみ実行される。</remarks>
		protected virtual bool SetValue<T>(
			ref T dst,
			T value,
			Action<T> actBeforeChange,
			Action<T> actAfterChange,
			string propertyName)
		{
			bool isChanged = false;
			if (dst == null) {
				isChanged = (value != null);
			}
			else {
				isChanged = !dst.Equals(value);
			}

			if (isChanged) {
				RaisePropertyChanging(propertyName);
				if (actBeforeChange != null) {
					actBeforeChange(dst);
				}
				dst = value;
				if (actAfterChange != null) {
					actAfterChange(dst);
				}
				RaisePropertyChanged(propertyName);
				return true;
			}
			return false;
		}
	}
}
