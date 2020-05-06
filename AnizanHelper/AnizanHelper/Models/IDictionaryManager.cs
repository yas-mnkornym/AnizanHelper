using System.Threading;
using System.Threading.Tasks;

namespace AnizanHelper.Models
{
	public interface IDictionaryManager
	{
		string[] DictionaryFilePaths { get; }
		Task LoadAsync(CancellationToken cancellationToken = default);
	}
}
