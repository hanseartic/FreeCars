using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using System.IO.IsolatedStorage;
using FreeCars.Resources;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;
using System.Globalization;
using System.Net;
using OAuth;
using System.Runtime.Serialization.Json;
using System.IO;

namespace FreeCars {
	public partial class SettingsPage : PhoneApplicationPage {

		public SettingsPage() {
			InitializeComponent();
			LoadAppBar();
			LoadCar2GoCities();
			connectToCar2GoButton.DataContext = this;
		}

		private void LoadCar2GoCities() {
			car2goCitiesList.DataContext = App.Current.Resources["car2go"] as Car2Go;
			var binding = new Binding("Cities") { Source = App.Current.Resources["car2go"] as Car2Go };
			BindingOperations.SetBinding(car2goCitiesList, ListPicker.ItemsSourceProperty, binding);
		}
		private void AppOnTrialModeChanged(object sender, EventArgs e) {
			if (App.IsInTrialMode) {
				ShowAdsToggleSwitch.Visibility = Visibility.Visible;
			}
		}
		private void OnSettingsPageLoaded(object sender, RoutedEventArgs e) {
			((App)App.Current).TrialModeChanged += AppOnTrialModeChanged;
			AppOnTrialModeChanged(null, null);
			CheckCar2GoApiAccess();
		}

		private void OnToggleSwitchChanged(ToggleSwitch sender) {
			sender.Content = true == sender.IsChecked
				? Strings.ToggleSwitchOn
				: Strings.ToggleSwitchOff;
		}
		private void SaveToggleSwitch(string setting, bool? isChecked) {
			App.SetAppSetting(setting, isChecked);
		}
		private void OnGPSToggleSwitchChanged(object sender, RoutedEventArgs e) {
			SaveToggleSwitch("settings_use_GPS", ((ToggleSwitch)sender).IsChecked);
			OnToggleSwitchChanged((ToggleSwitch)sender);
		}
		private void OnMulticityCarsToggleSwitchChanged(object sender, RoutedEventArgs e) {
			SaveToggleSwitch("settings_show_multicity_cars", ((ToggleSwitch)sender).IsChecked);
			OnToggleSwitchChanged((ToggleSwitch)sender);
		}

