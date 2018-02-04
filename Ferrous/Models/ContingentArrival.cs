using System;
using System.Collections.Generic;

namespace Ferrous.Models
{
    public partial class ContingentArrival
    {
        public ContingentArrival()
        {
            RoomAllocation = new HashSet<RoomAllocation>();
        }

        public long ContingentArrivalNo { get; set; }
        public string ContingentLeaderNo { get; set; }
        public DateTime CreatedOn { get; set; }
        public long? Female { get; set; }
        public long? FemaleOnSpot { get; set; }
        public long? Male { get; set; }
        public long? MaleOnSpot { get; set; }

        public Contingents ContingentLeaderNoNavigation { get; set; }
        public ICollection<RoomAllocation> RoomAllocation { get; set; }
    }
}
