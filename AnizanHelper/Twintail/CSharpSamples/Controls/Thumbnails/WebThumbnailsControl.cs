using System.IO;
using System.Net;

namespace CSharpSamples
{
	public class WebThumbnailsControl : ThumbnailsControl
	{
		public string CacheDataFolderPath { get; set; }

		public WebThumbnailsControl()
		{
			this.CacheDataFolderPath = "";
		}

		protected override byte[] LoadData(string uri)
		{
			if (uri.StartsWith("http://"))
			{
				try
				{
					string localPath = Path.Combine(this.CacheDataFolderPath, string.Format("{0:x}.ich", uri.GetHashCode()));
					if (!File.Exists(localPath))
					{
						using (WebClient w = new WebClient())
						{
							if (Directory.Exists(this.CacheDataFolderPath))
							{
								w.DownloadFile(uri, localPath);
								return File.ReadAllBytes(localPath);
							}
							else
							{
								return w.DownloadData(uri);
							}
						}
					}
				}
				catch { }
			}
			else if (File.Exists(uri))
			{
				return base.LoadData(uri);
			}

			return null;
		}

	}
}
