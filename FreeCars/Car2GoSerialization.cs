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
		[DataMember(Name = "engineType")]
		public string engineType;
	}

	[DataContract]
	public struct Car2GoLocations {
		[DataMember]
		public Car2GoLocation[] location { get; set; }
	} 
	
	[DataContract]
	public class Car2GoLocation {
		[DataMember]
		public string countryCode { get; set; }
		[DataMember]
		public int locationId { get; set; }
		[DataMember]
		public string locationName { get; set; }
		[DataMember]
		public string defaultLanguage { get; set; }
		public override string ToString() {
			return locationName;
		}
	}

#region car2go API
	// {"returnValue":{"code":0,"description":"Operation successful."},"account":[{"accountId":994,"description":"Max Mustermann11585"}]}
	[DataContract]
	public class ResultAccounts {
		[DataMember(Name = "returnValue")]
		public APIReturnValue ReturnValue;
		[DataMember(Name = "account")]
		public APIReturnValue[] Account;
	}

	[DataContract]
	public class APIAccount {
		[DataMember(Name = "accountId")]
		public int AccountId;
		[DataMember(Name = "description")]
		public string Description;
	}
	[DataContract]
	public class APIReturnValue {
		[DataMember(Name = "code")]
		public int Code;
		[DataMember(Name = "description")]
		public string Description;
	}
#endregion
}
