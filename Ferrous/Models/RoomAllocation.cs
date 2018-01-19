using System;
using System.Collections.Generic;

namespace Ferrous.Models
{
    public partial class RoomAllocation
    {
        public int Sno { get; set; }
        public int RoomId { get; set; }
        public string ContingentLeaderNo { get; set; }
        public int Partial { get; set; }

        public Contingents ContingentLeaderNoNavigation { get; set; }
        public Room Room { get; set; }
    }
}
