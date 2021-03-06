﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows;
using System.Windows.Navigation;
using System.Xml;
using FreeCars.Providers;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Info;
using Microsoft.Phone.Notification;
using Microsoft.Phone.Scheduler;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Controls.Maps;
using Microsoft.Phone.Marketplace;

namespace FreeCars {
	public partial class App {
		/// <summary>
		/// Provides easy access to the root frame of the Phone Application.
		/// </summary>
		/// <returns>The root frame of the Phone Application.</returns>
		public PhoneApplicationFrame RootFrame { get; private set; }

		/// <summary>
		/// Constructor for the Application object.
		/// </summary>
		public App() {
			// Global handler for uncaught exceptions. 
			UnhandledException += Application_UnhandledException;

			// Standard Silverlight initialization
			InitializeComponent();

			// Phone-specific initialization
			InitializePhoneApplication();

			// Show graphics profiling information while debugging.
			if (System.Diagnostics.Debugger.IsAttached) {
				// Display the current frame rate counters.
				Application.Current.Host.Settings.EnableFrameRateCounter = true;

				// Show the areas of the app that are being redrawn in each frame.
				//Application.Current.Host.Settings.EnableRedrawRegions = true;

				// Enable non-production analysis visualization mode, 
				// which shows areas of a page that are handed off to GPU with a colored overlay.
				//Application.Current.Host.Settings.EnableCacheVisualization = true;

				// Disable the application idle detection by setting the UserIdleDetectionMode property of the
				// application's PhoneApplicationService object to Disabled.
				// Caution:- Use this under debug mode only. Application that disables user idle detection will continue to run
				// and consume battery power when the user is not using the phone.
				// PhoneApplicationService.Current.UserIdleDetectionMode = IdleDetectionMode.Disabled;
			}
			PhoneApplicationService.Current.ApplicationIdleDetectionMode = IdleDetectionMode.Disabled;
		}
		private void ValidateTrialMode() {
			IsInTrialMode = new LicenseInformation().IsTrial();
			TriggerTrialModeChanged();
		}
		public static bool IsInTrialMode {
			get;
			private set;
		}
		// Code to execute when the application is launching (eg, from Start)
		// This code will not execute when the application is reactivated
		private void Application_Launching(object sender, LaunchingEventArgs e) {
			ValidateTrialMode();
			var multicity = new Multicity();
			multicity.Updated += OnLayerUpdated;
			multicity.LoadPOIs();
			this.Resources.Add("multicity", multicity);

			var driveNow = new DriveNow();
			driveNow.Updated += OnLayerUpdated;
			driveNow.LoadPOIs();
			this.Resources.Add("driveNow", driveNow);

			var car2Go = new Car2Go();
			car2Go.Updated += OnLayerUpdated;
			car2Go.LoadPOIs();
			this.Resources.Add("car2go", car2Go);
			StartFlurry();
			UpdateFlipTile(
				"", "FreeCars", "\n     car2go\n   DriveNow\n    multicity", "Find free rides around you - car2go, DriveNow, multicity", 0,
				new Uri("/", UriKind.Relative),
				null, //new Uri("/ApplicationIcon.png", UriKind.Relative),
				null,
				null,
				new Uri("/Resources/wide_back_tile.png", UriKind.Relative),
				null);

			var currentVersion = GetAppAttribute("Version");
			var lastAppVersion = (string)GetAppSetting("last_version");
			if (null != lastAppVersion && lastAppVersion == currentVersion) {
				IsFirstLaunch = false;
			} else {
				IsFirstLaunch = true;
			}
			SetAppSetting("last_version", currentVersion);
		}
		private void StartFlurry() {
			try {
				if (false == (bool)IsolatedStorageSettings.ApplicationSettings["settings_allow_analytics"]) return;
			} catch (KeyNotFoundException) { }
			// only use flurry if the user has allowed it.
			FlurryWP7SDK.Api.SetSecureTransportEnabled();
			FlurryWP7SDK.Api.SetSessionContinueSeconds(120);
			FlurryWP7SDK.Api.SetVersion(GetAppAttribute("Version"));
			FlurryWP7SDK.Api.StartSession("QSJ5BJB37BNTT862WT8G");
		}
		private void OnLayerUpdated(object sender, EventArgs e) {
			var ea = e as CarsUpdatedEventArgs;
			if (ea != null) {
				var ce = ea;
				TriggerCarsUpdated(sender, ce.Type == CarsUpdatedEventArgs.UpdateType.Refresh, ce.CurrentSubsetName);
			} else {
				TriggerCarsUpdated(sender);
			}
		}

