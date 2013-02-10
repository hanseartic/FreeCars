using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using FreeCars.Resources;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using OAuth;
using System.IO;
using System.Runtime.Serialization.Json;

namespace FreeCars {
	public partial class BookingControl : UserControl {
		public BookingControl() {
			InitializeComponent();
		}
		
		public void Activate(Marker item) {
			Item = item;
			try {
				VisualStateManager.GoToState(this, "ActiveState", true);
				IsActive = true;
			} catch (UnauthorizedAccessException) {
				Dispatcher.BeginInvoke(() => { Activate(item); });
			}
		}
		internal void Deactivate() {
			try {
				VisualStateManager.GoToState(this, "InactiveState", true);
				IsActive = false;
				TriggerEvent(Closed);
			} catch (UnauthorizedAccessException) {
				Dispatcher.BeginInvoke(Deactivate);
			}
		}

		private void OnCancelButtonClicked(object sender, System.Windows.RoutedEventArgs e) {
			Deactivate();
		}

		private void OnOKButtonClicked(object sender, System.Windows.RoutedEventArgs e) {
			if (null == Item) return;
			if (typeof(Car2GoInformation) == Item.GetType()) {
				CreateCar2GoBooking(delegate(object client, DownloadStringCompletedEventArgs arguments) {
					
				});
			}
		}

		private void CreateCar2GoBooking(DownloadStringCompletedEventHandler requestCallback) {
			var item = (Car2GoInformation)Item;
			var car2GoRequestEndpoint = "https://www.car2go.com/api/v2.1/bookings";
			
			var oauthToken = (string)App.GetAppSetting("car2go.oauth_token");
			var oauthTokenSecret = (string)App.GetAppSetting("car2go.oauth_token_secret");
			if (null == oauthToken || null == oauthTokenSecret) {
				HandleNotConnectedToCar2Go();
			}
			var accountId = "";
			try {
				accountId = ((int)App.GetAppSetting("car2go.oauth_account_id")).ToString();
			} catch (NullReferenceException) {
				return;
			}

			var parameters = new WebParameterCollection();
			parameters.Add("oauth_callback", "oob");
			parameters.Add("oauth_signature_method", "HMAC-SHA1");
			parameters.Add("oauth_token", oauthToken);
			parameters.Add("oauth_version", "1.0");
			parameters.Add("oauth_consumer_key", FreeCarsCredentials.Car2Go.ConsumerKey);
			parameters.Add("oauth_timestamp", OAuthTools.GetTimestamp());
			parameters.Add("oauth_nonce", OAuthTools.GetNonce());
			parameters.Add("format", "json");
			parameters.Add("loc", Car2Go.City);
			parameters.Add("vin", item.ID);
			parameters.Add("account", accountId);
			//parameters.Add("test", "1");
			var signatureBase = OAuthTools.ConcatenateRequestElements("POST", car2GoRequestEndpoint, parameters);
			var signature = OAuthTools.GetSignature(
				OAuthSignatureMethod.HmacSha1,
				OAuthSignatureTreatment.Escaped,
				signatureBase,
				FreeCarsCredentials.Car2Go.SharedSecred,
				oauthTokenSecret);

			var requestParameters = OAuthTools.NormalizeRequestParameters(parameters);
			var requestUrl = new Uri(car2GoRequestEndpoint + "?" + requestParameters + "&oauth_signature=" + signature, UriKind.Absolute);

			var para = requestParameters + "&oauth_signature=" + signature;

			Helpers.Post(car2GoRequestEndpoint, para, delegate(Stream response) {
				if (null == response) return;
				var serializer = new DataContractJsonSerializer(typeof(Car2GoBookingResult));
				var resultAccounts = (Car2GoBookingResult)serializer.ReadObject(response);
				Dispatcher.BeginInvoke(() => {
					var mbResult = MessageBoxResult.None;
					try {
						if (0 == resultAccounts.ReturnValue.Code) {
							mbResult = MessageBox.Show(resultAccounts.Booking[0].Vehicle.Position.Address, resultAccounts.ReturnValue.Description, MessageBoxButton.OK);
						} else {
							mbResult = MessageBox.Show("", resultAccounts.ReturnValue.Description, MessageBoxButton.OK);
						}
					} catch (Exception) {
						Deactivate();
					}
					if (mbResult == MessageBoxResult.OK) { Deactivate(); }
				});
			});
		}
		 
