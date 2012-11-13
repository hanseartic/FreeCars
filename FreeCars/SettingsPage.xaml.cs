using System;
using System.Collections.Generic;
using System.Windows;
using Microsoft.Phone.Controls;
using System.IO.IsolatedStorage;
using FreeCars.Resources;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;
using System.Threading;
using System.Globalization;

namespace FreeCars {
    public partial class SettingsPage : PhoneApplicationPage {

		public SettingsPage() {
            InitializeComponent();
			LoadAppBar();
        }
		private void AppOnTrialModeChanged(object sender, EventArgs e) {
			if (App.IsInTrialMode) {
				ShowAdsToggleSwitch.Visibility = Visibility.Visible;
			}
		}
		private void OnSettingsPageLoaded(object sender, RoutedEventArgs e) {
			((App)App.Current).TrialModeChanged += AppOnTrialModeChanged;
			AppOnTrialModeChanged(null, null);
		}
        private void OnToggleSwitchChanged(ToggleSwitch sender) {
            sender.Content = true == sender.IsChecked
                ? Strings.ToggleSwitchOn
                : Strings.ToggleSwitchOff;
        }
        private void SaveToggleSwitch(string setting, bool? isChecked) {
            try {
                IsolatedStorageSettings.ApplicationSettings.Add(setting, isChecked);
            } catch (ArgumentException) {
                IsolatedStorageSettings.ApplicationSettings[setting] = isChecked;
            }
            IsolatedStorageSettings.ApplicationSettings.Save();
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
            } catch (KeyNotFoundException) { ((ToggleSwitch)sender).IsChecked = true;  }
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
			} catch (KeyNotFoundException) { ((ToggleSwitch)sender).IsChecked = false; }
			OnToggleSwitchChanged((ToggleSwitch)sender);
		}
		private void OnShowAdsToggleSwitchLoaded(object sender, RoutedEventArgs e) {
			((ToggleSwitch)sender).IsChecked = App.IsInTrialMode;
			OnToggleSwitchChanged((ToggleSwitch)sender);
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
			} catch {}
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
				var promoCodeMailTask = new EmailComposeTask { 
					Subject = "",
					Body = "",
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
    }
}
