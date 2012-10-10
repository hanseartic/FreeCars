using System;
using System.Net;
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

namespace FreeCars {
    public class DriveNow {
				public DriveNow() {
						DriveNowCars = new List<DriveNowCarInformation>();
        }

				public List<DriveNowCarInformation> DriveNowCars { get; private set; }

        private GeoPosition<GeoCoordinate> position;
        public void LoadPOIs() {
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
                    return;
                }
            } catch (KeyNotFoundException) { }
            var wc = new WebClient();
            var cultureInfo = new CultureInfo("en-US");
						var callUri = "https://www.drive-now.com/php/metropolis/json.vehicle_filter?tenant=germany&language=de_DE&L=0&url=%2Fphp%2Fmetropolis%2Fcity_berlin&redirect_flag=1";
						wc.OpenReadCompleted += OnDriveNowCarsOpenReadCompleted;
            wc.OpenReadAsync(new Uri(callUri));
        
        }
        private void OnDriveNowCarsOpenReadCompleted(object sender, OpenReadCompletedEventArgs e) {
            var serializer = new DataContractJsonSerializer(typeof(DriveNowData));
						var driveNowData = (DriveNowData)serializer.ReadObject(e.Result);
            var drivenow_cars = new List<DriveNowCarInformation>();
						foreach (var car in driveNowData.rec.vehicles.vehicles) {
								drivenow_cars.Add(car);
            }
						DriveNowCars = drivenow_cars;
            if (null != Updated) {
                Updated(this, null);
            }
        }
        public event EventHandler Updated;
    }
}
