using System;
using System.Collections.Generic;
using System.Device.Location;
using System.Linq;
using System.Text;

namespace FreeCars.Providers {
	interface IProvider<T> {
		GeoPosition<GeoCoordinate> Position { get; set; }
		List<T> Markers { get; set; }
		void LoadPOIs();
	}
}
