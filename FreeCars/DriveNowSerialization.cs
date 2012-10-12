using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Runtime.Serialization;
using System.Collections.Generic;

namespace FreeCars {

    [DataContract]
    public class DriveNowData {
        [DataMember(Name = "err")]
				public object[] err { get; set; }
        [DataMember(Name = "msg")]
				public object[] msg { get; set; }
        [DataMember(Name = "state")]
				public object state { get; set; }
				[DataMember(Name = "rec")]
        public DriveNowRecord rec { get; set; }
    }
		[DataContract]
		public class DriveNowRecord {
					[DataMember(Name = "vehicles")]
					public DriveNowVehiclesList vehicles { get; set; }
		}
    [DataContract]
    public class DriveNowVehiclesList {
        [DataMember(Name = "search_criteria")]
        public List<DriveNowSearchCriterion> search_criteria { get; set; }
				[DataMember(Name = "vehicles")]
        public List<DriveNowCarInformation> vehicles { get; set; }
    }
		[DataContract]
		public class DriveNowCarInformation : Marker {
				[DataMember(Name = "position")]
				public DriveNowPosition position { get; set; }
				[DataMember(Name = "auto")]
				public String auto { get; set; }
				[DataMember(Name = "fuelType")]
				public String fuelType { get; set; }
				[DataMember(Name = "innerCleanliness")]
				public String innerCleanliness { get; set; }
				[DataMember(Name = "carName")]
				public String carName { get; set; }
				[DataMember(Name = "vin")]
				public String vin { get; set; }				
		}
		[DataContract]
    public class DriveNowSearchCriterion {
				[DataMember(Name = "auto")]
				public String auto { get; set; }
        [DataMember(Name = "group")]
				public String group { get; set; }
        [DataMember(Name = "model")]
				public String model { get; set; }
		}
    [DataContract]
    public class DriveNowPosition {
        [DataMember(Name = "latitude")]
        public String latitude { get; set; }
        [DataMember(Name = "longitude")]
        public String longitude { get; set; }
        [DataMember(Name = "address")]
        public String address { get; set; }
    }
}
