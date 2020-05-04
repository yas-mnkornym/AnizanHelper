using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AnizanHelper.Models.SongList
{
	public class IcecastSongMetadataRetreiver : ISongMetadataRetreiver
	{
		DateTimeOffset currentTimestamp;
		Guid currentMetadataId;
		private byte[] currentMetadataBuffer;

		private Encoding metadataEncoding;

		public IcecastSongMetadataRetreiver(HttpClient httpClient, Uri streamUri, Encoding metadataEncoding)
		{
			this.HttpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
			this.StreamUri = streamUri ?? throw new ArgumentNullException(nameof(streamUri));
			this.MetadataEncoding = metadataEncoding ?? throw new ArgumentNullException(nameof(metadataEncoding));
		}

		public Encoding MetadataEncoding
		{
			get => metadataEncoding;
			set
			{
				if (this.metadataEncoding != value)
				{
					this.metadataEncoding = value;
					this.DecodeAndNotifyMetadata();
				}
			}
		}

		private HttpClient HttpClient { get; }
		private Uri StreamUri { get; }

		public async Task<SongListFeedStopStatus> RunAsync(CancellationToken cancellationToken = default)
		{
			using (var request = new HttpRequestMessage(HttpMethod.Get, this.StreamUri))
			{
				request.Headers.Add("Icy-Metadata", "1");

				using (var response = await this.HttpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false))
				{
					response.EnsureSuccessStatusCode();

					var metaInterval = response.Headers.TryGetValues("icy-metaint", out var values)
						? int.TryParse(values.FirstOrDefault() ?? string.Empty, out var parsedValue)
							? parsedValue
							: 0
						: 0;

					if (metaInterval == 0)
					{
						return SongListFeedStopStatus.MetadataNotSupported;
					}

					using (var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false))
					{
						while (true)
						{
							cancellationToken.ThrowIfCancellationRequested();

							if (await stream.ReadBytesOrDefaultAsync(metaInterval, cancellationToken).ConfigureAwait(false) == null)
							{
								return SongListFeedStopStatus.StreamClosed;
							}

							var metadataLengthByte = stream.ReadByte();
							if (metadataLengthByte == -1)
							{
								return SongListFeedStopStatus.StreamClosed;
							}

							var metadataLength = metadataLengthByte * 16;
							if (metadataLength > 0)
							{
								var metadataBuffer = await stream.ReadBytesOrDefaultAsync(metadataLength, cancellationToken).ConfigureAwait(false);
								if (metadataBuffer == null)
								{
									return SongListFeedStopStatus.StreamClosed;
								}

								this.currentMetadataBuffer = metadataBuffer;
								this.currentMetadataId = Guid.NewGuid();
								this.currentTimestamp = DateTimeOffset.Now;
								this.DecodeAndNotifyMetadata();
							}
						}
					}
				}
			}
		}

		private void DecodeAndNotifyMetadata()
		{
			if (this.currentMetadataBuffer != null && this.MetadataEncoding != null)
			{
				try
				{
					var metadataString = this.MetadataEncoding.GetString(this.currentMetadataBuffer);
					var songMetadata = SongMetadata.Parse(metadataString, this.currentMetadataId, currentTimestamp);

					this.SongMetadataReceived?.Invoke(this, songMetadata);
				}
				catch (Exception ex)
				{
					this.ParseFailed?.Invoke(this, ex);
				}
			}
		}


		public event EventHandler<Exception> ParseFailed;

		public event EventHandler<ISongMetadata> SongMetadataReceived;
	}
}
