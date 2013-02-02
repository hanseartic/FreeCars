using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace FreeCars {
	class Helpers {
		public static Stream GenerateStreamFromString(string s) {
			MemoryStream stream = new MemoryStream();
			StreamWriter writer = new StreamWriter(stream);
			writer.Write(s);
			writer.Flush();
			stream.Position = 0;
			return stream;
		}

		public static void Post(string address, string parameters, Action<Stream> onResponseGot) {
			Uri uri = new Uri(address, UriKind.Absolute);

			HttpWebRequest r = (HttpWebRequest)WebRequest.Create(uri);
			r.Method = "POST";
			r.ContentType = "application/x-www-form-urlencoded";

			r.BeginGetRequestStream(delegate(IAsyncResult req) {
					var outStream = r.EndGetRequestStream(req);
					using (StreamWriter w = new StreamWriter(outStream))
						w.Write(parameters);
					r.BeginGetResponse(delegate(IAsyncResult result) {
							try {
								HttpWebResponse response = (HttpWebResponse)r.EndGetResponse(result);
								using (var stream = response.GetResponseStream()) {
									onResponseGot(stream);
									//using (StreamReader reader = new StreamReader(stream)) {
									//	onResponseGot(reader.ReadToEnd());
									//}
								}
							} catch (WebException ex) {
								onResponseGot(null);
							}
						}, null);
				}, null);
		}
	}
}