		public List<Pushpin> POIs { get; private set; }
		public void RefreshPOIs() {
			try {
				TriggerCarsUpdated(Resources["multicity"] as Multicity, true);
			} catch { }
			try {
				TriggerCarsUpdated(Resources["driveNow"] as DriveNow, true);
			} catch { }
			try {
				TriggerCarsUpdated(Resources["car2go"] as Car2Go, true);
			} catch { }
		}
		public void ReloadPOIs() {
			try {
				(Resources["multicity"] as Multicity).LoadPOIs();
			} catch { }
			try {
				(Resources["driveNow"] as DriveNow).LoadPOIs();
			} catch { }
			try {
				(Resources["car2go"] as Car2Go).LoadPOIs();
			} catch { }
		}
		public event EventHandler<CarsUpdatedEventArgs> CarsUpdated;
		public event EventHandler TrialModeChanged;
		// Code to execute when the application is activated (brought to foreground)
		// This code will not execute when the application is first launched
		private void Application_Activated(object sender, ActivatedEventArgs e) {
			ValidateTrialMode();
			StartFlurry();
		}
		private void TriggerCarsUpdated(object sender, bool isRefresh = false, String subset = "") {
			if (null != CarsUpdated) {
				CarsUpdated(sender, new CarsUpdatedEventArgs(isRefresh
					? CarsUpdatedEventArgs.UpdateType.Refresh
					: CarsUpdatedEventArgs.UpdateType.Reload, subset));
			}
		}
		private void TriggerTrialModeChanged() {
			if (null != TrialModeChanged) {
				TrialModeChanged(this, null);
			}
		}
		public static ShellTile CheckIfTileExists(string tileUri) {
			var shellTile = ShellTile.ActiveTiles.FirstOrDefault(
				tile => tile.NavigationUri.ToString().Contains(tileUri));
			return shellTile;
		}
		private static readonly Version targetedVersion = new Version(7, 10, 8858);
		public static bool LeastVersionIs78 { get { return Environment.OSVersion.Version >= targetedVersion; } }
		public static void UpdateFlipTile(
			string title,
			string backTitle,
			string backContent,
			string wideBackContent,
			int count,
			Uri tileUri,
			Uri smallBackgroundImage,
			Uri backgroundImage,
			Uri backBackgroundImage,
			Uri wideBackgroundImage,
			Uri wideBackBackgroundImage) {

			if (LeastVersionIs78) {
				// Loop through any existing Tiles that are pinned to Start.
				foreach (var tileToUpdate in ShellTile.ActiveTiles) {
					// Look for a match based on the Tile's NavigationUri (tileUri).
					if (tileToUpdate.NavigationUri.ToString() == tileUri.ToString()) {

						// Get the ShellTile type so we can call the new version of "Update" that takes the new Tile templates.
						var shellTileType = Type.GetType("Microsoft.Phone.Shell.ShellTile, Microsoft.Phone");
						// Get the constructor for the new FlipTileData class and assign it to our variable to hold the Tile properties.
						var updateTileData = CreateFlipTileData();
						if (null == updateTileData) return;
						// Set the properties.

						SetProperty(updateTileData, "Title", title);
						SetProperty(updateTileData, "Count", count);
						SetProperty(updateTileData, "BackTitle", backTitle);
						SetProperty(updateTileData, "BackContent", backContent);
						SetProperty(updateTileData, "SmallBackgroundImage", smallBackgroundImage);
						SetProperty(updateTileData, "BackgroundImage", backgroundImage);
						SetProperty(updateTileData, "BackBackgroundImage", backBackgroundImage);
						SetProperty(updateTileData, "WideBackgroundImage", wideBackgroundImage);
						SetProperty(updateTileData, "WideBackBackgroundImage", wideBackBackgroundImage);
						SetProperty(updateTileData, "WideBackContent", wideBackContent);

						// Invoke the new version of ShellTile.Update.
						shellTileType.GetMethod("Update").Invoke(tileToUpdate, new[] { updateTileData });
						break;
					}
				}
			}
		}

