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
using System.IO.IsolatedStorage;
using FreeCars.Resources;

namespace FreeCars {
    public partial class SettingsPage : PhoneApplicationPage {
        public SettingsPage() {
            InitializeComponent();

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

        private void OnGPSToggleSwitchLoaded(object sender, RoutedEventArgs e) {
            try {
                ((ToggleSwitch)sender).IsChecked = (true == (bool)IsolatedStorageSettings.ApplicationSettings["settings_use_GPS"]);
            } catch (KeyNotFoundException) { }
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

    }
}
