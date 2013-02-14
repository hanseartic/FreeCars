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
		public bool isBooked;
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
		[DataMember]
		public string interior;
		[DataMember]
		public string exterior;
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
	// account
	// {"returnValue":{"code":0,"description":"Operation successful."},"account":[{"accountId":994,"description":"Max Mustermann11585"}]}
	[DataContract]
	public class ResultAccounts {
		[DataMember(Name = "returnValue")]
		public APIReturnValue ReturnValue;
		[DataMember(Name = "account")]
		public APIAccount[] Account;
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
	[DataContract]
	public class Car2GoAPIPosition {
		[DataMember(Name = "address")]
		public String Address;
		[DataMember(Name = "latitude")]
		public double Latitude;
		[DataMember(Name = "longitude")]
		public double Longitude;
	}
	[DataContract]
	public class Car2GoAPIReservationTime {
		[DataMember(Name = "timeInMillis")]
		public long TimeInMillis;
	}
	//"vehicle":{"engineType":"CE","exterior":"GOOD","fuel":100,"interior":"GOOD","numberPlate":"B-GO0815","position":{"address":"Pariser Platz 1, 10117 Berlin","latitude":52.516391,"longitude":13.379588},"vin":"WME4513341K571875"}
	[DataContract]
	public class Car2GoAPIVehicleData {
		[DataMember(Name = "engineType")]
		public String EngineType;
		[DataMember(Name = "exterior")]
		public String Exterior;
		[DataMember(Name = "interior")]
		public String Interior;
		[DataMember(Name = "numberPlate")]
		public String NumberPlate;
		[DataMember(Name = "position")]
		public Car2GoAPIPosition Position;
		[DataMember(Name = "vin")]
		public String VIN;
		[DataMember(Name = "fuel")] 
		public String Fuel;
	}
	//booking
	// {"booking":[{"account":{"accountId":1001,"description":"Test User"},"bookingId":123456,"bookingposition":{"address":"Pariser Platz 1, 10117 Berlin","latitude":52.516391,"longitude":13.379588},"reservationTime":{"firstDayOfWeek":1,"gregorianChange":"1582-10-15","lenient":true,"minimalDaysInFirstWeek":1,"time":"2013-01-01","timeInMillis":1356994800,"timeZone":{"DSTSavings":3600000,"ID":"Europe/Berlin","dirty":false,"displayName":"Central European Time","lastRuleInstance":{"DSTSavings":3600000,"ID":"Europe/Berlin","displayName":"Central European Time","rawOffset":3600000},"rawOffset":3600000}},"vehicle":{"engineType":"CE","exterior":"GOOD","fuel":100,"interior":"GOOD","numberPlate":"B-GO0815","position":{"address":"Pariser Platz 1, 10117 Berlin","latitude":52.516391,"longitude":13.379588},"vin":"WME4513341K571875"}}],"returnValue":{"code":0,"description":"Operation successful."}}"
	[DataContract]
	public class Car2GoBookingResult {
		[DataMember(Name = "booking")]
		public Car2GoBooking[] Booking;
		[DataMember(Name = "returnValue")]
		public APIReturnValue ReturnValue;
	}
	[DataContract]
	public class Car2GoBooking {
		[DataMember(Name = "account")]
		public APIAccount Account;
		[DataMember(Name = "bookingId")]
		public int BookingId;
		[DataMember(Name = "bookingposition")]
		public Car2GoAPIPosition BookingPosition;
		[DataMember(Name = "reservationTime")]
		public Car2GoAPIReservationTime ReservationTime;
		[DataMember(Name = "vehicle")]
		public Car2GoAPIVehicleData Vehicle;
	}
#endregion
}
