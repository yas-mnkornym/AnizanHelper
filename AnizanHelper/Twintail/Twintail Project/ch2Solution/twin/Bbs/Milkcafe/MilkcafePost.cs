using System;
using System.Collections.Generic;
using System.Text;

namespace Twin.Bbs
{
	public class MilkcafePost : X2chPost
	{
		public MilkcafePost()
			: base()
		{
		}

		protected override void SetHttpWebRequest(System.Net.HttpWebRequest req)
		{
			req.AllowAutoRedirect = false;
		}
	}
}
