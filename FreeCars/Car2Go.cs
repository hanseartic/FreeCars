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
	public class Car2Go {
		public List<Car2GoInformation> Car2GoCars { get; private set; }
		public Car2Go() {
			Car2GoCars = new List<Car2GoInformation>();
		}
		private GeoPosition<GeoCoordinate> position;
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
			var callUri = "https://www.car2go.com/api/v2.1/vehicles?loc=berlin&format=json&oauth_consumer_key=JohannesRuth";

			wc.OpenReadCompleted += OnCar2GoCarsOpenReadCompleted;
			wc.OpenReadAsync(new Uri(callUri));
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
							model = "Smart ForTwo",
							fuelState = car.fuel,
						    position = carPosition,
             				licensePlate = car.name,
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

		public event EventHandler Updated;
	}
}
