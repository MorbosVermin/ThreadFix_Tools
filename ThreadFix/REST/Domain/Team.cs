using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Com.WaitWha.ThreadFix.REST.Domain
{
    /// <summary>
    /// A team within ThreadFix.
    /// </summary>
    [Serializable]
    [DataContract]
    public class Team
    {
        /// <summary>
        /// Id
        /// </summary>
        [DataMember(Name = "id")]
        public int Id { get; set; }

        /// <summary>
        /// Name
        /// </summary>
        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "infoVulnCount")]
        public int InfoVulnCount { get; set; }

        [DataMember(Name = "lowVulnCount")]
        public int LowVulnCount { get; set; }

        [DataMember(Name = "mediumVulnCount")]
        public int MediumVulnCount { get; set; }

        [DataMember(Name = "highVulnCount")]
        public int HighVulnCount { get; set; }

        [DataMember(Name = "criticalVulnCount")]
        public int CriticalVulnCount { get; set; }

        [DataMember(Name = "totalVulnCount")]
        public int TotalVulnCount { get; set; }

        [DataMember(Name = "applications")]
        public List<Application> Applications { get; set; }

        [DataMember(Name = "active")]
        public bool Active { get; set; }

    }
}
