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
using FreeCars.Providers;
using FreeCars.Serialization;
using Microsoft.Phone.BackgroundTransfer;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.Phone.Controls.Maps;
using OAuth;

namespace FreeCars {
	public class Multicity : ProviderBase<MulticityMarker> {
		public Multicity() {
			FlinksterStations = new List<FlinksterStationMarker>();
			Markers = new List<MulticityMarker>();
			MulticityChargers = new List<MulticityChargerMarker>();
		}

		public override sealed List<MulticityMarker> Markers { get; protected set; }
		public List<FlinksterStationMarker> FlinksterStations { get; private set; }
		public List<MulticityChargerMarker> MulticityChargers { get; private set; }

		private GeoPosition<GeoCoordinate> position;

		public override void LoadPOIs() {
			try {
				position = (GeoPosition<GeoCoordinate>)IsolatedStorageSettings.ApplicationSettings["my_last_location"];
			} catch (KeyNotFoundException) {
				position = null;
				return;
			}
			LoadMulticityChargers();
			LoadMulticityCars();
		}
		private void LoadMulticityCars() {
			if (null == position) return;
			try {
				if (false == (bool)IsolatedStorageSettings.ApplicationSettings["settings_show_multicity_cars"]) {
					Markers = new List<MulticityMarker>();
					TriggerUpdated();
					return;
				}
			} catch (KeyNotFoundException) { }
			var wc = new WebClient();
			var cultureInfo = new CultureInfo("en-US");
			var lat = position.Location.Latitude.ToString(cultureInfo.NumberFormat);
			var lng = position.Location.Longitude.ToString(cultureInfo.NumberFormat);
			var callUri = "https://kunden.multicity-carsharing.de/kundenbuchung/hal2ajax_process.php?zoom=10&lng1=&lat1=&lng2=&lat2=&stadtCache=&mapstation_id=&mapstadt_id=&verwaltungfirma=&centerLng=" + lng + "&centerLat=" + lat + "&searchmode=buchanfrage&with_staedte=false&buchungsanfrage=J&lat=" + lat + "&lng=" + lng + "&instant_access=J&open_end=J&objectname=multicitymarker&clustername=multicitycluster&ignore_virtual_stations=J&before=null&after=null&ajxmod=hal2map&callee=getMarker&_=" + OAuthTools.GetTimestamp();
			wc.OpenReadCompleted += OnMulticityCarsOpenReadCompleted;
			wc.OpenReadAsync(new Uri(callUri));

		}
		private void OnMulticityCarsOpenReadCompleted(object sender, OpenReadCompletedEventArgs e) {
			try {
				var serializer = new DataContractJsonSerializer(typeof(MulticityData));
				var objects = (MulticityData)serializer.ReadObject(e.Result);
				var usCultureInfo = new CultureInfo("en-US");
				var multicityCars = new List<MulticityMarker>();
				foreach (var car in objects.marker) {
					if (car.hal2option.objectname == "multicitymarker") {
						car.licensePlate = HttpUtility.HtmlDecode(Regex.Match(car.hal2option.tooltip, @"\(([^)]*)\)").Groups[1].Value);
						car.model = HttpUtility.HtmlDecode(car.hal2option.tooltip.Substring(0, car.hal2option.tooltip.IndexOf("(")).Substring(1));
						try {
							car.position = new GeoCoordinate(
								double.Parse(car.lat, usCultureInfo.NumberFormat),
								double.Parse(car.lng, usCultureInfo.NumberFormat));
						} catch { }
						multicityCars.Add(car);
					}
				}
				Markers = multicityCars;
				TriggerUpdated();
			} catch { }
		}
		public static void LoadChargeState(Pushpin pushpin) {
			var car = (MulticityMarker)pushpin.Tag;
			var batteryRequestClient = new WebClient();
			batteryRequestClient.DownloadStringCompleted += (client, eventArgs) => {
				try {
					var result = eventArgs.Result.Substring(eventArgs.Result.IndexOf("chargepercent"));
					result = result.Substring(0, result.IndexOf("%") - 1).Substring(15);
					pushpin.Dispatcher.BeginInvoke((Action)(() => {
						car.fuelState = result;
					}));
				} catch (WebException) { } catch { }
			};
			batteryRequestClient.DownloadStringAsync(new Uri("https://kunden.multicity-carsharing.de/kundenbuchung/hal2ajax_process.php?infoConfig%5BinfoTyp%5D=HM_AUTO_INFO&infoConfig%5Bpopup%5D=J&infoConfig%5BobjectId%5D=" + car.hal2option.id + "&infoConfig%5Bobjecttyp%5D=carpos&infoConfig%5BmarkerInfos%5D=false&ajxmod=hal2map&callee=markerinfo"));
		}
		private void LoadMulticityChargers() {
			if (null == position) return;
			try {
				if (false == (bool)IsolatedStorageSettings.ApplicationSettings["settings_show_multicity_chargers"]) {
					MulticityChargers = new List<MulticityChargerMarker>();
					TriggerUpdated();
					return;
				}
			} catch (KeyNotFoundException) { }
			var wc = new WebClient();
			var cultureInfo = new CultureInfo("en-US");
			var callUri = "http://www.multicity-carsharing.de/rwe/json.php";
			wc.OpenReadCompleted += OnMulticityChargersOpenReadCompleted;
			wc.OpenReadAsync(new Uri(callUri));
		}
		Stream ConvertStream(Stream stream, Encoding fromEncoding, Encoding toEncoding) {
			var inBytes = new Byte[stream.Length];
			stream.Read(inBytes, 0, (int)stream.Length);
			string convertString = fromEncoding.GetString(inBytes, 0, inBytes.Length);
			// wrong place ATM, but has to be hacked in here ... :C
			convertString = convertString.Replace("\"click\": \"showMarkerInfos\",", "");
			var outBytes = toEncoding.GetBytes(convertString);
			return new MemoryStream(outBytes);
		}
		private void SerializeChargers(Stream inputStream) {
			if (0 == inputStream.Length) return;
			try {
				var dataStream = ConvertStream(inputStream, Encoding.UTF8, Encoding.GetEncoding("iso-8859-1"));
				inputStream.Close();

				var serializer = new DataContractJsonSerializer(typeof(MulticityChargerData));
				var objects = (MulticityChargerData)serializer.ReadObject(dataStream);
				var multicity_chargers = new List<MulticityChargerMarker>();
				foreach (var marker in objects.marker) {
					if ("rwemarker_crowded" == marker.hal2option.objectname || "rwemarker_vacant" == marker.hal2option.objectname) {
						multicity_chargers.Add(marker);
					}
				}
				MulticityChargers = multicity_chargers;
				TriggerUpdated();
			} catch (DecoderFallbackException) { } catch (NullReferenceException) { } catch {
				// TODO: repair correctly
				// there is something nasty going on in WP8
				// this does the job for now
			}
		}
		private void OnMulticityChargersOpenReadCompleted(object sender, OpenReadCompletedEventArgs e) {
			try {
				SerializeChargers(e.Result);
			} catch (WebException) { }
		}
		public event EventHandler Updated;
		private void TriggerUpdated() {
			if (null != Updated) {
				Updated(this, null);
			}
		}
	}

}
