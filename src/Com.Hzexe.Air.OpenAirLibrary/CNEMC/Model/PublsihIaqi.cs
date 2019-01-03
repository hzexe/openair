namespace CNEMC.Model
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.Serialization;

    public class PublsihIaqi
    {
        [DataMember]
        public string AQI { get; set; }

        [DataMember]
        public string ICO { get; set; }

        [DataMember]
        public string INO2 { get; set; }

        [DataMember]
        public string IO3 { get; set; }

        [DataMember]
        public string IO3_8h { get; set; }

        [DataMember]
        public string IPM10 { get; set; }

        [DataMember]
        public string IPM2_5 { get; set; }

        [DataMember]
        public string ISO2 { get; set; }

        [DataMember]
        public string PositionName { get; set; }

        [DataMember]
        public string StationCode { get; set; }
    }
}

