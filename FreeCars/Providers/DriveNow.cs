using System;
using System.Net;
using System.Runtime.Serialization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Collections.Generic;
using System.Device.Location;
using System.IO.IsolatedStorage;
using System.Globalization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Text.RegularExpressions;
using FreeCars.Serialization;
using OAuth;

namespace FreeCars.Providers {
	public class DriveNow : ProviderBase<DriveNowMarker> {
		public DriveNow() {
			Markers = new List<DriveNowMarker>();
		}

		public override sealed List<DriveNowMarker> Markers { get; protected set; }
		private GeoPosition<GeoCoordinate> position;
		public override void LoadPOIs() {
			try {
				position = (GeoPosition<GeoCoordinate>)IsolatedStorageSettings.ApplicationSettings["my_last_location"];
			} catch (KeyNotFoundException) {
				position = null;
				return;
			}
			LoadDriveNowCars();
		}
		private void LoadDriveNowCars() {
			if (null == position) return;
			try {
				if (false == (bool)IsolatedStorageSettings.ApplicationSettings["settings_show_drivenow_cars"]) {
					Markers = new List<DriveNowMarker>();
					if (null != Updated) {
						Updated(this, null);
					}
					return;
				}
			} catch (KeyNotFoundException) { }
			var wc = new WebClient();
			//var callUri = "https://www.drive-now.com/php/metropolis/json.vehicle_filter?tenant=germany&language=de_DE&L=0&url=%2Fphp%2Fmetropolis%2Fcity_berlin&redirect_flag=1";
			var callUri = "https://de.drive-now.com/php/metropolis/json.vehicle_filter?timestamp=" + OAuthTools.GetTimestamp();

			wc.OpenReadCompleted += OnDriveNowCarsOpenReadCompleted;
			wc.OpenReadAsync(new Uri(callUri));

		}
		private void OnDriveNowCarsOpenReadCompleted(object sender, OpenReadCompletedEventArgs e) {
			try {
				if (0 == e.Result.Length) return;
				try {
					var serializer = new DataContractJsonSerializer(typeof(DriveNowData));
					var driveNowData = (DriveNowData)serializer.ReadObject(e.Result);
					var drivenow_cars = new List<DriveNowMarker>();
					Regex replaceMultipleSpaceWithOnlyOneSpaceRegex = new Regex(@"[ ]{2,}", RegexOptions.None);
					var usCultureInfo = new CultureInfo("en-US");
					foreach (var car in driveNowData.rec.vehicles.vehicles) {
						car.licensePlate = replaceMultipleSpaceWithOnlyOneSpaceRegex.Replace(car.licensePlate, @" ").Replace(" -", "-");
						try {
							car.position = new GeoCoordinate(
								double.Parse(car.dn_position.latitude, usCultureInfo.NumberFormat),
								double.Parse(car.dn_position.longitude, usCultureInfo.NumberFormat));
						} catch { }
						drivenow_cars.Add(car);
					}
					Markers = drivenow_cars;
					if (null != Updated) {
						Updated(this, null);
					}
				} catch (NullReferenceException) { } catch (ArgumentNullException) {
					LoadDriveNowCars();
				} catch (SerializationException) { }
			} catch (WebException) { }
		}
		public event EventHandler Updated;
	}
}
