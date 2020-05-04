using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace AnizanHelper.Models.SongList
{
	internal class SongMetadata : ReadOnlyDictionary<string, string>, ISongMetadata
	{
		public SongMetadata(IEnumerable<KeyValuePair<string, string>> pairs)
			: this(pairs?.ToDictionary(x => x.Key, x => x.Value) ?? throw new ArgumentNullException(nameof(pairs)))
		{
		}

		public SongMetadata(IDictionary<string, string> dictionary) : base(dictionary)
		{
		}

		public Guid Id { get; set; }

		public DateTimeOffset Timestamp { get; set; }

		public string StreamTitle => this.GetPropertyValueOrDefault();

		private string GetPropertyValueOrDefault(string defaultValue = null, [CallerMemberName]string key = null)
		{
			if (key == null) { throw new ArgumentNullException(nameof(key)); }

			return this.TryGetValue(key, out var value)
				? value
				: defaultValue;
		}

		public static SongMetadata Parse(string iceCastMetadataString, Guid itemId, DateTimeOffset timestamp)
		{
			if (iceCastMetadataString == null) { throw new ArgumentNullException(nameof(iceCastMetadataString)); }

			var pairs = iceCastMetadataString
				.Zip(iceCastMetadataString.Skip(1), (x, y) => (current: x, next: y));

			var inText = false;
			var properties = new List<KeyValuePair<string, string>>();

			string propertyName = null;
			string propertyValue = null;
			var sb = new StringBuilder();
			var shouldSkipNext = false;

			foreach (var (current, next) in pairs)
			{
				if (shouldSkipNext)
				{
					shouldSkipNext = false;
					continue;
				}

				switch (current)
				{
					case '\'':
						inText = !inText;
						break;

					case '\\':
						sb.Append(next);
						shouldSkipNext = true;
						break;

					case '=':
						propertyName = sb.ToString();
						sb.Clear();
						break;

					case ';':
						propertyValue = sb.ToString();
						sb.Clear();
						properties.Add(new KeyValuePair<string, string>(propertyName, propertyValue));
						propertyName = null;
						propertyValue = null;
						break;

					default:
						sb.Append(current);
						break;
				}
			}

			if (!string.IsNullOrWhiteSpace(propertyName))
			{
				properties.Add(new KeyValuePair<string, string>(propertyName, propertyValue ?? sb.ToString()));
			}

			return new SongMetadata(properties)
			{
				Id = itemId,
				Timestamp = timestamp,
			};
		}
	}
}