		public static object CreateFlipTileData() {
			try {
				// Get the new FlipTileData type.
				Type flipTileDataType = Type.GetType("Microsoft.Phone.Shell.FlipTileData, Microsoft.Phone");
				var flipTileData = flipTileDataType.GetConstructor(new Type[] { }).Invoke(null);
				return flipTileData;
			} catch (NullReferenceException) {
				return null;
			}
		}
		public static object CreateIconicTileData() {
			try {
				// Get the new FlipTileData type.
				Type flipTileDataType = Type.GetType("Microsoft.Phone.Shell.FlipTileData, Microsoft.Phone");
				var flipTileData = flipTileDataType.GetConstructor(new Type[] { }).Invoke(null);
				return flipTileData;
			} catch (NullReferenceException) {
				return null;
			}
		}
		// Code to execute when the application is deactivated (sent to background)
		// This code will not execute when the application is closing
		private void Application_Deactivated(object sender, DeactivatedEventArgs e) {
		}

		// Code to execute when the application is closing (eg, user hit Back)
		// This code will not execute when the application is deactivated
		private void Application_Closing(object sender, ClosingEventArgs e) {
		}

		// Code to execute if a navigation fails
		private void RootFrame_NavigationFailed(object sender, NavigationFailedEventArgs e) {
			if (System.Diagnostics.Debugger.IsAttached) {
				// A navigation has failed; break into the debugger
				System.Diagnostics.Debugger.Break();
			}
		}

		// Code to execute on Unhandled Exceptions
		private void Application_UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e) {
			if (System.Diagnostics.Debugger.IsAttached) {
				// An unhandled exception has occurred; break into the debugger
				System.Diagnostics.Debugger.Break();
			}
			FlurryWP7SDK.Api.LogError("Uncaught Exception occured", e.ExceptionObject);
			//e.Handled = true;
		}
		public static void SetProperty(object instance, string name, object value) {
			var setMethod = instance.GetType().GetProperty(name).GetSetMethod();
			setMethod.Invoke(instance, new object[] { value });
		}
		public static bool IsCertified { get; set; }

		private static bool? isLowMemoryDevice = null;
		internal static bool IsLowMemoryDevice {
			get {
				if (null == isLowMemoryDevice) {
					var result = true;
					try {
						// On newer devices we can check for the available memory
						var applicationWorkingSetLimit = (Int64)DeviceExtendedProperties.GetValue("ApplicationWorkingSetLimit");
						if (applicationWorkingSetLimit >= 94371840L)
							result = false;
					} catch (ArgumentOutOfRangeException) {
						// Windows Phone OS update not installed, which indicates a 512-MB device. 
						result = true;
					}
					isLowMemoryDevice = result;
				}
				return (bool)isLowMemoryDevice;
			}
		}

		public static bool IsFirstLaunch { get; set; }

		internal static void SetAppSetting(string key, object value) {
			try {
				IsolatedStorageSettings.ApplicationSettings.Add(key, value);
			} catch (ArgumentException) {
				IsolatedStorageSettings.ApplicationSettings[key] = value;
			}
			try {
				IsolatedStorageSettings.ApplicationSettings.Save();
			} catch (IsolatedStorageException) {
			} catch (InvalidOperationException) { }
		}