		private void OnMulticityChargersToggleSwitchChanged(object sender, RoutedEventArgs e) {
			SaveToggleSwitch("settings_show_multicity_chargers", ((ToggleSwitch)sender).IsChecked);
			OnToggleSwitchChanged((ToggleSwitch)sender);
		}
		private void OnDriveNowCarsToggleSwitchChanged(object sender, RoutedEventArgs e) {
			SaveToggleSwitch("settings_show_drivenow_cars", ((ToggleSwitch)sender).IsChecked);
			OnToggleSwitchChanged((ToggleSwitch)sender);
		}
		private void OnCar2GoCarsToggleSwitchChanged(object sender, RoutedEventArgs e) {
			SaveToggleSwitch("settings_show_car2go_cars", ((ToggleSwitch)sender).IsChecked);
			OnToggleSwitchChanged((ToggleSwitch)sender);
		}
		private void OnAllowAnalyticsToggleSwitchChanged(object sender, RoutedEventArgs e) {
			SaveToggleSwitch("settings_allow_analytics", ((ToggleSwitch)sender).IsChecked);
			OnToggleSwitchChanged((ToggleSwitch)sender);
		}
		private void OnCar2goCitySelected(object sender, SelectionChangedEventArgs e) {
			try {
				var selectedItem = (sender as ListPicker).SelectedItem as Car2GoLocation;
				if (0 == selectedItem.locationId) {
					try {
						IsolatedStorageSettings.ApplicationSettings.Remove("car2goSelectedCity");
						IsolatedStorageSettings.ApplicationSettings.Save();
					} catch (Exception) { }
				} else {
					App.SetAppSetting("car2goSelectedCity", selectedItem.locationName);
				}
			} catch (NullReferenceException) { }
		}
		private void OnShowAdsToggleSwitchChanged(object sender, RoutedEventArgs e) {
			if (false == ((ToggleSwitch)sender).IsChecked) {
				var buyAppNow = MessageBox.Show(Strings.SettingsPageBuyText, Strings.SettingsPageBuyCaption, MessageBoxButton.OKCancel);
				if (MessageBoxResult.OK == buyAppNow) {
					var mt = new MarketplaceDetailTask {
						ContentType = MarketplaceContentType.Applications,
					};
					mt.Show();
				}
			}
			OnToggleSwitchChanged((ToggleSwitch)sender);
		}
		private void OnGPSToggleSwitchLoaded(object sender, RoutedEventArgs e) {
			try {
				((ToggleSwitch)sender).IsChecked = (true == (bool)IsolatedStorageSettings.ApplicationSettings["settings_use_GPS"]);
			} catch (KeyNotFoundException) { }
			OnToggleSwitchChanged((ToggleSwitch)sender);
		}
		private void OnCar2GoCarsToggleSwitchLoaded(object sender, RoutedEventArgs e) {
			try {
				((ToggleSwitch)sender).IsChecked = (true == (bool)IsolatedStorageSettings.ApplicationSettings["settings_show_car2go_cars"]);
			} catch (KeyNotFoundException) { ((ToggleSwitch)sender).IsChecked = true; }
			OnToggleSwitchChanged((ToggleSwitch)sender);
		}
		private void OnMulticityCarsToggleSwitchLoaded(object sender, RoutedEventArgs e) {
			try {
				((ToggleSwitch)sender).IsChecked = (true == (bool)IsolatedStorageSettings.ApplicationSettings["settings_show_multicity_cars"]);
			} catch (KeyNotFoundException) { ((ToggleSwitch)sender).IsChecked = true; }
			OnToggleSwitchChanged((ToggleSwitch)sender);
		}
		private void OnMulticityChargersToggleSwitchLoaded(object sender, RoutedEventArgs e) {
			try {
				((ToggleSwitch)sender).IsChecked = (true == (bool)IsolatedStorageSettings.ApplicationSettings["settings_show_multicity_chargers"]);
			} catch (KeyNotFoundException) { ((ToggleSwitch)sender).IsChecked = true; }
			OnToggleSwitchChanged((ToggleSwitch)sender);
		}
		private void OnDriveNowCarsToggleSwitchLoaded(object sender, RoutedEventArgs e) {
			try {
				((ToggleSwitch)sender).IsChecked = (true == (bool)IsolatedStorageSettings.ApplicationSettings["settings_show_drivenow_cars"]);
			} catch (KeyNotFoundException) { ((ToggleSwitch)sender).IsChecked = true; }
			OnToggleSwitchChanged((ToggleSwitch)sender);
		}
		private void OnAllowAnalyticsToggleSwitchLoaded(object sender, RoutedEventArgs e) {
			try {
				((ToggleSwitch)sender).IsChecked = (true == (bool)IsolatedStorageSettings.ApplicationSettings["settings_allow_analytics"]);
			} catch (KeyNotFoundException) { ((ToggleSwitch)sender).IsChecked = true; }
			OnToggleSwitchChanged((ToggleSwitch)sender);
		}
		private void OnShowAdsToggleSwitchLoaded(object sender, RoutedEventArgs e) {
			((ToggleSwitch)sender).IsChecked = App.IsInTrialMode;
			OnToggleSwitchChanged((ToggleSwitch)sender);
		}
		private void OnCar2goCitySelectLoaded(object sender, RoutedEventArgs e) {
			try {
				var selectedCityName = (string)IsolatedStorageSettings.ApplicationSettings["car2goSelectedCity"];
				foreach (Car2GoLocation cityLocation in from Car2GoLocation cityLocation in (sender as ListPicker).Items where null != cityLocation where cityLocation.locationName == selectedCityName select cityLocation) {
					(sender as ListPicker).SelectedItem = cityLocation;
					return;
				}
			} catch (KeyNotFoundException) { }
		}
		private void LoadAppBar() {
			ApplicationBar = new ApplicationBar {
				Mode = ApplicationBarMode.Minimized,
				Opacity = 0.8,
				IsVisible = true,
				IsMenuEnabled = false,
			};

			var settingsPageApplicationBarInfoButton = new ApplicationBarIconButton {
				IconUri = new Uri("/Resources/about48x48.png", UriKind.Relative),
				Text = FreeCars.Resources.Strings.SettingsAppbarAbout,
			};
			settingsPageApplicationBarInfoButton.Click += OnSettingsPageApplicationBarInfoButtonClick;
			ApplicationBar.Buttons.Add(settingsPageApplicationBarInfoButton);
		}
		private void OnSettingsPageApplicationBarInfoButtonClick(object sender, EventArgs e) {
			NavigationService.Navigate(new Uri("/About.xaml", UriKind.RelativeOrAbsolute));
		}

