using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace AnizanHelper.Models.SongList
{
	public static class StreamExtensions
	{
		public static async Task<byte[]> ReadBytesOrDefaultAsync(
			this Stream stream,
			int length,
			CancellationToken cancellationToken = default)
		{
			var buffer = new byte[length];

			for (var remaining = length; remaining > 0;)
			{
				var bytesRead = await stream.ReadAsync(buffer, 0, remaining).ConfigureAwait(false);
				if (bytesRead == 0)
				{
					return null;
				}

				remaining -= bytesRead;
			}

			return buffer;
		}
	}
}
