using System.Threading;
using System.Threading.Tasks;

namespace AnizanHelper.Modules.Dictionaries
{
	public interface IDictionaryUpdaterService
	{
		Task CheckForUpdateAsync(CancellationToken cancellationToken = default);
	}
}