		private void OnCallMulticityTap(object sender, System.Windows.Input.GestureEventArgs e) {
			try {
				var callTask = new PhoneCallTask {
					DisplayName = Strings.SettingsPageCallMulticityPhoneName,
					PhoneNumber = Strings.SettingsPageCallMulticityPhoneNumber,
				};
				callTask.Show();
			} catch { }
		}

		private void OnCallDriveNowTap(object sender, System.Windows.Input.GestureEventArgs e) {
			try {
				var callTask = new PhoneCallTask {
					DisplayName = Strings.SettingsPageCallDrivenowPhoneName,
					PhoneNumber = Strings.SettingsPageCallDrivenowPhoneNumberToDial,
				};
				callTask.Show();
			} catch { }
		}

		private void OnRedeemDrivenowPromoViaMailButtonTap(object sender, System.Windows.Input.GestureEventArgs e) {
			try {
				var lang = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName.ToUpper() == "DE"
					? "de_DE"
					: "en_US";
				var promoCodeMailTask = new EmailComposeTask {
					Subject = "DriveNow PromoCode",
					Body = "PromoCode: ZNEUATHQJA\n\nhttps://de.drive-now.com/php/metropolis/registration?language=" + lang + "&L=2&prc=ZNEUATHQJA",
				};
				promoCodeMailTask.Show();
			} catch { }
		}

		private void OnRedeemDrivenowPromoViaWebButtonTap(object sender, System.Windows.Input.GestureEventArgs e) {
			try {
				var lang = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName.ToUpper() == "DE"
					? "de_DE"
					: "en_US";
				var promoCodeBrowserTask = new WebBrowserTask {
					Uri = new Uri("https://de.drive-now.com/php/metropolis/registration?language=" + lang + "&L=2&prc=ZNEUATHQJA"),
				};
				promoCodeBrowserTask.Show();
			} catch { }
		}

		private void OnConnectToCar2GoClicked(object sender, RoutedEventArgs e) {
			connectCar2Go();
		}

		private void connectCar2Go() {
			var car2GoGetTokenEndpoint = "https://www.car2go.com/api/reqtoken";

			var oauthRequest = new OAuthRequest() {
				CallbackUrl = "oob",
				ConsumerKey = FreeCarsCredentials.Car2Go.ConsumerKey,
				ConsumerSecret = FreeCarsCredentials.Car2Go.SharedSecred,
				Method = "GET",
				RequestUrl = car2GoGetTokenEndpoint,
				SignatureMethod = OAuthSignatureMethod.HmacSha1,
				Type = OAuthRequestType.RequestToken,
				Version = "1.0",
			};
			var requestParameters = oauthRequest.GetAuthorizationQuery();
			var requestUrl = new Uri(car2GoGetTokenEndpoint + "?" + requestParameters, UriKind.Absolute);

			var webClient = new WebClient();
			webClient.DownloadStringCompleted += delegate(object client, DownloadStringCompletedEventArgs arguments) {
				string[] results = { };
				if (null == arguments.Error) {
					results = arguments.Result.Split(new string[] { "&" }, StringSplitOptions.None);
					App.SetAppSetting("car2go.oauth_token_secret", results[1].Split(new char[] { '=' })[1]);
					//(string)IsolatedStorageSettings.ApplicationSettings["current_map_city"]
					c2gAuthBrowser.Dispatcher.BeginInvoke(() => {
						c2gAuthBrowser.Visibility = System.Windows.Visibility.Visible;
						c2gAuthBrowser.Navigate(new Uri("https://www.car2go.com/api/authorize?" + arguments.Result, UriKind.Absolute));
					});
				}
				client = null;
			};
			webClient.DownloadStringAsync(requestUrl);
			return;
			//c2gAuthBrowser.Navigate();
		}

