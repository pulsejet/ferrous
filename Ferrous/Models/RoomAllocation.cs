using System;
using System.Collections.Generic;

namespace Ferrous.Models
{
    public partial class RoomAllocation
    {
        public long Sno { get; set; }
        public long? ContingentArrivalNo { get; set; }
        public string ContingentLeaderNo { get; set; }
        public long Partial { get; set; }
        public long RoomId { get; set; }

        public ContingentArrival ContingentArrivalNoNavigation { get; set; }
        public Contingents ContingentLeaderNoNavigation { get; set; }
        public Room Room { get; set; }
    }
}
