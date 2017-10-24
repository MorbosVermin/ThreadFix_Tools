using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Com.WaitWha.ThreadFix.REST.Domain
{
    [Serializable]
    public class Waf
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string WafTypeName { get; set; }
        public List<Application> Applications { get; set; }
    }
}
