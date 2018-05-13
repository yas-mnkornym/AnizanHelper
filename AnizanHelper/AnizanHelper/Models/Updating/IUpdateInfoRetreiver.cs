using System.Threading.Tasks;

namespace AnizanHelper.Models.Updating
{
	public interface IUpdateInfoRetreiver
	{
		Task<UpdateInfo> GetUpdateInfoAsync();
	}
}
