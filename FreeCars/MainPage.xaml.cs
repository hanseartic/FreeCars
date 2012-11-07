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
using AdDuplex;
using Microsoft.Advertising;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Controls.Maps;
using System.Device.Location;
using System.Globalization;
using System.Windows.Data;
using Microsoft.Phone.Shell;
using System.IO.IsolatedStorage;
using FreeCars.Resources;
using System.Windows.Media.Imaging;

namespace FreeCars {
	public partial class MainPage : PhoneApplicationPage {
		private GeoCoordinateWatcher cw;
		private bool gpsAllowed = false;
		public MainPage() {
			InitializeComponent();
			((App)App.Current).CarsUpdated += OnCarsUpdated;
			cw = new GeoCoordinateWatcher(GeoPositionAccuracy.High);
			cw.MovementThreshold = 10;
			cw.PositionChanged += OnMyPositionChanged;
			Loaded += OnMainPageLoaded;
			//map.Children.Add(me);
			SetAppBar();
		}

		private void CheckTrialAndAds() {
			AdsGrid.Visibility = App.IsInTrialMode
				? Visibility.Visible
				: Visibility.Collapsed;
		}
		void OnAppTrialModeChanged(object sender, EventArgs e) {
			CheckTrialAndAds();
		}
		void OnMainPageLoaded(object sender, RoutedEventArgs e) {
			try {
				gpsAllowed = (bool)IsolatedStorageSettings.ApplicationSettings["settings_use_GPS"];
			} catch (KeyNotFoundException) {
				checkForGPSUsage();
			}
			if (gpsAllowed) {
				try {
					var position = (GeoPosition<GeoCoordinate>)IsolatedStorageSettings.ApplicationSettings["my_last_location"];
					ShowMeAtLocation(position.Location);
				} catch (KeyNotFoundException) { }
			} else {
				try {
					IsolatedStorageSettings.ApplicationSettings.Remove("my_last_location");
				} catch { }
			}
			((App)Application.Current).ReloadPOIs();
			CheckTrialAndAds();
			((App)App.Current).TrialModeChanged += OnAppTrialModeChanged;
		}
		bool checkForGPSUsage() {
			var gpsOK = MessageBox.Show(Strings.WelcomeAskGPSBody1 + Environment.NewLine + Strings.WelcomeAskGPSBody2, Strings.WelcomeAskGPSHeader, MessageBoxButton.OKCancel);
			if (MessageBoxResult.OK == gpsOK) {
				gpsAllowed = true;
			} else {
				gpsAllowed = false;
			}
			try {
				IsolatedStorageSettings.ApplicationSettings.Add("settings_use_GPS", gpsAllowed);
			} catch (ArgumentException) {
				IsolatedStorageSettings.ApplicationSettings["settings_use_GPS"] = gpsAllowed;
			}
			IsolatedStorageSettings.ApplicationSettings.Save();
			return gpsAllowed;
		}
		void SetAppBar() {
			ApplicationBar = new ApplicationBar {
				Mode = ApplicationBarMode.Default,
				Opacity = 1.0,
				IsVisible = true,
				IsMenuEnabled = true,
			};

			var mainPageApplicationBarCentermeButton = new ApplicationBarIconButton {
				IconUri = new Uri("/Resources/appbar.map.centerme.rest.png", UriKind.Relative),
				Text = FreeCars.Resources.Strings.MainpageAppbarCenterme,
			};
			mainPageApplicationBarCentermeButton.Click += OnMainPageApplicationBarCentermeButtonClick;

			var mainPageApplicationBarSettingsButton = new ApplicationBarIconButton {
				IconUri = new Uri("/Resources/dark_appbar.feature.settings.rest.png", UriKind.Relative),
				Text = FreeCars.Resources.Strings.MainPageBarSettings,
			};
			mainPageApplicationBarSettingsButton.Click += OnMainPageApplicationBarSettingsButtonClick;

			var mainPageApplicationBarReloadButton = new ApplicationBarIconButton {
				IconUri = new Uri("/Resources/appbar.transport.repeat.rest.png", UriKind.Relative),
				Text = FreeCars.Resources.Strings.MainPageBarReload,
			};
			mainPageApplicationBarReloadButton.Click += OnMainPageApplicationBarReloadButtonClick;
			var mainPageApplicationBarAboutMenuItem = new ApplicationBarMenuItem {
				Text = FreeCars.Resources.Strings.MainMenuAboutAppItemText,
			};
			mainPageApplicationBarAboutMenuItem.Click += OnMainPageApplicationBarAboutMenuItemClick;


			ApplicationBar.Buttons.Add(mainPageApplicationBarCentermeButton);
			ApplicationBar.Buttons.Add(mainPageApplicationBarReloadButton);
			ApplicationBar.Buttons.Add(mainPageApplicationBarSettingsButton);
			ApplicationBar.MenuItems.Add(mainPageApplicationBarAboutMenuItem);

		}

