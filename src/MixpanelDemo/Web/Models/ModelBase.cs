using System.Collections.Generic;

namespace Web.Models
{
    public class ModelBase
    {
        public ActionType ActionType { get; set; }
        public string Token { get; set; }
        public string DistinctId { get; set; }
        public IEnumerable<Property> Properties { get; set; }
        public IEnumerable<Property> SuperProperties { get; set; }
        public Config Config { get; set; }
    }
}