		internal static object GetAppSetting(string key) {
			try {
				return IsolatedStorageSettings.ApplicationSettings[key];
			} catch (KeyNotFoundException) {
				return null;
			}
		}

		internal static bool ClearAppSetting(string key) {
			return IsolatedStorageSettings.ApplicationSettings.Remove(key);
		}
		internal static string GetAppAttribute(string attributeName) {
			string appManifestName = "WMAppManifest.xml";
			string appNodeName = "App";

			var settings = new XmlReaderSettings();
			settings.XmlResolver = new XmlXapResolver();

			using (XmlReader rdr = XmlReader.Create(appManifestName, settings)) {
				rdr.ReadToDescendant(appNodeName);
				if (!rdr.IsStartElement()) {
					throw new System.FormatException(appManifestName + " is missing " + appNodeName);
				}
				return rdr.GetAttribute(attributeName);
			}
		}

		/*
		#region Radar tasks
		private const String FreeCarsRadarTaskName = "FreeCars.RadarAgent";
		private const String FreeCarsRadarNotificationChannelName = "FreeCars.RadarAgentNotificationChannel";

		protected internal static void SetupRadarNotificationChannel() {
			var liveTileUpdateNotificationChannel = HttpNotificationChannel.Find(FreeCarsRadarNotificationChannelName);
			if (null == liveTileUpdateNotificationChannel) {
				liveTileUpdateNotificationChannel = new HttpNotificationChannel(FreeCarsRadarNotificationChannelName);
				liveTileUpdateNotificationChannel.ChannelUriUpdated += 
					OnLiveTileUpdateNotificationChannelChannelUriUpdated;
				liveTileUpdateNotificationChannel.ErrorOccurred += 
					OnLiveTileUpdateNotificationChannelErrorOccurred;
				liveTileUpdateNotificationChannel.ConnectionStatusChanged += 
					OnLiveTileUpdateNotificationChannelConnectionStatusChanged;
				liveTileUpdateNotificationChannel.ShellToastNotificationReceived +=
					OnLiveTileUpdateNotificationChannelShellToastNotificationReceived;
				liveTileUpdateNotificationChannel.HttpNotificationReceived += OnLiveTileUpdateNotificationChannelHttpNotificationReceived;
				liveTileUpdateNotificationChannel.Open();
			} else {
				// completely re-setup the channel to circumvent the 500 messages/day limit
				liveTileUpdateNotificationChannel.Close();
				liveTileUpdateNotificationChannel.ChannelUriUpdated -= 
					OnLiveTileUpdateNotificationChannelChannelUriUpdated;
				liveTileUpdateNotificationChannel.ErrorOccurred -= 
					OnLiveTileUpdateNotificationChannelErrorOccurred;
				liveTileUpdateNotificationChannel.ConnectionStatusChanged -=
					OnLiveTileUpdateNotificationChannelConnectionStatusChanged;
				liveTileUpdateNotificationChannel.ShellToastNotificationReceived -=
					OnLiveTileUpdateNotificationChannelShellToastNotificationReceived;
				liveTileUpdateNotificationChannel.Dispose();
				SetupRadarNotificationChannel();
				return;
			}

			try {
				var radarTileUri =
					ShellTile.ActiveTiles.FirstOrDefault(x => x.NavigationUri.ToString().Contains("FreeCarsRadarTile")).NavigationUri;
				var radarTileUriString = Uri.EscapeUriString(radarTileUri.ToString());
				var tileUris = new Collection<Uri> {
					//radarTileUri
					new Uri("http://freecars.hanseartic.de", UriKind.Absolute)
				};
				RegisterForPushService(liveTileUpdateNotificationChannel.ChannelUri);
			} catch (NullReferenceException) { }
		}

		protected internal static void UnsetRadarNotificationChannel() {
			var liveTileUpdateNotificationChannel = HttpNotificationChannel.Find(FreeCarsRadarNotificationChannelName);
			if (null == liveTileUpdateNotificationChannel) {
				return;
			}
			if (liveTileUpdateNotificationChannel.IsShellTileBound) {
				liveTileUpdateNotificationChannel.UnbindToShellTile();
			}
			if (liveTileUpdateNotificationChannel.IsShellToastBound) {
				liveTileUpdateNotificationChannel.UnbindToShellToast();
			}
			liveTileUpdateNotificationChannel.Close();
			liveTileUpdateNotificationChannel.ChannelUriUpdated -=
				OnLiveTileUpdateNotificationChannelChannelUriUpdated;
			liveTileUpdateNotificationChannel.ErrorOccurred -=
				OnLiveTileUpdateNotificationChannelErrorOccurred;
			liveTileUpdateNotificationChannel.ConnectionStatusChanged -=
				OnLiveTileUpdateNotificationChannelConnectionStatusChanged;
			liveTileUpdateNotificationChannel.ShellToastNotificationReceived -=
				OnLiveTileUpdateNotificationChannelShellToastNotificationReceived;
			liveTileUpdateNotificationChannel.Dispose();
		}

		private static void OnLiveTileUpdateNotificationChannelHttpNotificationReceived(object sender, HttpNotificationEventArgs httpNotificationEventArgs) {
			// ONLY FOR RAW NOTIFICATIONS
		}

		private static void OnLiveTileUpdateNotificationChannelShellToastNotificationReceived(object sender, NotificationEventArgs e) {
			// NO ACTION IN RUNNIG APP
		}

		private static void OnLiveTileUpdateNotificationChannelConnectionStatusChanged(object sender, NotificationChannelConnectionEventArgs notificationChannelConnectionEventArgs) {
			// IGNORE FOR NOW
		}

		private static void OnLiveTileUpdateNotificationChannelErrorOccurred(object sender, NotificationChannelErrorEventArgs e) {
			switch (e.ErrorType) {
				case ChannelErrorType.ChannelOpenFailed:
					break;
				case ChannelErrorType.PayloadFormatError:
					break;
				case ChannelErrorType.MessageBadContent:
					break;
				case ChannelErrorType.NotificationRateTooHigh:
					break;
				case ChannelErrorType.PowerLevelChanged:
					if (e.ErrorAdditionalData == (int)ChannelPowerLevel.LowPowerLevel) {
						// TODO: notify user about deactivated push
					}
					break;
				case ChannelErrorType.Unknown:
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		protected internal static void SubscribeForRadarLocation(string tileId, string city) {
			jsonRequestFromServer("add_radar", new[] { tileId, city });
		}
		protected internal static void RegisterForPushService(Uri channelUri) {
			try {
				var pushUrl = channelUri.ToString();
				jsonRequestFromServer("update_user", new[] { pushUrl, GetAppAttribute("Version"), (IsInTrialMode ? "0" : "1") });
			} catch (NullReferenceException) { }
		}

		protected internal static void jsonRequestFromServer(String methodName, String[] parameters) {
			object anidObj;
			UserExtendedProperties.TryGetValue("ANID", out anidObj);
			var anid = string.Empty;
			if (anidObj != null && anidObj.ToString().Length >= (34)) {
				anid = anidObj.ToString().Substring(2, 32);
			}
			var jsonParams = "";
			if (0 < parameters.Length) {
				jsonParams = parameters.Aggregate(jsonParams, (current, param) => current + (", \"" + param + "\""));
			}
			var jsonRpc = "{\"jsonrpc\": \"2.0\", \"method\": \"" + methodName + "\", \"id\": \"" + DateTime.UtcNow.ToFileTime() + "\","
				+ " \"params\": [\"" + anid + "\"" + jsonParams + "] }";
			var wr = WebRequest.CreateHttp("http://freecars.hanseartic.de/server/json.php");
			wr.Method = "POST";
			wr.ContentType = "application/json";
			wr.BeginGetRequestStream(iar => {
				HttpWebRequest request = null;
				try {
					request = (HttpWebRequest)iar.AsyncState;
					var requestStream = request.EndGetRequestStream(iar);
					using (var streamWriter = new StreamWriter(requestStream)) {
						streamWriter.Write(jsonRpc);
						streamWriter.Flush();
						streamWriter.Close();
					}
				} catch { }
				request.BeginGetResponse(responseResult => {
					try {
						var responseRequest = (HttpWebRequest)responseResult.AsyncState;
						var response = (HttpWebResponse)responseRequest.EndGetResponse(responseResult);
						var responseStream = response.GetResponseStream();
						using (var streamReader = new StreamReader(CopyAndClose(responseStream))) {
							var responseStreamString = streamReader.ReadToEnd();
						}
					} catch {

					}

				}, request);
			}, wr);
		}

		private static Stream CopyAndClose(Stream inputStream) {
			const int readSize = 256;
			byte[] buffer = new byte[readSize];
			MemoryStream ms = new MemoryStream();

			int count = inputStream.Read(buffer, 0, readSize);
			while (count > 0) {
				ms.Write(buffer, 0, count);
				count = inputStream.Read(buffer, 0, readSize);
			}
			ms.Position = 0;
			inputStream.Close();
			return ms;
		}

		private static void OnLiveTileUpdateNotificationChannelChannelUriUpdated(object sender, NotificationChannelUriEventArgs e) {
			var liveTileUpdateNotificationChannel = (HttpNotificationChannel)sender;
			if (liveTileUpdateNotificationChannel.IsShellTileBound) {
				liveTileUpdateNotificationChannel.UnbindToShellTile();
			}
			if (liveTileUpdateNotificationChannel.IsShellToastBound) {
				liveTileUpdateNotificationChannel.UnbindToShellToast();
			}
			liveTileUpdateNotificationChannel.BindToShellTile();
			liveTileUpdateNotificationChannel.BindToShellToast();
			RegisterForPushService(e.ChannelUri);
		}
		#endregion
		*/
		#region Phone application initialization

