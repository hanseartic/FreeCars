using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace FreeCars {
	public partial class BookingControl : UserControl {
		public BookingControl() {
			InitializeComponent();
		}
		
		public void Activate(Marker item) {
			VisualStateManager.GoToState(this, "ActiveState", true);
		}

		private void OnCancelButtonClicked(object sender, System.Windows.RoutedEventArgs e) {
			VisualStateManager.GoToState(this, "InactiveState", true);
			TriggerEvent(Closed);
		}

		private void OnOKButtonClicked(object sender, System.Windows.RoutedEventArgs e) {
			//VisualStateManager.GoToState(this, "InactiveState", true);
		}

		private void OnLoaded(object sender, System.Windows.RoutedEventArgs e) {
			VisualStateManager.GoToState(this, "InactiveState", false);
		}

		private void TriggerEvent(EventHandler eventName) {
			try {
				if (null != eventName) {
					eventName(this, null);
				}
			} catch { }
		}

		public event EventHandler Closed;
	}
}
