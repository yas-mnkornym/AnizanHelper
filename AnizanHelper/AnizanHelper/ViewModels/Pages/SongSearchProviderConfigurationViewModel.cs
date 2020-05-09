using System;
using System.Linq;
using System.Reactive.Linq;
using AnizanHelper.Models;
using AnizanHelper.Models.Searching;
using AnizanHelper.Models.Searching.AnisonDb;
using AnizanHelper.Models.Searching.Zanmai;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using Studiotaiha.LazyProperty;

namespace AnizanHelper.ViewModels.Pages
{
	internal class SongSearchProviderConfigurationViewModel : ReactiveViewModelBase
	{
		public ISongSearchProvider SongSearchProvider { get; }
		public Settings Settings { get; }

		public SongSearchProviderConfigurationViewModel(
			ISongSearchProvider songSearchProvider,
			Settings settings)
		{
			this.SongSearchProvider = songSearchProvider ?? throw new ArgumentNullException(nameof(songSearchProvider));
			this.Settings = settings ?? throw new ArgumentNullException(nameof(settings));
		}

		#region Bindings

		public ReactiveProperty<string> Name => this.LazyReactiveProperty(this.SongSearchProvider.Id switch
		{
			nameof(AnisonDbSongNameSearchProvider) => "アニソンDB",
			nameof(ZanmaiWikiSearchProvider) => "三昧さーち",
			_ => this.SongSearchProvider.Id,
		});

		public ReactiveProperty<bool> IsEnabled => this.LazyReactiveProperty<bool>(() =>
		{
			var property = this.Settings
				.PropertyChangedAsObservable(nameof(this.Settings.DisabledSearchProviders))
				.Select(x => this.Settings.DisabledSearchProviders)
				.Select(x => x?.Contains(this.SongSearchProvider.Id) == false)
				.ToReactiveProperty(this.Settings.DisabledSearchProviders?.Contains(this.SongSearchProvider.Id) == false);

			property
				.Subscribe(value =>
				{
					var shouldBeDisabled = !value;
					if (this.Settings.DisabledSearchProviders?.Contains(this.SongSearchProvider.Id) != shouldBeDisabled)
					{
						if (shouldBeDisabled)
						{
							this.Settings.DisabledSearchProviders = (this.Settings.DisabledSearchProviders ?? Array.Empty<string>())
								.Append(this.SongSearchProvider.Id)
								.ToArray();
						}
						else
						{
							this.Settings.DisabledSearchProviders = (this.Settings.DisabledSearchProviders ?? Array.Empty<string>())
								.Except(new string[] { this.SongSearchProvider.Id })
								.ToArray();
						}
					}
				})
				.AddTo(this.Disposables);

			return property;
		});

		#endregion Bindings
	}
}
