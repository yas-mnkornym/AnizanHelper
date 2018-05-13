using System;
using System.Threading.Tasks;

namespace AnizanHelper.Models.Updating
{
	public interface IUpdateManager
	{
		Version CurrentVersion { get; }
		bool IsUpdateAvailable { get; }
		UpdateInfo UpdateInfo { get; }

		Task CheckForUpdateAsync();
	}
}