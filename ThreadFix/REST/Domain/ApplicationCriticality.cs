using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Com.WaitWha.ThreadFix.REST.Domain
{
    /// <summary>
    /// Criticality of an application within ThreadFix.
    /// </summary>
    [Serializable]
    [DataContract]
    public class ApplicationCriticality
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
    }
}
