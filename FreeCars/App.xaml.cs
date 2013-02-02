using System;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.IO;
using System.Text;
using System.Runtime.Serialization.Json;
using System.Runtime.Serialization;
using Microsoft.Phone.Controls.Maps;
using Microsoft.Phone.Marketplace;

namespace FreeCars {
    public partial class App : Application {
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

		internal static void SetAppSetting(string key, object value) {
			try {
				IsolatedStorageSettings.ApplicationSettings.Add(key, value);
			} catch (ArgumentException) {
				IsolatedStorageSettings.ApplicationSettings[key] = value;
			}
		}

		internal static object GetAppSetting(string key) {
			try { 
				return IsolatedStorageSettings.ApplicationSettings[key];
			} catch (KeyNotFoundException) {
				return null;
			}
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