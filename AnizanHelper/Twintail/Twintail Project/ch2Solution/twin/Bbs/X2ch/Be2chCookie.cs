// Be2chCookie.cs

using System;
using System.Runtime;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using CSharpSamples;

namespace Twin.Bbs
{
	/// <summary>
	/// Be2chCookie ÇÃäTóvÇÃê‡ñæÇ≈Ç∑ÅB
	/// </summary>
	[Serializable]
	[System.ComponentModel.TypeConverter(typeof(TwinExpandableConverter))]
	public class Be2chCookie : SerializableSettings
	{
		public string Dmdm;
		public string Mdmd;

		public bool IsEmpty {
			get { return (Dmdm == null || Mdmd == null ||
					  Dmdm == "" || Mdmd == ""); }
		}

		public Be2chCookie()
		{
			Mdmd = Dmdm = null;
		}

		public Be2chCookie(SerializationInfo info, StreamingContext context)
			: base(info, context) {}

		public void SetEmpty()
		{
			Dmdm = Mdmd = String.Empty;
		}

	}
}
