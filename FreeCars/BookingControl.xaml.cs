using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using FreeCars.Providers;
using FreeCars.Resources;
using FreeCars.Serialization;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using OAuth;
using System.IO;
using System.Runtime.Serialization.Json;

namespace FreeCars {
	public partial class BookingControl {

		private string username;
		private string password;

		public BookingControl() {
			InitializeComponent();
		}
		
		public void Activate(Marker item) {
			username = null;
			password = null;
			okButton.IsEnabled = true;
			cancelButton.IsEnabled = true;
			bookingProgressBar.Visibility = Visibility.Collapsed;
			try {
				VisualStateManager.GoToState(this, "ActiveState", true);
				Item = item;
				IsActive = true;
			} catch (UnauthorizedAccessException) {
				Dispatcher.BeginInvoke(() => Activate(item));
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

		private void OnOKButtonClicked(object sender, RoutedEventArgs e) {
			if (null == Item) return;
			if (typeof(Car2GoMarker) == Item.GetType()) {
				okButton.IsEnabled = false;
				cancelButton.IsEnabled = false;
				bookingProgressBar.Visibility = Visibility.Visible;
				if ((Item as Car2GoMarker).isBooked) {
					CancelCar2GoBooking();
				} else {
					CreateCar2GoBooking(delegate(object client, DownloadStringCompletedEventArgs arguments) {
					});
				}
			} else if (typeof (DriveNowMarker) == Item.GetType()) {
				//CreateDriveNowBooking();
			}
		}

		private void OnLoaded(object sender, System.Windows.RoutedEventArgs e) {
			Deactivate();
			DataContext = this;
		}

		private void OnBookingBrowserNavigated(object sender, NavigationEventArgs e) {
		}
		private void OnBookingBrowserNavigationFailed(object sender, NavigationFailedEventArgs e) {
			/// TODO message-box or toast notifiyng about failed browsing.
		}

		public bool IsActive {
			get;
			private set;
		}

		DependencyProperty ItemProperty = DependencyProperty.Register("Item", typeof(Marker), typeof(BookingControl), new PropertyMetadata(null));
		public Marker Item {
			get { return (Marker)GetValue(ItemProperty); }
			private set {
				SetValue(ItemProperty, value);
				Visibility
					car2GoVisibility = Visibility.Collapsed,
					car2GoBookVisibility = Visibility.Collapsed,
					car2GoCancelVisibility = Visibility.Collapsed,
					driveNowVisbility = Visibility.Collapsed,
					multicityVisibility = Visibility.Collapsed;
				if (null != value) {
					var itemType = value.GetType();
					if (typeof(Car2GoMarker) == itemType) {
						if (((Car2GoMarker)value).isBooked) {
							car2GoCancelVisibility = Visibility.Visible;
						} else {
							car2GoBookVisibility = Visibility.Visible;
						}
						car2GoVisibility = Visibility.Visible;
						Car2GoInteriorImagePath = ("GOOD" == (value as Car2GoMarker).interior)
													  ? "/Resources/ib_condition_good.png"
													  : "/Resources/ib_condition_unacceptable.png";
						Car2GoExteriorImagePath = ("GOOD" == (value as Car2GoMarker).exterior)
													  ? "/Resources/ib_condition_good.png"
													  : "/Resources/ib_condition_unacceptable.png";
						carDescription.Text = (value as Car2GoMarker).address;
					} else if (typeof(DriveNowMarker) == itemType) {
						driveNowVisbility = Visibility.Visible;
						CreateDriveNowBooking();
					} else if (typeof(MulticityMarker) == itemType) {
						multicityVisibility = Visibility.Visible;
					}
				}
				Car2GoVisibility = car2GoVisibility;
				Car2GoBookVisibility = car2GoBookVisibility;
				Car2GoCancelVisibility = car2GoCancelVisibility;
				DriveNowVisibility = driveNowVisbility;
				MulticityVisibility = multicityVisibility;
			}
		}

		private void TriggerEvent(EventHandler eventName) {
			try {
				if (null != eventName) {
					eventName(this, null);
				}
			} catch { }
		}

		public event EventHandler Closed;

		public event EventHandler ActionCompleted;
		protected virtual void InvokeActionCompleted() {
			Deactivate();
			var handler = ActionCompleted;
			if (handler != null) {
				handler(this, EventArgs.Empty);
			}
		}
		# region Car2Go
		private void CreateCar2GoBooking(DownloadStringCompletedEventHandler requestCallback) {
			var item = (Car2GoMarker)Item;
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
			var signatureBase = OAuthTools.ConcatenateRequestElements("POST", car2GoRequestEndpoint, parameters);
			var signature = OAuthTools.GetSignature(
				OAuthSignatureMethod.HmacSha1,
				OAuthSignatureTreatment.Escaped,
				signatureBase,
				FreeCarsCredentials.Car2Go.SharedSecred,
				oauthTokenSecret);

			var requestParameters = OAuthTools.NormalizeRequestParameters(parameters);
			var para = requestParameters + "&oauth_signature=" + signature;

			Helpers.Post(car2GoRequestEndpoint, para, delegate(Stream response) {
				if (null == response) return;
				var serializer = new DataContractJsonSerializer(typeof(Car2GoBookingResult));
				var resultAccounts = (Car2GoBookingResult)serializer.ReadObject(response);
				Dispatcher.BeginInvoke(() => {
					var mbResult = MessageBoxResult.None;
					try {
						mbResult = 0 == resultAccounts.ReturnValue.Code 
							? MessageBox.Show(resultAccounts.Booking[0].Vehicle.Position.Address, resultAccounts.ReturnValue.Description, MessageBoxButton.OK)
							: MessageBox.Show(resultAccounts.ReturnValue.Description);
					} catch (Exception) {
						Deactivate();
					}
					if (mbResult == MessageBoxResult.OK) {
						InvokeActionCompleted();
					}
				});
			});
		}
		private void CancelCar2GoBooking() {
			var item = (Car2GoMarker)Item;
			var car2GoRequestEndpoint = "https://www.car2go.com/api/v2.1/booking/" + item.BookingId;

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
			parameters.Add("account", accountId);
			var signatureBase = OAuthTools.ConcatenateRequestElements("DELETE", car2GoRequestEndpoint, parameters);
			var signature = OAuthTools.GetSignature(
				OAuthSignatureMethod.HmacSha1,
				OAuthSignatureTreatment.Escaped,
				signatureBase,
				FreeCarsCredentials.Car2Go.SharedSecred,
				oauthTokenSecret);

			var requestParameters = OAuthTools.NormalizeRequestParameters(parameters);
			var para = requestParameters + "&oauth_signature=" + signature;

			Helpers.Delete(car2GoRequestEndpoint, para, delegate(Stream args) {
				if (null == args) return;
				try {
					var serializer = new DataContractJsonSerializer(typeof(Car2GoCancelBookingResult));
					var resultAccount = (Car2GoCancelBookingResult)serializer.ReadObject(args);
					Dispatcher.BeginInvoke(() => {
						var mbResult = MessageBoxResult.None;
						try {
							if (0 == resultAccount.ReturnValue.Code) {
								var message = (resultAccount.CancelBooking[0].cancelFeeExists)
												  ? String.Format(
													  Strings.BookingPageC2GCancelationSuccessful,
													  resultAccount.CancelBooking[0].cancelFee,
													  resultAccount.CancelBooking[0].cancelFeeCurrency)
												  : String.Format(
													  Strings.BookingPageC2GCancelationSuccessful,
													  0, "");
								mbResult = MessageBox.Show(
									message,
									resultAccount.ReturnValue.Description, MessageBoxButton.OK);
							} else {
								mbResult = MessageBox.Show(resultAccount.ReturnValue.Description);
							}
						} catch (Exception) {
							Deactivate();
						}
						if (mbResult != MessageBoxResult.OK) {
							return;
						}
						InvokeActionCompleted();
					});
				} catch (SerializationException) {
					InvokeActionCompleted();
				}
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
				default:
					Deactivate();
					break;
			}
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
		public DependencyProperty Car2GoVisibilityProperty = DependencyProperty.Register(
			"Car2GoVisibility", typeof(Visibility), typeof(BookingControl), new PropertyMetadata(Visibility.Visible));
		public DependencyProperty Car2GoBookVisibilityProperty = DependencyProperty.Register(
			"Car2GoBookVisibility", typeof(Visibility), typeof(BookingControl), new PropertyMetadata(Visibility.Visible));
		public DependencyProperty Car2GoCancelVisibilityProperty = DependencyProperty.Register(
			"Car2GoCancelVisibility", typeof(Visibility), typeof(BookingControl), new PropertyMetadata(Visibility.Visible));
		public Visibility Car2GoVisibility {
			get { return (Visibility)GetValue(Car2GoVisibilityProperty); }
			private set { SetValue(Car2GoVisibilityProperty, value); }
		}
		public Visibility Car2GoBookVisibility {
			get { return (Visibility)GetValue(Car2GoBookVisibilityProperty); }
			private set { SetValue(Car2GoBookVisibilityProperty, value); }
		}
		public Visibility Car2GoCancelVisibility {
			get { return (Visibility)GetValue(Car2GoCancelVisibilityProperty); }
			private set { SetValue(Car2GoCancelVisibilityProperty, value); }
		}
		# endregion Car2Go

		# region DriveNow
		private void CreateDriveNowBooking() {
			if (!CheckDriveNowCredentials()) {
				Deactivate();
				return;
			}
			VisualStateManager.GoToState(this, "DNActiveState", false);
			dnBookingBrowser.LoadCompleted -= onDriveNowLoadCompleted;
			dnBookingBrowser.ScriptNotify -= onDriveNowScriptNotify;
			var item = (DriveNowMarker)Item;

			dnBookingBrowser.ScriptNotify += onDriveNowScriptNotify;
			dnBookingBrowser.LoadCompleted += onDriveNowLoadCompleted;
			dnBookingBrowser.Navigate(new Uri("https://m.drive-now.com/", UriKind.Absolute));

			//dnBookingBrowser.Navigate(new Uri("https://m.drive-now.com/php/metropolis/details?vin=" + item.vin, UriKind.Absolute));
		}
		private void gotoDriveNowCredentials() {
			var phoneApplicationFrame = Application.Current.RootVisual as PhoneApplicationFrame;
			if (phoneApplicationFrame != null) {
				phoneApplicationFrame.Navigate(
					new Uri("/SettingsPage.xaml?tab=driveNowTab&action=enterCredentials", UriKind.RelativeOrAbsolute));
			}
		}

		private bool CheckDriveNowCredentials() {
			try {
				username = (string)App.GetAppSetting("driveNow.username");
				password = (string)App.GetAppSetting("driveNow.password");
				if ((null != username) && (null != password)) {
					return true;
				}
				MessageBoxResult result = MessageBox.Show(Strings.DriveNowCredentialsMissingDialogMessage, Strings.DriveNowCredentialsMissingDialogHeader, MessageBoxButton.OKCancel);
				if (MessageBoxResult.OK == result) {
					gotoDriveNowCredentials();
				} else {
					Deactivate();
				}
			} catch {

			}
			return false;
		}
		private void onDriveNowScriptNotify(object sender, NotifyEventArgs args) {
			var item = Item as DriveNowMarker;
			if (null == item) {
				dnBookingBrowser.LoadCompleted -= onDriveNowLoadCompleted;
				return;
			}
			if ("dn-login-incorrect" == args.Value) {
				if (MessageBoxResult.OK == MessageBox.Show(
					Strings.BookingDriveNowUsernamePasswordWrongDialog,
					Strings.UsernameOrPasswordWrong, MessageBoxButton.OKCancel)) {
					gotoDriveNowCredentials();
				}
				Deactivate();
				return;
			}
			if ("dn-booking-successful" == args.Value) {
				MessageBox.Show(Strings.DriveNowBookingSucessfull);
				Deactivate();
				return;
			}
			if ("dn-loggedin" != args.Value) {
				return;
			}
			dnBookingBrowser.LoadCompleted -= onDriveNowLoadCompleted;
			VisualStateManager.GoToState(this, "DnBookingBrowserOpenState", true);
			dnBookingBrowser.Navigate(new Uri("https://m.drive-now.com/php/metropolis/details?vin=" + item.ID + "#bookingdetails-form", UriKind.Absolute));
		}
		private void onDriveNowLoadCompleted(object sender, NavigationEventArgs e) {
			try {
				dnBookingBrowser.InvokeScript("eval", "window.checkLoggedIn = function() {" +
					"window.external.notify('enter');" +
					//"dn_login.throw_login_error = function(e, d) { window.external.notify('dn-login-incorrect'); };" +
					"if ($('.dn-welcome').length <= 0) {" +
						"$('#login-field').val('" + username + "');$('#password-field').val('" + password + "');" +
						"window.external.notify('dn-not-loggedin');" +
						"dn_login.init('login-field', 'password-field', '.login_error');" +
					"} else {" +
						"window.external.notify('dn-loggedin');" +
					"}" +
					//"window.original_dn_booking_confirmation = dn_booking_confirmation;" +
					//"window.original_error_func = dn_login.throw_login_error;" +
					"window.dn_booking_confirmation = function() { window.external.notify('dn-booking-successful'); };" +
					"dn_login.throw_login_error = function(e, d) { window.external.notify('dn-login-incorrect'); };" +
				"}; " +
				"window.readyStateCheckInterval = setInterval(function() {window.external.notify('timer');if (document.readyState === 'complete') {" +
				"if (typeof dn_login === 'undefined') return; clearInterval(window.readyStateCheckInterval);window.checkLoggedIn();}}, 100);" +
					//"window.checkLoggedIn();" +
				"");
			} catch (Exception exception) {
				if (exception.Message.Contains("Error: 80020101.")) {
					Deactivate();
				}
			}
		}
		public DependencyProperty DriveNowVisibilityProperty = DependencyProperty.Register(
			"DriveNowVisibility", typeof(Visibility), typeof(BookingControl), new PropertyMetadata(Visibility.Visible));
		public Visibility DriveNowVisibility {
			get { return (Visibility)GetValue(DriveNowVisibilityProperty); }
			private set { SetValue(DriveNowVisibilityProperty, value); }
		}
		# endregion DriveNow

		# region multicity
		private bool CheckMulticityCredentials() {
			try {
				username = "";
			} catch {}
			MessageBoxResult result;
			return false;
		}
		public DependencyProperty MulticityVisibilityProperty = DependencyProperty.Register(
			"MulticityVisibility", typeof(Visibility), typeof(BookingControl), new PropertyMetadata(Visibility.Visible));
		public Visibility MulticityVisibility {
			get { return (Visibility)GetValue(MulticityVisibilityProperty); }
			private set { SetValue(MulticityVisibilityProperty, value); }
		}
		#endregion multicity

	}
}
