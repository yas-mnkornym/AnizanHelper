using System;
using System.Threading.Tasks;

namespace AnizanHelper.Models.Updating
{
	public class DummyUpdateInfoRetreiver : IUpdateInfoRetreiver
	{
		public Version Version { get; }

		public DummyUpdateInfoRetreiver(Version version)
		{
			this.Version = version ?? throw new ArgumentNullException(nameof(version));
		}

		public Task<UpdateInfo> GetUpdateInfoAsync()
		{
			return Task.FromResult(new UpdateInfo
			{
				Version = Version,
			});
		}
	}
}
