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
    public class Multicity {
        public Multicity() {
            FlinsterStations = new List<IMarker>();
            MulticityCars = new List<MulticityMarker>();
            MulticityChargers = new List<MulticityChargerMarker>();
        }

        public List<MulticityMarker> MulticityCars { get; private set; }
        public List<IMarker> FlinsterStations { get; private set; }
        public List<MulticityChargerMarker> MulticityChargers { get; private set; }

        private GeoPosition<GeoCoordinate> position;
        public void LoadPOIs() {
            try {
                position = (GeoPosition<GeoCoordinate>)IsolatedStorageSettings.ApplicationSettings["my_last_location"];
            } catch (KeyNotFoundException) {
                position = null;
                return;
            }
            LoadMulticityCars();
            LoadMulticityChargers();
        }
        private void LoadMulticityCars() {
            if (null == position) return;
            try {
                if (false == (bool)IsolatedStorageSettings.ApplicationSettings["settings_show_multicity_cars"]) {
                    return;
                }
            } catch (KeyNotFoundException) { }
            var wc = new WebClient();
            var cultureInfo = new CultureInfo("en-US");
            var lat = position.Location.Latitude.ToString(cultureInfo.NumberFormat);
            var lng = position.Location.Longitude.ToString(cultureInfo.NumberFormat);
            var callUri = "https://kunden.multicity-carsharing.de/kundenbuchung/hal2ajax_process.php?zoom=10&lng1=&lat1=&lng2=&lat2=&stadtCache=&mapstation_id=&mapstadt_id=&verwaltungfirma=&centerLng=" + lng + "&centerLat=" + lat + "&searchmode=buchanfrage&with_staedte=false&buchungsanfrage=J&lat=" + lat + "&lng=" + lng + "&instant_access=J&open_end=J&objectname=multicitymarker&clustername=multicitycluster&ignore_virtual_stations=J&before=null&after=null&ajxmod=hal2map&callee=getMarker&_=1349642335368";
            wc.OpenReadCompleted += OnMulticityCarsOpenReadCompleted;
            wc.OpenReadAsync(new Uri(callUri));
        
        }
        private void OnMulticityCarsOpenReadCompleted(object sender, OpenReadCompletedEventArgs e) {
            var serializer = new DataContractJsonSerializer(typeof(MulticityData));
            var objects = (MulticityData)serializer.ReadObject(e.Result);
            var multicity_cars = new List<MulticityMarker>();
            foreach (var marker in objects.marker) {
                if (marker.hal2option.objectname == "multicitymarker") {
                    multicity_cars.Add(marker);
                }
            }
            MulticityCars = multicity_cars;
            if (null != Updated) {
                Updated(this, null);
            }
        }
        private void LoadMulticityChargers() {
            if (null == position) return;
            try {
                if (false == (bool)IsolatedStorageSettings.ApplicationSettings["settings_show_multicity_chargers"]) {
                    return;
                }
            } catch (KeyNotFoundException) { }
            var wc = new WebClient();
            var cultureInfo = new CultureInfo("en-US");
            var callUri = "https://www.multicity-carsharing.de/rwe/json.php";
            wc.OpenReadCompleted += OnMulticityChargersOpenReadCompleted;
            wc.OpenReadAsync(new Uri(callUri));
        }

        private void OnMulticityChargersOpenReadCompleted(object sender, OpenReadCompletedEventArgs e) {
            try {
                var serializer = new DataContractJsonSerializer(typeof(MulticityChargerData));
                var objects = (MulticityChargerData)serializer.ReadObject(e.Result);
                var multicity_chargers = new List<MulticityChargerMarker>();
                foreach (var marker in objects.marker) {
                    if ("rwemarker_crowded" == marker.hal2option.objectname || "rwemarker_vacant" == marker.hal2option.objectname) {
                        multicity_chargers.Add(marker);
                    }
                }
                MulticityChargers = multicity_chargers;
                if (null != Updated) {
                    Updated(this, null);
                }
            } catch (DecoderFallbackException) { }
        }
        public event EventHandler Updated;
    }
    
}
