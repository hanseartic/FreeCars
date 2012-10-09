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
using Microsoft.Phone.Controls.Maps;
using System.Device.Location;
using System.Globalization;
using System.Windows.Data;
using Microsoft.Phone.Shell;
using System.IO.IsolatedStorage;

namespace FreeCars {
		public partial class MainPage : PhoneApplicationPage {
				private GeoCoordinateWatcher cw;
				public MainPage() {
						InitializeComponent();
						((App)App.Current).CarsLoaded += OnCarsLoaded;
						cw = new GeoCoordinateWatcher(GeoPositionAccuracy.High);
						cw.MovementThreshold = 10;
						cw.PositionChanged += OnMyPositionChanged;
						Loaded += OnMainPageLoaded;
						//map.Children.Add(me);
						SetAppBar();
				}

				void OnMainPageLoaded(object sender, RoutedEventArgs e) {
						try {
								var position = (GeoPosition<GeoCoordinate>)IsolatedStorageSettings.ApplicationSettings["my_last_location"];
								ShowMeAtLocation(position.Location);
						} catch (KeyNotFoundException) {}
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

						ApplicationBar.Buttons.Add(mainPageApplicationBarCentermeButton);
						ApplicationBar.Buttons.Add(mainPageApplicationBarSettingsButton);
				}

				void OnMainPageApplicationBarCentermeButtonClick(object sender, EventArgs e) {
						myLocationPushpin.Visibility = Visibility.Collapsed;
						cw.Start();
				}
				void OnMainPageApplicationBarSettingsButtonClick(object sender, EventArgs e) {

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
				}
				void ShowMeAtLocation(GeoCoordinate location) {
						myLocationPushpin.Location = location;
						map.Center = location;
						map.ZoomLevel = 14.0;
						myLocationPushpin.Visibility = Visibility.Visible;
				}

				void OnCarsLoaded(object sender, EventArgs e) {
						var cultureInfo = new CultureInfo("en-US");
						foreach (var car in ((App)sender).Cars) {
								var coordinate = new GeoCoordinate(
										Double.Parse(car.lat, cultureInfo.NumberFormat),
										Double.Parse(car.lng, cultureInfo.NumberFormat));

								var pushpin = new Pushpin() {
										Location = coordinate,
										Name = car.hal2option.tooltip,
										Opacity = .3,
										Width = 20

								};
								pushpin.Tap += OnPushpinTap;
								map.Children.Add(pushpin);
								//pushpin.Location = 
						}  
				}

				void OnPushpinTap(object sender, GestureEventArgs e) {
						var provider = "Multicity";
						MessageBox.Show(((Pushpin)sender).Name.Replace("'", ""), provider, MessageBoxButton.OK);
				}
				public void LoadFreeCars() {
						
				}
		}
}
