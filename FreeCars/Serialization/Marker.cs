using System;
using System.Runtime.Serialization;
using System.Device.Location;

namespace FreeCars.Serialization {
	[DataContract]
	public class Marker {
		internal String lat { get; set; }
		internal String lng { get; set; }
		internal GeoCoordinate position { get; set; }
		private String _fuelState;
		[DataMember(Name = "fuelState")]
		public String fuelState { get { return _fuelState; } set { _fuelState = value; TriggerUpdated(); } }
		[DataMember(Name = "model")]
		public String model { get; set; }
		[DataMember(Name = "licensePlate")]
		public String licensePlate { get; set; }
		public event EventHandler Updated;
		private void TriggerUpdated() {
			if (null != Updated) {
				Updated(this, null);
			}
		}
		public String ID { get; set; }
		public String exterior { get; set; }
		public String interior { get; set; }
	}
}
