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
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Controls.Maps;
using System.Device.Location;
using System.Globalization;

namespace FreeCars {
    public partial class MainPage : PhoneApplicationPage {
        // Constructor
        private Pushpin me;
        private bool firstLocationReceived = false;
        public MainPage() {
            InitializeComponent();
            ((App)App.Current).CarsLoaded += OnCarsLoaded;
            var cw = new GeoCoordinateWatcher(GeoPositionAccuracy.High);
            cw.MovementThreshold = 20;
            cw.PositionChanged += OnMyPositionChanged;
            me = new Pushpin() { 
                Opacity = .6,
                Content = "You",
                Background = (Brush)Application.Current.Resources["PhoneAccentBrush"],
            };
            cw.Start();
            map.Children.Add(me);
        }

        void OnMyPositionChanged(object sender, GeoPositionChangedEventArgs<GeoCoordinate> e) {
            me.Location = e.Position.Location;
            if (false == firstLocationReceived) {
                map.Center = e.Position.Location;
                map.ZoomLevel = 12.0;
                firstLocationReceived = true;
            }
        }

        void OnCarsLoaded(object sender, EventArgs e) {
            var cultureInfo = new CultureInfo("en-US");
            foreach (var car in ((App)sender).Cars) {
                var coordinate = new GeoCoordinate(
                    Double.Parse(car.lat, cultureInfo.NumberFormat),
                    Double.Parse(car.lng, cultureInfo.NumberFormat));

                var pushpin = new Pushpin() {
                    Location = coordinate,
                    Name = car.hal2option.tooltip,
                    Opacity = .3,
                    Width = 20

                };
                pushpin.Tap += OnPushpinTap;
                map.Children.Add(pushpin);
                //pushpin.Location = 
            }  
        }

        void OnPushpinTap(object sender, GestureEventArgs e) {
            var provider = "Multicity";
            MessageBox.Show(((Pushpin)sender).Name.Replace("'", ""), provider, MessageBoxButton.OK);
        }
        public void LoadFreeCars() {
            
        }
    }
}