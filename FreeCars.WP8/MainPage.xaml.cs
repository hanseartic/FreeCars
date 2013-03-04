using System;
using System.Device.Location;
using System.IO.IsolatedStorage;
using System.Windows;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using FreeCars.Resources;
using Windows.Devices.Geolocation;

namespace FreeCars {
	public partial class MainPage {
		private Geolocator geolocator;
		// Constructor
		public MainPage() {
			InitializeComponent();
			geolocator = new Geolocator();
			geolocator.DesiredAccuracy = PositionAccuracy.High;
			// Sample code to localize the ApplicationBar
			BuildLocalizedApplicationBar();
			DataContext = this;
		}

		private void BuildLocalizedApplicationBar() {
			// Set the page's ApplicationBar to a new instance of ApplicationBar.
			ApplicationBar = new ApplicationBar();

			// Create a new button and set the text value to the localized string from AppResources.
			var appBarButton = new ApplicationBarIconButton(new Uri("/Assets/AppBar/appbar.map.centerme.rest.png", UriKind.Relative)) {
				Text = AppResources.AppBarButtonText
			};
			appBarButton.Click += (sender, args) => {
				ShowMyPositionOnMap();
				//Map.SetView(position.Coordinate.ToGeoCoordinate(), 14);
				//MyPositionGeoCoordinate = position.Coordinate.ToGeoCoordinate();
			};
			ApplicationBar.Buttons.Add(appBarButton);

			// Create a new menu item with the localized string from AppResources.
			ApplicationBarMenuItem appBarMenuItem = new ApplicationBarMenuItem(AppResources.AppBarMenuItemText);
			ApplicationBar.MenuItems.Add(appBarMenuItem);
		}

		protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e) {
			base.OnNavigatedTo(e);
			CheckForGPSUsage();
			
		}

		private void CheckForGPSUsage() {
			if (IsolatedStorageSettings.ApplicationSettings.Contains("settings_use_GPS") && 
				(bool)IsolatedStorageSettings.ApplicationSettings["settings_use_GPS"]) {
				return;
			}
			var mbResult = MessageBox.Show(AppResources.AllowUseOfGPSQuestion, AppResources.AllowUseOfGPSQuestionHeader,
			                               MessageBoxButton.OKCancel);
			IsolatedStorageSettings.ApplicationSettings["settings_use_GPS"] = (MessageBoxResult.OK == mbResult);
		}
		private async void ShowMyPositionOnMap() {
			var coordinate = (await geolocator.GetGeopositionAsync()).Coordinate;
			Map.SetView(coordinate.ToGeoCoordinate(), 14);
			MyPositionGeoCoordinate = coordinate.ToGeoCoordinate();
		}

		public static readonly DependencyProperty MyPositionVisibilityProperty =
			DependencyProperty.Register("MyPositionVisibility", typeof (Visibility), typeof (MainPage), new PropertyMetadata(Visibility.Collapsed));

		public Visibility MyPositionVisibility {
			get { return (Visibility)GetValue(MyPositionVisibilityProperty); }
			set { SetValue(MyPositionVisibilityProperty, value); }
		}
		public static readonly DependencyProperty MyPositionGeoCoordinateProperty =
			DependencyProperty.Register("MyPositionGeoCoordinate", typeof(GeoCoordinate), typeof(MainPage),
			new PropertyMetadata(default(Geocoordinate), (o, args) => { o.SetValue(MyPositionVisibilityProperty, args.NewValue == null ? Visibility.Collapsed : Visibility.Visible); }));

		public GeoCoordinate MyPositionGeoCoordinate {
			get { return (GeoCoordinate)GetValue(MyPositionGeoCoordinateProperty); }
			set { SetValue(MyPositionGeoCoordinateProperty, value); }
		}
	}
}
