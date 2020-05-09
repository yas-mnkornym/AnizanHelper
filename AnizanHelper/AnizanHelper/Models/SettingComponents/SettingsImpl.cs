using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace AnizanHelper.Models.SettingComponents
{
	internal class SettingsContainer : ISettingsContainer
	{
		#region Private Field
		private List<Type> knownTypes_ = new List<Type>();
		internal Dictionary<string, object> SettingsData { get; } = new Dictionary<string, object>(); // 設定データ
		private Dictionary<string, SettingsContainer> ChildSettings { get; } = new Dictionary<string, SettingsContainer>(); // 子設定
		private ISettingsSerializer ChildSettingsSerializer { get; } = new DataContractSettingsSerializer(); // 子設定のシリアライザ 
		#endregion


		#region Properties

		protected SettingsContainer ParentSettings { get; private set; }
		#endregion

		#region Constructors
		public SettingsContainer(string tag)
		{
			this.Tag = tag;
		}

		public SettingsContainer(IEnumerable<Type> knownTypes)
			: this(null, knownTypes)
		{ }

		public SettingsContainer(string tag, IEnumerable<Type> knownTypes)
			: this(tag)
		{
			if (knownTypes != null)
			{
				this.knownTypes_.AddRange(knownTypes);
			}
		}

		protected SettingsContainer(SettingsContainer parentSettings, string tag, IEnumerable<Type> knownTypes)
			: this(tag, knownTypes)
		{
			if (parentSettings == null) { throw new ArgumentNullException("parentSettings"); }
			this.ParentSettings = parentSettings;

			// タグ編集
			List<string> tags = new List<string>();
			var settings = this;
			while (settings != null)
			{
				tags.Add(settings.Tag);
				settings = settings.ParentSettings;
			}
			tags.Reverse();
			this.Tag = string.Join("_", tags);
		}

		#endregion

		#region ISettings メンバ
		public IList<Type> KnownTypes
		{
			get
			{
				return this.knownTypes_;
			}
		}

		public string Tag { get; private set; }

		public void Set<T>(string key, T value)
		{
			var actKey = key;
			bool isNew = true;
			object oldValue = null;

			if (this.SettingsData.ContainsKey(actKey))
			{
				oldValue = this.SettingsData[actKey];
				if (oldValue != null)
				{
					isNew = (!oldValue.Equals(value));
				}
				else
				{
					isNew = (value != null);
				}
			}

			if (isNew)
			{
				var args = new SettingChangeEventArgs(key, oldValue, value);
				this.OnSettingChanging(args);
				this.SettingsData[actKey] = value;
				this.OnSettingChanged(args);
			}
		}

		public T Get<T>(string key, T defaultValue = default(T))
		{
			return this.SettingsData.TryGetValue(key, out var value) && value is T
				? (T)value
				: defaultValue;
		}

		public void SetCrypted<T>(string key, T value)
		{
			throw new NotImplementedException();
		}

		public T GetDecrypted<T>(string key, T defaultValue = default(T))
		{
			throw new NotImplementedException();
		}

		public bool Exists(string key)
		{
			var actKey = key;
			return this.SettingsData.ContainsKey(actKey);
		}

		public void Remove(string key)
		{
			var actKey = key; // GetTaggedKey(key);
			if (this.SettingsData.ContainsKey(actKey))
			{
				var args = new SettingChangeEventArgs(key, this.SettingsData[actKey], null);
				this.OnSettingChanging(args);
				this.SettingsData.Remove(actKey);
				this.OnSettingChanged(args);
			}
		}

		public void Clear()
		{
			foreach (var key in this.SettingsData.Keys.ToArray())
			{
				this.Remove(key);
			}
		}

		public IEnumerable<string> Keys
		{
			get
			{
				return this.SettingsData.Keys;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:オブジェクトを複数回破棄しない")]
		public ISettingsContainer GetChildSettings(string tag, IEnumerable<Type> knownTypes)
		{
			if (this.ChildSettings.ContainsKey(tag))
			{
				return this.ChildSettings[tag];
			}
			else
			{
				var settings = new SettingsContainer(this, tag, knownTypes);

				// 設定をロード
				var setTag = this.GetTaggedKey(settings.Tag, true);
				var setStr = this.Get<string>(setTag, null);
				if (setStr != null)
				{
					using (var ms = new MemoryStream())
					using (var writer = new StreamWriter(ms, Encoding.UTF8))
					{
						writer.Write(setStr);
						writer.Flush();
						ms.Seek(0, SeekOrigin.Begin);
						this.ChildSettingsSerializer.Deserialize(ms, settings);
					}
				}

				// 子設定に追加
				settings.SettingChanged += this.settings_SettingChanged;
				this.ChildSettings[settings.Tag] = settings;
				return settings;
			}
		}

		/// <summary>
		/// 子設定が変更されたとき、その値を保存
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void settings_SettingChanged(object sender, SettingChangeEventArgs e)
		{
			var settings = sender as SettingsContainer;
			if (settings != null)
			{
				var tag = this.GetTaggedKey(settings.Tag, true);
				string str = null;
				using (var ms = new MemoryStream())
				{
					this.ChildSettingsSerializer.Serialize(ms, settings);
					ms.Seek(0, SeekOrigin.Begin);
					using (var reader = new StreamReader(ms))
					{
						str = reader.ReadToEnd();
					}
				}

				// 設定保存
				this.Set(tag, str);
			}
		}

		public void RemoveChildSettings(string tag)
		{
			if (this.ChildSettings.ContainsKey(tag))
			{
				var settings = this.ChildSettings[tag];
				this.ChildSettings[tag] = null;
				settings.SettingChanged -= this.settings_SettingChanged;
			}
		}

		public IEnumerable<string> ChildSettingsTags
		{
			get
			{
				return this.ChildSettings.Keys;
			}
		}

		public event EventHandler<SettingChangeEventArgs> SettingChanging;

		public event EventHandler<SettingChangeEventArgs> SettingChanged;
		#endregion // ISettings メンバ

		#region Private Methods
		private string GetTaggedKey(string key, bool isEmbed = false)
		{
			string result;
			if (this.Tag == null)
			{
				result = key;
			}
			else
			{
				result = string.Format("{0}.{1}", this.Tag, key);
			}

			if (isEmbed)
			{
				return "__" + result;
			}
			else
			{
				return result;
			}
		}

		private void OnSettingChanging(SettingChangeEventArgs args)
		{
			if (SettingChanging != null)
			{
				SettingChanging(this, args);
			}
		}

		private void OnSettingChanged(SettingChangeEventArgs args)
		{
			if (SettingChanged != null)
			{
				SettingChanged(this, args);
			}
		}
		#endregion
	}
}