		void OnMainPageApplicationBarCentermeButtonClick(object sender, EventArgs e) {
			myLocationPushpin.Visibility = Visibility.Collapsed;
			if (!gpsAllowed && !checkForGPSUsage()) { return; }
			cw.Start();
		}
		void OnMainPageApplicationBarSettingsButtonClick(object sender, EventArgs e) {
			NavigationService.Navigate(new Uri("/SettingsPage.xaml", UriKind.RelativeOrAbsolute));
		}
		void OnMainPageApplicationBarReloadButtonClick(object sender, EventArgs e) {
			((App)Application.Current).ReloadPOIs();
		}
		void OnMainPageApplicationBarAboutMenuItemClick(object sender, EventArgs e) {
			NavigationService.Navigate(new Uri("/About.xaml", UriKind.RelativeOrAbsolute));
		}
		void OnMyPositionChanged(object sender, GeoPositionChangedEventArgs<GeoCoordinate> e) {
			if (GeoPositionStatus.Ready != ((GeoCoordinateWatcher)sender).Status) return;
			cw.Stop();
			try {
				IsolatedStorageSettings.ApplicationSettings.Add("my_last_location", e.Position);
			} catch (ArgumentException) {
				IsolatedStorageSettings.ApplicationSettings["my_last_location"] = e.Position;
			}
			IsolatedStorageSettings.ApplicationSettings.Save();
			ShowMeAtLocation(e.Position.Location);
			SDKAdControl.Latitude = e.Position.Location.Latitude;
			SDKAdControl.Longitude = e.Position.Location.Longitude;
		}
		void ShowMeAtLocation(GeoCoordinate location) {
			myLocationPushpin.Location = location;
			map.Center = location;
			map.ZoomLevel = 14.0;
			myLocationPushpin.Visibility = Visibility.Visible;
		}

