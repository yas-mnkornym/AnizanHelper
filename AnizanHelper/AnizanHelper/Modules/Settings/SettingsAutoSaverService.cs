using System;
using System.IO;
using System.Reactive.Disposables;
using AnizanHelper.Models.SettingComponents;
using AnizanHelper.Services;
using Reactive.Bindings.Extensions;

namespace AnizanHelper.Modules.Settings
{
	internal class SettingsAutoSaverService : ReactiveServiceBase
	{
		private SettingsContainer SettingsContainer { get; }
		private ISettingsSerializer SettingsSerializer { get; }
		private TimeSpan ThrottlingInterval { get; }

		public SettingsAutoSaverService(SettingsContainer settingsContainer, ISettingsSerializer settingsSerializer, TimeSpan throttlingInterval)
		{
			this.SettingsContainer = settingsContainer ?? throw new ArgumentNullException(nameof(settingsContainer));
			this.SettingsSerializer = settingsSerializer ?? throw new ArgumentNullException(nameof(settingsSerializer));
			this.ThrottlingInterval = throttlingInterval;
		}

		protected override void RegisterDisposables(CompositeDisposable disposables)
		{
			new SettingsAutoExpoter(
				Path.Combine(AppInfo.Current.StartupDirectory, Constants.SettingsFileName),
				Path.Combine(AppInfo.Current.StartupDirectory, Constants.SettingsTempFileName),
				this.SettingsContainer,
				this.SettingsSerializer,
				this.ThrottlingInterval)
				.AddTo(disposables);
		}
	}
}
