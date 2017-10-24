using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Com.WaitWha.ThreadFix.REST.Domain
{
    /// <summary>
    /// An application within Threadfix. 
    /// </summary>
    [Serializable]
    [DataContract]
    public class Application
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

        [DataMember(Name = "url")]
        public string Url { get; set; }

        [DataMember(Name = "repositoryFolder")]
        public string RepositoryFolder { get; set; }

        [DataMember(Name = "repositoryUrl")]
        public string RepositoryUrl { get; set; }

        [DataMember(Name = "repositoryBranch")]
        public string RepositoryBranch { get; set; }

        [DataMember(Name = "repositoryUserName")]
        public string RepositoryUserName { get; set; }

        [DataMember(Name = "repositoryPassword")]
        public string RepositoryPassword { get; set; }

        [DataMember(Name = "projectName")]
        public string ProjectName { get; set; }

        [DataMember(Name = "projectId")]
        public string ProjectId { get; set; }

        [DataMember(Name = "component")]
        public string Component { get; set; }

        [DataMember(Name = "defectTracker")]
        public string DefectTracker { get; set; }

        [DataMember(Name = "obscuredUserName")]
        public string ObscuredUserName { get; set; }

        [DataMember(Name = "obscuredPassword")]
        public string ObscuredPassword { get; set; }

        [DataMember(Name = "skipApplicationMerge")]
        public bool SkipApplicationMerge { get; set; }

        [DataMember(Name = "uniqueId")]
        public string UniqueId { get; set; }

        [DataMember(Name = "applicationCriticality")]
        public ApplicationCriticality ApplicationCriticality { get; set; }

        [DataMember(Name = "grcApplication")]
        public string GrcApplication { get; set; }
        
        [DataMember(Name = "lowVulnCount")]
        public int LowVulnCount { get; set; }

        [DataMember(Name = "infoVulnCount")]
        public int InfoVulnCount { get; set; }

        [DataMember(Name = "mediumVulnCount")]
        public int MediumVulnCount { get; set; }
        
        [DataMember(Name = "highVulnCount")]
        public int HighVulnCount { get; set; }

        [DataMember(Name = "crticialVulnCount")]
        public int CriticalVulnCount { get; set; }

        [DataMember(Name = "totalVulnCount")]
        public int TotalVulnCount { get; set; }

        [DataMember(Name = "waf")]
        public string WAF { get; set; }

        [DataMember(Name = "organization")]
        public Organization Organization { get; set; }

        [DataMember(Name = "active")]
        public bool Active { get; set; }

        [DataMember(Name = "frameworkType")]
        public string FrameworkType { get; set; }

    }
}