		void OnCarsUpdated(object sender, EventArgs e) {
			if (sender.GetType() == typeof(Multicity)) {
				UpdateMulticityLayers((Multicity)sender);
			}
			if (sender.GetType() == typeof(DriveNow)) {
				UpdateDriveNowLayer((DriveNow)sender);
			}
			if (sender.GetType() == typeof(Car2Go)) {
				UpdateCar2GoLayer((Car2Go)sender);
			}
		}
		void UpdateDriveNowLayer(DriveNow driveNow) {
			var centerLocation = map.Center;
			var cultureInfo = new CultureInfo("en-US");
			var driveNowBrush = new SolidColorBrush(new Color { A = 255, R = 176, G = 105, B = 9 });
			driveNowCarsLayer.Children.Clear();
			foreach (var car in driveNow.DriveNowCars) {
				var distanceToMapCenter = (int)car.position.GetDistanceTo(centerLocation);
				if (1500 < distanceToMapCenter) continue;
				var distance = (int)car.position.GetDistanceTo(myLocationPushpin.Location);
				var pushpinContent = new Border {
					Child = new StackPanel {
						Orientation = System.Windows.Controls.Orientation.Vertical,
						Children = {
							new TextBlock { Text = car.model, },
							new TextBlock { Text = car.licensePlate, },
							new StackPanel {
								Orientation = System.Windows.Controls.Orientation.Horizontal,
								Children = {
									new Image {
										Source = new BitmapImage(new Uri("/Resources/fuel28x28.png", UriKind.Relative)), 
										Margin = new Thickness(0, 0, 12, 0),
									},
									new TextBlock { Text = car.fuelState + "%", },
								},
							},
						},
					},
					Visibility = Visibility.Collapsed,
				};
				var pushpin = new Pushpin {
					Location = car.position,
					Name = car.licensePlate,
					Background = driveNowBrush,
					Opacity = .6,
					Content = pushpinContent,
					Tag = car,
				};
				pushpin.Tap += OnPushpinTap;
				try {
					driveNowCarsLayer.Children.Add(pushpin);
				} catch (ArgumentException) { }
			}
		}
		void UpdateMulticityLayers(Multicity multicity) {
			var myLocation = myLocationPushpin.Location;
			var centerLocation = map.Center;
			var cultureInfo = new CultureInfo("en-US");
			var multcityCarsBrush = new SolidColorBrush(Colors.Red);
			var multcityChargersBrush = new SolidColorBrush(Colors.Green);
			multicityCarsLayer.Children.Clear();
			var tempList = new List<Pushpin>();
			foreach (var car in multicity.MulticityCars) {
				try {
					var distanceToMapCenter = (int)car.position.GetDistanceTo(centerLocation);
					var fuelTextBlock = new TextBlock { Text = !string.IsNullOrEmpty(car.fuelState) ? car.fuelState + "%" : "", };

					if (1500 < distanceToMapCenter) continue;

					if (null == car.fuelState) {
						car.Updated += (updatedCar, eventArgs) => {
						  if (car.fuelState != ((MulticityMarker)updatedCar).fuelState) {
								try {
									car.fuelState = ((MulticityMarker)updatedCar).fuelState;
									fuelTextBlock.Text = car.fuelState + "%";
									} catch { }
							}
							fuelTextBlock.Text = car.fuelState + "%";
						};
					}
					var pushpinContent = new Border {
						Child = new StackPanel {
							Orientation = System.Windows.Controls.Orientation.Vertical,
							VerticalAlignment = System.Windows.VerticalAlignment.Center,
							Children = {
								new TextBlock { Text = car.model, },
								new TextBlock { Text = car.licensePlate, },
								new StackPanel {
									Orientation = System.Windows.Controls.Orientation.Horizontal,
									Children = {
										new Image {
											Source = new BitmapImage(new Uri("/Resources/battery28x28.png", UriKind.Relative)), 
											Margin = new Thickness(0, 0, 12, 0),
										},
										fuelTextBlock,
									},
								},
							},
						},
						Visibility = Visibility.Collapsed,
					};
					var pushpin = new Pushpin() {
						Location = car.position,
						Name = car.hal2option.tooltip,
						Opacity = .6,
						Background = multcityCarsBrush,
						Content = pushpinContent,
						Tag = car,
					};
					pushpin.Tap += OnPushpinTap;
					try {
						multicityCarsLayer.Children.Add(pushpin);
					} catch (ArgumentException) { }
				} catch (ArgumentException) { }
			}
			multicityFlinksterLayer.Children.Clear();
			multicityChargingLayer.Children.Clear();
			foreach (var station in multicity.MulticityChargers) {
				try {
					var coordinate = new GeoCoordinate(
							Double.Parse(station.lat, cultureInfo.NumberFormat),
							Double.Parse(station.lng, cultureInfo.NumberFormat));
					var pushpin = new Pushpin() {
						Location = coordinate,
						Name = station.hal2option.tooltip,
						Opacity = .6,
						Background = multcityChargersBrush,
						Content = new Border {
							Child = new StackPanel {
								Orientation = System.Windows.Controls.Orientation.Horizontal,
								Children = {
									new Image {
										Source = new BitmapImage(new Uri("/Resources/plug28x28.png", UriKind.Relative)),
										Height = 38,
										Width = 38,
									},
									new TextBlock { Text = station.hal2option.markerInfo.free, },
								},
							},
						},
						Tag = station,
					};
					pushpin.Tap += OnPushpinTap;
					multicityCarsLayer.Children.Add(pushpin);
				} catch (ArgumentException) { }
			}

		}
		void UpdateCar2GoLayer(Car2Go car2Go) {
			var usCultureInfo = new CultureInfo("en-US");
			var car2GoCarsBrush = new SolidColorBrush(new Color() {A = 255, R = 0, G = 159, B = 228,});
			var centerLocation = map.Center;
			car2goCarsLayer.Children.Clear();
			foreach (var car in car2Go.Car2GoCars) {
				try {
					var distanceToMapCenter = (int)car.position.GetDistanceTo(centerLocation);

					if (1500 < distanceToMapCenter) continue;

					var pushpin = new Pushpin {
						Location = car.position,
						Background =  car2GoCarsBrush,
					};
					car2goCarsLayer.Children.Add(pushpin);
				} catch { }
			}
		}
		void OnPushpinTap(object sender, System.Windows.Input.GestureEventArgs e) {
			if (null != ((Pushpin)sender).Tag && typeof(MulticityChargerMarker) == ((Pushpin)sender).Tag.GetType()) return;
			e.Handled = true;
			var pushpinContent = ((Pushpin)sender).Content;
			if (typeof(Border) == pushpinContent.GetType()) {
				if (Visibility.Collapsed == ((Border)pushpinContent).Visibility) {
					foreach (var pushpin in activeLayer.Children.ToArray()) {
						if (pushpin.GetType() == typeof(Pushpin)) {
							DeactivatePushpin(pushpin as Pushpin);
						}
					}
					var parentLayer = VisualTreeHelper.GetParent((Pushpin)sender) as MapLayer;
					parentLayer.Children.Remove((Pushpin)sender);
					activeLayer.Children.Add((Pushpin)sender);
					//((Pushpin)sender).Tag = parentLayer;
					((Pushpin)sender).Opacity = 1;
					((Border)pushpinContent).Visibility = Visibility.Visible;
					if (null != ((Pushpin)sender).Tag && typeof(MulticityMarker) == ((Pushpin)sender).Tag.GetType()) {
						Multicity.LoadChargeState(sender as Pushpin);
					}
				} else {
					DeactivatePushpin(sender as Pushpin);
				}
			}

		}
		void loadMulticityChargeState(Pushpin pushpin) {

		}
		void DeactivatePushpin(Pushpin pushpin) {
			if (null != pushpin.Tag) {
				activeLayer.Children.Remove(pushpin);
				if (typeof(MulticityMarker) == pushpin.Tag.GetType()) {
					multicityCarsLayer.Children.Add(pushpin);
				} else if (typeof(MulticityChargerMarker) == pushpin.Tag.GetType()) {
					multicityChargingLayer.Children.Add(pushpin);
				} else if (typeof(DriveNowCarInformation) == pushpin.Tag.GetType()) {
					driveNowCarsLayer.Children.Add(pushpin);
				}
			}
			((Border)pushpin.Content).Visibility = Visibility.Collapsed;
			pushpin.Opacity = .6;
		}
		public void LoadFreeCars() {

		}

