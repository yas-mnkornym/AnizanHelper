using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web;
using HtmlAgilityPack;

namespace AnizanHelper.Models.DbSearch
{
	public abstract class DbSeacherBase<TResult> : IDbSearcher<TResult>
	{
		string queryUrlBase_ = "http://anison.info/data/n.php";
		string userAgent_ = @"Mozilla/5.0 (Windows NT 6.3; Trident/7.0; rv:11.0) like Gecko";


		public DbSeacherBase()
		{ }

		public DbSeacherBase(string queryUrl)
		{
			if (queryUrl == null) { throw new ArgumentNullException("queryUrl"); }
			queryUrlBase_ = queryUrl;
		}

		abstract public IEnumerable<TResult> Search(string searchWord);

		protected HtmlDocument QueryDocument(string word, string type)
		{
			if (word == null) { throw new ArgumentNullException("word"); }
			if (type == null) { throw new ArgumentNullException("type"); }
			
			var queryUrl = string.Format("{0}?q={1}&m={2}",
				queryUrlBase_,
				string.Join("+", word.Split(new char[]{' ', '\t'}, StringSplitOptions.RemoveEmptyEntries)
					.Select(x => HttpUtility.UrlEncode(x))),
				HttpUtility.UrlEncode(type));
			using (var client = new HttpClient()) {
				if (!string.IsNullOrWhiteSpace(UserAgent)) {
					client.DefaultRequestHeaders.Add("User-Agent", UserAgent);
				}

				using (var stream = client.GetStreamAsync(queryUrl).Result) {
					var doc = new HtmlDocument();
					doc.Load(stream, Encoding.UTF8);
					return doc;
				}
			}
		}

		public string UserAgent
		{
			get
			{
				return userAgent_;
			}
			set
			{
				userAgent_ = value; ;
			}
		}
	}
}
