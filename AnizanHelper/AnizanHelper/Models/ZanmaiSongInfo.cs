using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AnizanHelper.Models
{
	public class ZanmaiSongInfo
	{
		public int Number { get; set; }
		public string Title { get; set; }
		public string[] Artists { get; set; }
		public string Genre { get; set; }
		public string Series { get; set; }
		public string SongType { get; set; }

		public bool IsSpecialItem { get; set; }
		public string SpecialItemName { get; set; }
		public string SpecialHeader { get; set; }
		public string Additional { get; set; }
		public string ShortDescription { get; set; }


		[JsonExtensionData]
		private IDictionary<string, JToken> additionalData;

		[OnDeserialized]
		internal void OnDeserialized(StreamingContext context)
		{
			if (this.Artists == null && this.additionalData.TryGetValue("Singer", out var jToken))
			{
				this.Artists = ((string)jToken)
					?.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
					?? Array.Empty<string>();
			}
		}
	}
}
