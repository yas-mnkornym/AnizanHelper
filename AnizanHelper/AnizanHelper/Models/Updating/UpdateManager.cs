using System;
using System.Threading.Tasks;

namespace AnizanHelper.Models.Updating
{
	public class UpdateManager : IUpdateManager
	{
		public Version CurrentVersion { get; } = typeof(UpdateManager).Assembly.GetName().Version;

		public UpdateInfo UpdateInfo { get; private set; }
		public bool IsUpdateAvailable => this.UpdateInfo?.Version > this.CurrentVersion == true;

		private IUpdateInfoRetreiver UpdateInfoRetreiver { get; }

		public UpdateManager(IUpdateInfoRetreiver updateInfoRetreiver)
		{
			this.UpdateInfoRetreiver = updateInfoRetreiver ?? throw new ArgumentNullException(nameof(updateInfoRetreiver));
		}

		public async Task CheckForUpdateAsync()
		{
			this.UpdateInfo = await this.UpdateInfoRetreiver.GetUpdateInfoAsync().ConfigureAwait(false);
		}
	}
}
