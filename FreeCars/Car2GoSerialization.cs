using System;
using System.Collections.Generic;
using System.Net;
using System.Runtime.Serialization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace FreeCars {
	[DataContract]
	public class Car2GoInformation : Marker {
		
	}

	[DataContract]
	public class Car2GoVehicleData {
		[DataMember(Name = "placemarks")]
		public List<Car2GoVehicleInformation> placemarks;
	}
	[DataContract]
	public class Car2GoVehicleInformation {
		[DataMember(Name = "address")] 
		public string address;
		[DataMember(Name = "coordinates")] 
		public string[] coordinates;
		[DataMember(Name = "fuel")]
		public string fuel;
		[DataMember(Name = "name")]
		public string name;
		[DataMember(Name = "vin")]
		public string vin;
	}
}