		// Avoid double-initialization
		private bool phoneApplicationInitialized = false;

		// Do not add any additional code to this method
		private void InitializePhoneApplication() {
			if (phoneApplicationInitialized)
				return;

			// Create the frame but don't set it as RootVisual yet; this allows the splash
			// screen to remain active until the application is ready to render.
			RootFrame = new PhoneApplicationFrame();
			RootFrame.Navigated += CompleteInitializePhoneApplication;

			// Handle navigation failures
			RootFrame.NavigationFailed += RootFrame_NavigationFailed;

			// Ensure we don't initialize again
			phoneApplicationInitialized = true;
		}

		// Do not add any additional code to this method
		private void CompleteInitializePhoneApplication(object sender, NavigationEventArgs e) {
			// Set the root visual to allow the application to render
			if (RootVisual != RootFrame)
				RootVisual = RootFrame;

			// Remove this handler since it is no longer needed
			RootFrame.Navigated -= CompleteInitializePhoneApplication;
			//SetupRadarNotificationChannel();
		}

		#endregion
	}


	public class CarsUpdatedEventArgs : EventArgs {
		private UpdateType type;
		private string currentSubsetName;

		public enum UpdateType {
			Reload, Refresh,
		}

		public CarsUpdatedEventArgs()
			: this(UpdateType.Reload, "") {
		}
		public CarsUpdatedEventArgs(UpdateType updateType, String subsetName) {
			Type = updateType;
			CurrentSubsetName = subsetName;
		}
		public UpdateType Type {
			get { return type; }
			private set { type = value; }
		}
		public String CurrentSubsetName {
			get { return currentSubsetName; }
			private set { currentSubsetName = value; }
		}
	}

}