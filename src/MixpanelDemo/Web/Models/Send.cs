using System.Collections.Generic;
using Mixpanel;

namespace Web.Models
{
    public class Send : ModelBase
    {
        public IList<MixpanelMessage> Messages { get; set; }
    }
}