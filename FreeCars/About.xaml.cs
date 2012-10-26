using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Tasks;
using FreeCars.Resources;
using System.Xml;

namespace FreeCars {
	public partial class About : PhoneApplicationPage {
		public About() {
			InitializeComponent();
			DataContext = this;
			try {
				Version = GetAppAttribute("Version");
			} catch {
				Version = "Error loading Version";
			}
		}
		public String Version {
			get { return (String)GetValue(VersionProperty); }
			private set { SetValue(VersionProperty, value); }
		}
		public static readonly DependencyProperty VersionProperty = DependencyProperty.Register(
			"Version",
			typeof(String),
			typeof(About),
			new PropertyMetadata(
				"1.0", 
				new PropertyChangedCallback((DependencyObject d, DependencyPropertyChangedEventArgs e) => { }))
		);
		private void OnTwitterTap(object sender, System.Windows.Input.GestureEventArgs e) {
			try {
				var shareTask = new ShareStatusTask {
					Status = Strings.AboutPageContactTwitter,
				};
				shareTask.Show();
			} catch { }
		}

		private void OnMailTap(object sender, System.Windows.Input.GestureEventArgs e) {
			try {
				var mailTask = new EmailComposeTask {
					To = Strings.AboutPageContactAddress,
					Subject =  String.Format(Strings.SupportEmailSubject, Version),
				};
				mailTask.Show();
			} catch { }
		}

		private static string GetAppAttribute(string attributeName) {
			string appManifestName = "WMAppManifest.xml";
			string appNodeName = "App";

			var settings = new XmlReaderSettings();
			settings.XmlResolver = new XmlXapResolver();

			using (XmlReader rdr = XmlReader.Create(appManifestName, settings)) {
				rdr.ReadToDescendant(appNodeName);
				if (!rdr.IsStartElement()) {
					throw new System.FormatException(appManifestName + " is missing " + appNodeName);
				}
				return rdr.GetAttribute(attributeName);
			}
		}

		private void OnRateTap(object sender, System.Windows.Input.GestureEventArgs e) {
			try {
				var marketplaceReviewTask = new MarketplaceReviewTask();
				marketplaceReviewTask.Show();
			} catch { }
		}

		private void OnGotoUservoiceTap(object sender, System.Windows.Input.GestureEventArgs e) {
			try {
				var webBrowserTask = new WebBrowserTask {
					Uri = new Uri("http://hanseartic.uservoice.com/"),
				};
				webBrowserTask.Show();
			} catch { }
		}
	}
}