using System;
using System.Collections.Generic;
using System.Text;
using Twin.IO;
using Twin.Bbs;
using System.IO;

namespace Twin
{
	public delegate void DatIndexingCallback(float percentage);

	public class CreateDatIndexer
	{
		public void Indexing(BoardInfo bi, string[] datFiles, DatIndexingCallback callback)
		{
			LocalThreadReader reader = new LocalThreadReader(new X2chThreadParser());
			float length = datFiles.Length, current = 0;

			if (length == 0)
				return;

			foreach (string path in datFiles)
			{
				ThreadHeader header = new X2chThreadHeader();
				header.BoardInfo = bi;
				header.Key = Path.GetFileNameWithoutExtension(path);
				int parsed;

				try
				{
					if (!reader.__Open(path))
						continue;

					ResSetCollection buffer = new ResSetCollection();
					int byteCount = 0;

					while (reader.Read(buffer, out parsed) != 0)
						byteCount += parsed;

					if (buffer.Count > 0)
					{
						header.GotByteCount = byteCount;
						header.ResCount = buffer.Count;
						header.GotResCount = buffer.Count;
						header.LastModified = File.GetLastWriteTime(path);
						header.UseGzip = false;
						header.Subject = buffer[0].Tag as String;

						ThreadIndexer.Write(
							Path.ChangeExtension(path, ".idx"), header);
					}

					if (callback != null)
						callback(++current / length * 100f);
				}
				finally
				{
					reader.Close();
				}
			}
		}
	}
}