		private void OnMapTap(object sender, System.Windows.Input.GestureEventArgs e) {
			foreach (var pushpin in activeLayer.Children.ToArray()) {
				if (pushpin.GetType() == typeof(Pushpin)) {
					DeactivatePushpin(pushpin as Pushpin);
				}
			}
			e.Handled = true;
		}

		private void OnMapViewChangeEnd(object sender, MapEventArgs e) {
			((App)Application.Current).RefreshPOIs();
		}

		private void OnSDKAddControlErrorOccured(object sender, AdErrorEventArgs e) {
			((Microsoft.Advertising.Mobile.UI.AdControl)sender).Visibility = Visibility.Collapsed;
			AdDuplexAdControl.Visibility = Visibility.Visible;
		}
		private void OnAdduplexAddControlErrorOccured(object sender, AdLoadingErrorEventArgs e) {
			((AdControl)sender).Visibility = Visibility.Visible;
			SDKAdControl.Visibility = Visibility.Collapsed;
		}

		private void OnSDKAddControlAdRefreshed(object sender, EventArgs e) {
			SDKAdControl.Visibility = Visibility.Visible;
			AdDuplexAdControl.Visibility = Visibility.Collapsed;
		}

		private void OnAdduplexAddControlAdRefreshed(object sender, AdLoadedEventArgs e) {
			AdDuplexAdControl.Visibility = Visibility.Visible;
			SDKAdControl.Visibility = Visibility.Collapsed;
		}
	}
}