		public DependencyProperty ShowConnectCar2GoApiButtonProperty = DependencyProperty.Register(
			"ShowConnectCar2GoApiButton",
			typeof(Visibility),
			typeof(SettingsPage),
			new PropertyMetadata(Visibility.Collapsed));

		public Visibility ShowConnectCar2GoApiButton {
			get { return (Visibility)GetValue(ShowConnectCar2GoApiButtonProperty); }
			set { SetValue(ShowConnectCar2GoApiButtonProperty, value); }
		}

		private void CheckCar2GoApiAccess() {
			var hasApiAccess = false;

			var oauth_token = (string)App.GetAppSetting("car2go.oauth_token");
			var oauth_token_secret = (string)App.GetAppSetting("car2go.oauth_token_secret");

			if (null != oauth_token && null != oauth_token_secret) {
				getCar2GoAccounts(
					oauth_token,
					oauth_token_secret,
					delegate(object client, DownloadStringCompletedEventArgs arguments) {
						string[] results = { };
						if (null == arguments.Error) {
							//results = arguments.Result.Split(new char[] { '&' }, StringSplitOptions.None);
							using (Stream resultStream = Helpers.GenerateStreamFromString(arguments.Result)) {
								var serializer = new DataContractJsonSerializer(typeof(ResultAccounts));
								var resultAccounts = (ResultAccounts)serializer.ReadObject(resultStream);
								switch (resultAccounts.ReturnValue.Code) {
									case 0:
										c2gAuthText.Text = String.Format(
											Strings.SettingsPageCar2GoAuthStatusOK,
											new string[] { resultAccounts.Account[0].Description, Car2Go.City });
										App.SetAppSetting("car2go.oauth_account_id", resultAccounts.Account[0].AccountId);
										break;
									case 1:
										goto default;
									case 2:
										goto default;
									case 3:
										goto default;
									default:
										c2gAuthText.Text = String.Format(
											Strings.SettingsPageCar2GoAuthStatusError,
											new string[] { Car2Go.City });
										break;
								}
								ShowConnectCar2GoApiButton = Visibility.Collapsed;
							}
						}
						client = null;
					}
				);
			}

			if (false == hasApiAccess) {
				ShowConnectCar2GoApiButton = Visibility.Visible;
			}
		}
		private void getCar2GoAccounts(string token, string token_secret, DownloadStringCompletedEventHandler requestCallback) {

			var car2GoRequestEndpoint = "https://www.car2go.com/api/v2.1/accounts";

			var parameters = new WebParameterCollection();
			parameters.Add("oauth_callback", "oob");
			parameters.Add("oauth_signature_method", "HMAC-SHA1");
			parameters.Add("oauth_token", token);
			parameters.Add("oauth_version", "1.0");
			parameters.Add("oauth_consumer_key", FreeCarsCredentials.Car2Go.ConsumerKey);
			parameters.Add("oauth_timestamp", OAuthTools.GetTimestamp());
			parameters.Add("oauth_nonce", OAuthTools.GetNonce());
			parameters.Add("format", "json");
			parameters.Add("loc", Car2Go.City);
			//parameters.Add("test", "1");
			var signatureBase = OAuthTools.ConcatenateRequestElements("GET", car2GoRequestEndpoint, parameters);
			var signature = OAuthTools.GetSignature(OAuthSignatureMethod.HmacSha1, OAuthSignatureTreatment.Escaped, signatureBase, FreeCarsCredentials.Car2Go.SharedSecred, token_secret);

			var requestParameters = OAuthTools.NormalizeRequestParameters(parameters);
			var requestUrl = new Uri(car2GoRequestEndpoint + "?" + requestParameters + "&oauth_signature=" + signature, UriKind.Absolute);

			var webClient = new WebClient();
			webClient.DownloadStringCompleted += requestCallback;

			webClient.DownloadStringAsync(requestUrl);
		}
		private void getC2GAccessToken(string[] tokenData) {
			var car2GoGetTokenEndpoint = "https://www.car2go.com/api/accesstoken";

			var oauthRequest = new OAuthRequest() {
				CallbackUrl = "oob",
				ConsumerKey = FreeCarsCredentials.Car2Go.ConsumerKey,
				ConsumerSecret = FreeCarsCredentials.Car2Go.SharedSecred,
				Method = "GET",
				RequestUrl = car2GoGetTokenEndpoint,
				SignatureMethod = OAuthSignatureMethod.HmacSha1,
				Token = tokenData[0],
				TokenSecret = (string)App.GetAppSetting("car2go.oauth_token_secret"),
				//TokenSecret = tokenData[1],
				Type = OAuthRequestType.AccessToken,
				Verifier = tokenData[1],
				Version = "1.0",
			};
			var requestParameters = oauthRequest.GetAuthorizationQuery();
			var requestUrl = new Uri(car2GoGetTokenEndpoint + "?" + requestParameters, UriKind.Absolute);

			var webClient = new WebClient();
			webClient.DownloadStringCompleted += (client, arguments) => {
				string[] results = { };
				if (null == arguments.Error) {
					results = arguments.Result.Split(new char[] { '&' }, StringSplitOptions.None);
					App.SetAppSetting("car2go.oauth_token", results[0].Split(new char[] { '=' })[1]);
					App.SetAppSetting("car2go.oauth_token_secret", results[1].Split(new char[] { '=' })[1]);
				}
			};
			webClient.DownloadStringAsync(requestUrl);
		}

