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
using FreeCars.Resources;

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

						ApplicationBar.Buttons.Add(mainPageApplicationBarCentermeButton);
						ApplicationBar.Buttons.Add(mainPageApplicationBarSettingsButton);
				}

				void OnMainPageApplicationBarCentermeButtonClick(object sender, EventArgs e) {
                    myLocationPushpin.Visibility = Visibility.Collapsed;
                    if (!gpsAllowed && !checkForGPSUsage()) { return; }
						cw.Start();
				}
				void OnMainPageApplicationBarSettingsButtonClick(object sender, EventArgs e) {
                    NavigationService.Navigate(new Uri("/SettingsPage.xaml", UriKind.RelativeOrAbsolute));
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

				void OnCarsUpdated(object sender, EventArgs e) {
						if (sender.GetType() == typeof(Multicity)) {
								UpdateMulticityLayers((Multicity)sender);
						}
						if (sender.GetType() == typeof(DriveNow)) {
								UpdateDriveNowLayer((DriveNow)sender);
						}                
				}
				void UpdateDriveNowLayer(DriveNow driveNow) {
						var myLocation = myLocationPushpin.Location;
						var cultureInfo = new CultureInfo("en-US");
						var driveNowBrush = new SolidColorBrush(Colors.Brown);
						driveNowCarsLayer.Children.Clear();
						foreach (var car in driveNow.DriveNowCars) {
								var coordinate = new GeoCoordinate(
										Double.Parse(car.position.latitude, cultureInfo.NumberFormat),
										Double.Parse(car.position.longitude, cultureInfo.NumberFormat)
								);
								var distance = (int)coordinate.GetDistanceTo(myLocation);
								if (1500 < distance) continue;
								var pushpin = new Pushpin {
										Location = coordinate,
										Name = car.licensePlate,
										Background = driveNowBrush,
										Opacity = .6,
										Content = distance + " m",
								};
								pushpin.Tap += OnPushpinTap;
								driveNowCarsLayer.Children.Add(pushpin);
						}
				}
				void UpdateMulticityLayers(Multicity multicity) {
						var myLocation = myLocationPushpin.Location;
						var cultureInfo = new CultureInfo("en-US");
						var multcityCarsBrush = new SolidColorBrush(Colors.Red);
						var multcityChargersBrush = new SolidColorBrush(Colors.Blue);
						multicityCarsLayer.Children.Clear();
						var tempList = new List<Pushpin>();
						foreach (var car in multicity.MulticityCars) {
								try {
										var coordinate = new GeoCoordinate(
																		Double.Parse(car.lat, cultureInfo.NumberFormat),
																		Double.Parse(car.lng, cultureInfo.NumberFormat));
										var distance = (int)coordinate.GetDistanceTo(myLocation);
										if (1500 < distance) continue;
										var pushpinContent = new Border {
												Child = new StackPanel {
														Orientation = System.Windows.Controls.Orientation.Vertical,
														Children = {
														new TextBlock { Text = Strings.Multicity, },
														new TextBlock { Text = car.hal2option.tooltip, },
																
														},
												},
												Visibility = Visibility.Collapsed,
										};
										var pushpin = new Pushpin() {
												Location = coordinate,
												Name = car.hal2option.tooltip,
												Opacity = .6,
												Background = multcityCarsBrush,
												Content = pushpinContent,
												//Content = distance + " m",
										};
										pushpin.Tap += OnPushpinTap;
										multicityCarsLayer.Children.Add(pushpin);
								} catch (ArgumentException) { }
						}
						multicityFlinksterLayer.Children.Clear();
						multicityChargingLayer.Children.Clear();
						foreach (var station in multicity.MulticityChargers) {
								var coordinate = new GeoCoordinate(
										Double.Parse(station.lat, cultureInfo.NumberFormat),
										Double.Parse(station.lng, cultureInfo.NumberFormat));
								var pushpin = new Pushpin() {
										Location = coordinate,
										Name = station.hal2option.tooltip,
										Opacity = .6,
										Background = multcityChargersBrush,
								};
						}

				}
				void OnPushpinTap(object sender, System.Windows.Input.GestureEventArgs e) {

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
										((Pushpin)sender).Tag = parentLayer;
										((Pushpin)sender).Opacity = 1;
										((Border)pushpinContent).Visibility = Visibility.Visible;

								} else {
										DeactivatePushpin(sender as Pushpin);
								}
						}
						e.Handled = true;
				}
				void DeactivatePushpin(Pushpin pushpin) {
						if (null != pushpin.Tag && typeof(MapLayer) == pushpin.Tag.GetType()) {
								activeLayer.Children.Remove(pushpin);
								((MapLayer)pushpin.Tag).Children.Add(pushpin);
								pushpin.Tag = null;
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
				}
		}
}
