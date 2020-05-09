using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace AnizanHelper.Models.Updating
{
	public class WebUpdateInfoRetreiver : IUpdateInfoRetreiver
	{
		private HttpClient HttpClient { get; }
		private Uri UpdateInfoUri { get; }

		public WebUpdateInfoRetreiver(HttpClient httpClient, Uri updateInfoUri)
		{
			this.HttpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
			this.UpdateInfoUri = updateInfoUri ?? throw new ArgumentNullException(nameof(updateInfoUri));
		}

		public async Task<UpdateInfo> GetUpdateInfoAsync()
		{
			var updateInfoJson = await this.HttpClient.GetStringAsync(this.UpdateInfoUri).ConfigureAwait(false);
			return JsonConvert.DeserializeObject<UpdateInfo>(updateInfoJson);
		}
	}
}
