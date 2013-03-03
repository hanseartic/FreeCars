
using System;
using System.Collections.Generic;
using System.Device.Location;
using System.Diagnostics;
using System.Globalization;
using System.IO.IsolatedStorage;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Windows;
using FreeCars.Serialization;
using OAuth;

namespace FreeCars {
	public class Car2Go : DependencyObject {
		public List<Car2GoMarker> Car2GoCars { get; private set; }
		public Car2Go() {
			Car2GoCars = new List<Car2GoMarker>();
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
			LoadBookedCars();
			LoadCars();
		}
		private void LoadCars() {
			if (null == position) return;
			try {
				if (false == (bool)IsolatedStorageSettings.ApplicationSettings["settings_show_car2go_cars"]) {
					Car2GoCars = new List<Car2GoMarker>();
					if (null != Updated) {
						Updated(this, null);
					}
					return;
				}
			} catch (KeyNotFoundException) { }
			
			var wc = new WebClient();
			var callUri = "https://www.car2go.com/api/v2.1/vehicles?loc=" + City + "&format=json&oauth_consumer_key=" + consumerkey + "&timestamp=" + OAuthTools.GetTimestamp();

			wc.OpenReadCompleted += OnCar2GoCarsOpenReadCompleted;
			wc.OpenReadAsync(new Uri(callUri));
		}
		public bool HasBooking { get; private set; }

		private DateTime lastBookedCarsUpdate = DateTime.MinValue;
		private void LoadBookedCars() {
			HasBooking = false;
			try {
				var token = (string)App.GetAppSetting("car2go.oauth_token");
				var tokenSecret = (string)App.GetAppSetting("car2go.oauth_token_secret");
				if (null == token || null == tokenSecret) return;

				const string car2GoRequestEndpoint = "https://www.car2go.com/api/v2.1/booking";
				var parameters = new WebParameterCollection {
					{"oauth_callback", "oob"},
					{"oauth_signature_method", "HMAC-SHA1"},
					{"oauth_token", token},
					{"oauth_version", "1.0"},
					{"oauth_consumer_key", consumerkey},
					{"oauth_timestamp", OAuthTools.GetTimestamp()},
					{"oauth_nonce", OAuthTools.GetNonce()},
					{"format", "json"},
					{"loc", City},
				};
				//parameters.Add("test", "1");
				var signatureBase = OAuthTools.ConcatenateRequestElements("GET", car2GoRequestEndpoint, parameters);
				var signature = OAuthTools.GetSignature(OAuthSignatureMethod.HmacSha1, OAuthSignatureTreatment.Escaped, signatureBase, FreeCarsCredentials.Car2Go.SharedSecred, tokenSecret);

				var requestParameters = OAuthTools.NormalizeRequestParameters(parameters);
				var requestUrl = new Uri(car2GoRequestEndpoint + "?" + requestParameters + "&oauth_signature=" + signature, UriKind.Absolute);

				var webClient = new WebClient();
				webClient.OpenReadCompleted += (sender, args) => {
					try {
						if (0 == args.Result.Length) return;
						try {
							var serializer = new DataContractJsonSerializer(typeof(Car2GoBookingResult));
							var bookingResult = (Car2GoBookingResult)serializer.ReadObject(args.Result);
							var car2GoCars = new List<Car2GoMarker>();
							if (0 == bookingResult.ReturnValue.Code) {
								if (bookingResult.Booking.Length > 0) {
									lastBookedCarsUpdate = DateTime.Now;
								}
								foreach (var booking in bookingResult.Booking) {
									var car = booking.Vehicle;
									GeoCoordinate carPosition = null;
									try {
										carPosition = new GeoCoordinate(car.Position.Latitude, car.Position.Longitude);
									} catch {}
									var carInfo = new Car2GoMarker {
										model = ("CE" == car.EngineType) ? "C-Smart" : "Smart ElectricDrive",
										fuelState = car.Fuel,
										position = carPosition,
										licensePlate = car.NumberPlate,
										ID = car.VIN,
										exterior = car.Exterior,
										interior = car.Interior,
										isBooked = true,
										BookingId = bookingResult.Booking[0].BookingId,
									};
									HasBooking = true;
									car2GoCars.Add(carInfo);
								}
								Car2GoCars = car2GoCars;
								if (null != Updated) {
									Updated(this, null);
								}
							}
						} catch (NullReferenceException) { }
					} catch (WebException) { }
				};

				webClient.OpenReadAsync(requestUrl);
			} catch (Exception e) {
				Console.WriteLine(e);
			}
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
			if (lastBookedCarsUpdate > DateTime.Now - TimeSpan.FromSeconds(20)) {
				return;
			}
			try {
				if (0 == e.Result.Length) return;
				try {
					var serializer = new DataContractJsonSerializer(typeof(Car2GoVehicleData));
					var car2GoData = (Car2GoVehicleData)serializer.ReadObject(e.Result);
					var car2GoCars = new List<Car2GoMarker>();
					var usCultureInfo = new CultureInfo("en-US");
					foreach (var car in car2GoData.placemarks) {
						GeoCoordinate carPosition = null;
						try {
							carPosition = new GeoCoordinate(
								double.Parse(car.coordinates[1], usCultureInfo.NumberFormat),
								double.Parse(car.coordinates[0], usCultureInfo.NumberFormat));
						} catch {}
						var carInfo = new Car2GoMarker {
							model = ("CE" == car.engineType) ? "C-Smart" : "Smart ElectricDrive",
							fuelState = car.fuel,
							position = carPosition,
							licensePlate = car.name,
							ID = car.vin,
							exterior = car.exterior,
							interior = car.interior,
							address = car.address,
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
			var callUri = "https://www.car2go.com/api/v2.1/locations?&format=json&oauth_consumer_key=" + consumerkey + "&timestamp=" + OAuthTools.GetTimestamp();

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
