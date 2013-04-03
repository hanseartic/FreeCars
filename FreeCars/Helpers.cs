using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows;
using OAuth;

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

		public static void Delete(string address, string parameters, Action<Stream> onDeleted) {
			var uri = new Uri(address + "?" + parameters, UriKind.Absolute);

			var webClient = new WebClient();
			webClient.UploadStringCompleted += (client, arguments) => {
				if (null != arguments.Error) {
					onDeleted(null);
					return;
				}
				using (var resultStream = GenerateStreamFromString(arguments.Result)) {
					onDeleted(resultStream);
				}
			};
			webClient.UploadStringAsync(uri, "DELETE", "");//parameters);
		}

		public static void Post(string address, string parameters, Action<Stream> onResponseGot) {
			var uri = new Uri(address, UriKind.Absolute);

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
							} catch (WebException) {
								onResponseGot(null);
							}
						}, null);
				}, null);
		}
	}
}
