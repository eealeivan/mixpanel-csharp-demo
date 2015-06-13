using System;

namespace Web.Models
{
    public class PeopleTrackCharge : ModelBase
    {
        public decimal Amount { get; set; }
        public DateTime? Time { get; set; }
    }
}