		protected override void OnNavigatedTo(NavigationEventArgs e) {
			string tab = string.Empty;
			string action = string.Empty;
			if (NavigationContext.QueryString.TryGetValue("tab", out tab) && NavigationContext.QueryString.TryGetValue("action", out action)) {
				var pivotItemToShow = settingsTabs.Items.Cast<PivotItem>().Single(i => i.Name == tab);
				settingsTabs.SelectedItem = pivotItemToShow;
				if ("connectCar2Go" == action) {
					connectCar2Go();
				}
			}
			base.OnNavigatedTo(e);
		}
		protected override void OnBackKeyPress(CancelEventArgs e) {
			if (Visibility.Visible == c2gAuthBrowser.Visibility) {
				c2gAuthBrowser.Visibility = Visibility.Collapsed;
				e.Cancel = true;
			}
		}
		private void Onc2gAuthBrowserNavigated(object sender, System.Windows.Navigation.NavigationEventArgs e) {
			var browser = (sender as WebBrowser);
			try {
				browser.InvokeScript("eval",
					"var script = document.createElement('script');" +
					"script.src = \"https://ajax.aspnetcdn.com/ajax/jQuery/jquery-1.9.0.min.js\";" +
					"document.body.appendChild(script);" +
					"$('body').css('transform-origin', '0 0');" +
					"$('body').css('transform', 'scale(2.5)');"
				);
			} catch { }
			switch (e.Uri.AbsolutePath) {
				case "/api/authorize/granted.jsp":
					var tokenData = new string[2];
					var tokenIndex = 0;
					foreach (var data in e.Uri.Query.Substring(1).Split(new char[] { '&' })) {
						tokenData[tokenIndex++] = data.Split(new char[] { '=' })[1];
					}
					getC2GAccessToken(tokenData);
					browser.Navigate(new Uri("https://" + e.Uri.Host + "/api/logout.jsp"));
					break;
				case "/api/logout.jsp":
					browser.Visibility = System.Windows.Visibility.Collapsed;
					CheckCar2GoApiAccess();
					break;
				default:

					break;
			}
		}
	}
}
