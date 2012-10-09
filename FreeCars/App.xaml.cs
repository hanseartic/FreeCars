using System;
using System.Collections.Generic;
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
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.IO;
using System.Text;
using System.Runtime.Serialization.Json;
using System.Runtime.Serialization;

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
                PhoneApplicationService.Current.UserIdleDetectionMode = IdleDetectionMode.Disabled;
            }

        }

        // Code to execute when the application is launching (eg, from Start)
        // This code will not execute when the application is reactivated
        private void Application_Launching(object sender, LaunchingEventArgs e) {
            var wc = new WebClient();
            wc.OpenReadCompleted += new OpenReadCompletedEventHandler(wc_OpenReadCompleted);
            wc.OpenReadAsync(new Uri("https://kunden.multicity-carsharing.de/kundenbuchung/hal2ajax_process.php?zoom=10&lng1=&lat1=&lng2=&lat2=&stadtCache=&mapstation_id=&mapstadt_id=&verwaltungfirma=&centerLng=13.382322739257802&centerLat=52.50734843957503&searchmode=buchanfrage&with_staedte=true&buchungsanfrage=J&lat=52.50734843957503&lng=13.382322739257802&instant_access=J&open_end=J&objectname=multicitymarker&clustername=multicitycluster&ignore_virtual_stations=J&before=null&after=null&ajxmod=hal2map&callee=getMarker&_=1349642335368"));
        }
        public List<FlinksterMarker> Cars { get; private set; }
        private void wc_OpenReadCompleted(object sender, OpenReadCompletedEventArgs e) {
            //Stream strm = e.Result;
            
            var serializer = new DataContractJsonSerializer(typeof(FlinksterData));
            var objects = (FlinksterData)serializer.ReadObject(e.Result);
            var cars = new List<FlinksterMarker>();
            foreach (var marker in objects.marker) {
                if (marker.hal2option.objectname == "multicitymarker") {
                    cars.Add(marker);
                }
            }
            Cars = cars;
            CarsLoaded.Invoke(this, new EventArgs());
            /*
            using (var reader = new StreamReader(mStream, Encoding.UTF8)) {
                string value = reader.ReadToEnd();

                Json.JsonArray jsonValues = (JsonArray)JsonArray.Load(value);
var test = null;
                // Do something with the value
            }
            */
        }
        public event EventHandler CarsLoaded;
        // Code to execute when the application is activated (brought to foreground)
        // This code will not execute when the application is first launched
        private void Application_Activated(object sender, ActivatedEventArgs e) {
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

    [DataContract]
    public class FlinksterData {
        [DataMember(Name = "statusCode")]
        public String statusCode { get; set; }
        [DataMember(Name = "statusText")]
        public String statusText { get; set; }
        [DataMember(Name = "marker")]
        public List<FlinksterMarker> marker { get; set; }

    }
    [DataContract]
    public class FlinksterMarker {
        [DataMember(Name = "lat")]
        public String lat { get; set; }
        [DataMember(Name = "lng")]
        public String lng { get; set; }
        [DataMember(Name = "iconName")]
        public String iconName { get; set; }
        [DataMember(Name = "iconNameSelected")]
        public String iconNameSelected { get; set; }
        [DataMember(Name = "hal2option")]
        public hal2option hal2option { get; set; }

    }
    [DataContract]
    public class hal2option {
        [DataMember(Name = "minZoom")]
        public String minZoom { get; set; }
        [DataMember(Name = "maxZoom")]
        public String maxZoom { get; set; }
        [DataMember(Name = "tooltip")]
        public String tooltip { get; set; }
        [DataMember(Name = "objectname")]
        public String objectname { get; set; }
    }
}