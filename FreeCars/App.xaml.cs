using System;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Windows;
using System.Windows.Navigation;
using System.Xml;
using FreeCars.Providers;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Info;
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
            TriggerCarsUpdated(sender);
        }
        public List<Pushpin> POIs { get; private set; }
		public void RefreshPOIs() {
			try {
					TriggerCarsUpdated(Resources["multicity"] as Multicity);
			} catch { }
			try {
					TriggerCarsUpdated(Resources["driveNow"] as DriveNow);
			} catch { }
			try {
					TriggerCarsUpdated(Resources["car2go"] as Car2Go);
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
        public event EventHandler CarsUpdated;
		public event EventHandler TrialModeChanged;
        // Code to execute when the application is activated (brought to foreground)
        // This code will not execute when the application is first launched
        private void Application_Activated(object sender, ActivatedEventArgs e) {
			ValidateTrialMode();
			StartFlurry();
        }
		private void TriggerCarsUpdated(object sender) {
			if (null != CarsUpdated) {
				CarsUpdated(sender, null);
			}
		}
		private void TriggerTrialModeChanged() {
			if (null != TrialModeChanged) {
				TrialModeChanged(this, null);
			}
		}
		public static ShellTile CheckIfTileExist(string tileUri) {
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
				var flipTileData = flipTileDataType.GetConstructor(new Type[] {}).Invoke(null);
				return flipTileData;
			} catch (NullReferenceException) {
				return null;
			}
		}
		public static object CreateIconicTileData() {
			try {
				// Get the new FlipTileData type.
				Type flipTileDataType = Type.GetType("Microsoft.Phone.Shell.FlipTileData, Microsoft.Phone");
				var flipTileData = flipTileDataType.GetConstructor(new Type[] {}).Invoke(null);
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
						// Check the working set limit and set the IsLowMemDevice flag accordingly.
						var applicationWorkingSetLimit = (Int64)DeviceExtendedProperties.GetValue("ApplicationWorkingSetLimit");
						if (applicationWorkingSetLimit >= 94371840L)
							result = false;
					} catch (ArgumentOutOfRangeException) {
						// Windows Phone OS update not installed, which indicates a 512-MB device. 
						result = false;
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
			} catch (InvalidOperationException) {}
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
        }

        #endregion
    }




}