		private void HandleNotConnectedToCar2Go() {
			var mbResult = MessageBox.Show(Strings.BookingPageCar2GoNotConnected, Strings.NotConnected, MessageBoxButton.OKCancel);
			switch (mbResult) {
				case MessageBoxResult.OK:
					try {
						(Application.Current.RootVisual as PhoneApplicationFrame).Navigate(
							new Uri("/SettingsPage.xaml?tab=car2goTab&action=connectCar2Go", UriKind.RelativeOrAbsolute));
					} catch (NullReferenceException) {}
					
					break;
				case MessageBoxResult.Cancel:
				default:
					Deactivate();
					break;
			}
		}

		private void OnLoaded(object sender, System.Windows.RoutedEventArgs e) {
			Deactivate();
			DataContext = this;
		}

		public bool IsActive {
			get;
			private set;
		}

		public DependencyProperty Car2GoInteriorImagePathProperty = DependencyProperty.Register("Car2GoInteriorImagePath", typeof(string), typeof(BookingControl), new PropertyMetadata(null));
		public string Car2GoInteriorImagePath {
			get { return (string)GetValue(Car2GoInteriorImagePathProperty); }
			set { SetValue(Car2GoInteriorImagePathProperty, value); }
		}
		public DependencyProperty Car2GoExteriorImagePathProperty = DependencyProperty.Register("Car2GoExteriorImagePath", typeof(string), typeof(BookingControl), new PropertyMetadata(null));
		public string Car2GoExteriorImagePath {
			get { return (string)GetValue(Car2GoExteriorImagePathProperty); }
			set { SetValue(Car2GoExteriorImagePathProperty, value); }
		}

		DependencyProperty ItemProperty = DependencyProperty.Register("Item", typeof(Marker), typeof(BookingControl), new PropertyMetadata(null));
		public Marker Item {
			get { return (Marker)GetValue(ItemProperty); }
			private set {
				SetValue(ItemProperty, value);
				Visibility car2GoVisibility = Visibility.Collapsed, driveNowVisbility = Visibility.Collapsed, multicityVisibility = Visibility.Collapsed;
				if (null != value) {
					var itemType = value.GetType();
					if (typeof(Car2GoInformation) == itemType) {
						car2GoVisibility = Visibility.Visible;
						Car2GoInteriorImagePath = ("GOOD" == (value as Car2GoInformation).interior)
							? "/Resources/ib_condition_good.png"
							: "/Resources/ib_condition_unacceptable.png";
						Car2GoExteriorImagePath = ("GOOD" == (value as Car2GoInformation).exterior)
							? "/Resources/ib_condition_good.png"
							: "/Resources/ib_condition_unacceptable.png";
					} else if (typeof(DriveNowCarInformation) == itemType) {
						driveNowVisbility = Visibility.Visible;
					} else if (typeof(MulticityMarker) == itemType) {
						multicityVisibility = Visibility.Visible;
					}
				}
				Car2GoVisibility = car2GoVisibility;
				DriveNowVisibility = driveNowVisbility;
				MulticityVisibility = multicityVisibility;
			}
		}

		public DependencyProperty Car2GoVisibilityProperty = DependencyProperty.Register(
			"Car2GoVisibility", typeof(Visibility), typeof(BookingControl), new PropertyMetadata(Visibility.Visible));
		public DependencyProperty DriveNowVisibilityProperty = DependencyProperty.Register(
			"DriveNowVisibility", typeof(Visibility), typeof(BookingControl), new PropertyMetadata(Visibility.Visible));
		public DependencyProperty MulticityVisibilityProperty = DependencyProperty.Register(
			"MulticityVisibility", typeof(Visibility), typeof(BookingControl), new PropertyMetadata(Visibility.Visible));

		public Visibility Car2GoVisibility {
			get { return (Visibility)GetValue(Car2GoVisibilityProperty); }
			private set { SetValue(Car2GoVisibilityProperty, value); }
		}
		public Visibility DriveNowVisibility {
			get { return (Visibility)GetValue(DriveNowVisibilityProperty); }
			private set { SetValue(DriveNowVisibilityProperty, value); }
		}
		public Visibility MulticityVisibility {
			get { return (Visibility)GetValue(MulticityVisibilityProperty); }
			private set { SetValue(MulticityVisibilityProperty, value); }
		}

		private void TriggerEvent(EventHandler eventName) {
			try {
				if (null != eventName) {
					eventName(this, null);
				}
			} catch { }
		}

		public event EventHandler Closed;
	}
}
