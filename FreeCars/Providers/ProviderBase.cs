using System;
using System.Collections.Generic;
using System.Device.Location;
using System.Linq;
using System.Text;
using System.Windows;

namespace FreeCars.Providers {
	public abstract class ProviderBase<T> : DependencyObject {
		public virtual GeoPosition<GeoCoordinate> Position { get; set; }
		public abstract List<T> Markers { get; protected set; }
		public abstract void LoadPOIs();
	}
}
