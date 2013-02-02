
using System;
using System.Collections.Generic;
using System.Device.Location;
using System.Globalization;
using System.IO.IsolatedStorage;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace FreeCars {
	public class Car2Go : DependencyObject {
		public List<Car2GoInformation> Car2GoCars { get; private set; }
		public Car2Go() {
			Car2GoCars = new List<Car2GoInformation>();
			LoadCities();
		}

		private GeoPosition<GeoCoordinate> position;
		private const string consumerkey = FreeCarsCredentials.Car2Go.ConsumerKey;
		public void LoadPOIs() {
			try {
				position = (GeoPosition<GeoCoordinate>)IsolatedStorageSettings.ApplicationSettings["my_last_location"];
			} catch (KeyNotFoundException) {
				position = null;
				return;
			}
			LoadCars();
		}
		private void LoadCars() {
			if (null == position) return;
			try {
				if (false == (bool)IsolatedStorageSettings.ApplicationSettings["settings_show_car2go_cars"]) {
					Car2GoCars = new List<Car2GoInformation>();
					if (null != Updated) {
						Updated(this, null);
					}
					return;
				}
			} catch (KeyNotFoundException) { }
			
			var wc = new WebClient();
			var callUri = "https://www.car2go.com/api/v2.1/vehicles?loc="+City+"&format=json&oauth_consumer_key=" + consumerkey;

			wc.OpenReadCompleted += OnCar2GoCarsOpenReadCompleted;
			wc.OpenReadAsync(new Uri(callUri));
		}

		public static string City {
			get {
				var city = "ulm"; // fallback
				var setCity = (string)App.GetAppSetting("car2goSelectedCity");
				if (null == setCity || "Autodetect" == setCity) {
					setCity = (string)App.GetAppSetting("current_map_city");
				}
				city = (null != setCity)
					? setCity.ToLower()
					: city;
				return city;
			}
		}
		private void OnCar2GoCarsOpenReadCompleted(object sender, OpenReadCompletedEventArgs e) {
			try {
				if (0 == e.Result.Length) return;
				try {
					var serializer = new DataContractJsonSerializer(typeof(Car2GoVehicleData));
					var car2GoData = (Car2GoVehicleData)serializer.ReadObject(e.Result);
					var car2GoCars = new List<Car2GoInformation>();
					var usCultureInfo = new CultureInfo("en-US");
					foreach (var car in car2GoData.placemarks) {
						GeoCoordinate carPosition = null;
						try {
							carPosition = new GeoCoordinate(
								double.Parse(car.coordinates[1], usCultureInfo.NumberFormat),
								double.Parse(car.coordinates[0], usCultureInfo.NumberFormat));
						} catch {}
						var carInfo = new Car2GoInformation {
							model = ("CE" == car.engineType) ? "C-Smart" : "Smart ElectricDrive",
							fuelState = car.fuel,
							position = carPosition,
							licensePlate = car.name,
							ID = car.vin,
							exterior = car.exterior,
							interior = car.interior,
						};
						car2GoCars.Add(carInfo);
					}
					Car2GoCars = car2GoCars;
					if (null != Updated) {
						Updated(this, null);
					}
				} catch (NullReferenceException) {
				}
			} catch (WebException) { }
		}
		private void LoadCities() {
			var wc = new WebClient();
			var callUri = "https://www.car2go.com/api/v2.1/locations?&format=json&oauth_consumer_key=" + consumerkey;

			wc.OpenReadCompleted +=
				delegate(object o, OpenReadCompletedEventArgs e) {
					var cities = new List<Car2GoLocation>();
					try {
						var serializer = new DataContractJsonSerializer(typeof(Car2GoLocations));
					    cities = new List<Car2GoLocation>(((Car2GoLocations)serializer.ReadObject(e.Result)).location);
						
					} catch (WebException) { } catch (NullReferenceException) { }
					cities.Insert(0, new Car2GoLocation { locationName = "Autodetect", locationId = 0, });
					cities.Insert(0, null);
					Cities = cities;
				};
			wc.OpenReadAsync(new Uri(callUri));
		}
		public static readonly DependencyProperty CitiesProperty = DependencyProperty.Register(
			"Cities",
			typeof(List<Car2GoLocation>),
			typeof(Car2Go),
			new PropertyMetadata(new List<Car2GoLocation>())
		);

		public List<Car2GoLocation> Cities {
			get { return (List<Car2GoLocation>)GetValue(CitiesProperty); }
			set { SetValue(CitiesProperty, value); }
		}

		public event EventHandler Updated;
	}
}
