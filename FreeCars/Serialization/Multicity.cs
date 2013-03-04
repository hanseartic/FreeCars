using System;
using System.Runtime.Serialization;
using System.Collections.Generic;

namespace FreeCars.Serialization {
	[DataContract]
	public class MulticityMarker : Marker {
		[DataMember(Name = "lat")]
		public new String lat { get; set; }
		[DataMember(Name = "lng")]
		public new String lng { get; set; }
		[DataMember(Name = "iconName")]
		public String iconName { get; set; }
		[DataMember(Name = "hal2option")]
		public hal2option hal2option { get; set; }

	}

	[DataContract]
	public class MulticityChargerMarker {
		[DataMember(Name = "lat")]
		public String lat { get; set; }
		[DataMember(Name = "lng")]
		public String lng { get; set; }
		[DataMember(Name = "hal2option")]
		public hal2optionCharger hal2option { get; set; }
	}

	[DataContract]
	public class hal2optionCharger {
		[DataMember(Name = "id")]
		public String id { get; set; }
		[DataMember(Name = "tooltip")]
		public String tooltip { get; set; }
		[DataMember(Name = "objectname")]
		public String objectname { get; set; }
		[DataMember(Name = "markerInfo")]
		public ChargerMarkerInfo markerInfo { get; set; }
	}
	[DataContract]
	public class ChargerMarkerInfo {
		[DataMember(Name = "name")]
		public String name { get; set; }
		[DataMember(Name = "capacity")]
		public String capacity { get; set; }
		[DataMember(Name = "free")]
		public String free { get; set; }
	}
	/*
  
"hal2option": {
				"StationInfoTyp": "HM_LADESAEULEN_INFO",
				"click": "showMarkerInfos",
				"clustername": "rwecluster_vacant",
				"dblclick": false,
				"draggable": false,
				"id": "RS00178",
				"maxZoom": false,
				"minZoom": "11",
				"mouseout": false,
				"mouseover": false,
				"objectname": "rwemarker_vacant",
				"objecttyp": "rweLadestation",
				"openinfo": "openDynamicInfoWindow",
				"rclick": false,
				"tooltip": "Ladesäule RS00178",
				"markerInfo": {
					"name": "Ladesäule RS00178",
					"capacity": "2",
					"free": "2",
					"address": {
						"postalCode": "10117",
						"houseNumber": "33",
						"streetName": "Franz�sische Stra�e",
						"city": "Berlin",
						"freeText": ""
					}
				}
			},
			"iconName": "rweIcon_vacant",
			"iconNameSelected": "rweIcon_selected",
			"lat": 52.515556,
			"lng": 13.395833

	 */

	[DataContract]
	public class MulticityData {
		[DataMember(Name = "statusCode")]
		public String statusCode { get; set; }
		[DataMember(Name = "statusText")]
		public String statusText { get; set; }
		[DataMember(Name = "marker")]
		public List<MulticityMarker> marker { get; set; }
	}
	[DataContract]
	public class MulticityChargerData {
		[DataMember(Name = "marker")]
		public List<MulticityChargerMarker> marker { get; set; }
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
		[DataMember(Name = "id")]
		public String id { get; set; }
	}

	public class FlinksterStationMarker {
	}
}
