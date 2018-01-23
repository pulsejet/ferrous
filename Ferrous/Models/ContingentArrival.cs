﻿using System;
using System.Collections.Generic;

namespace Ferrous.Models
{
    public partial class ContingentArrival
    {
        public ContingentArrival()
        {
            RoomAllocation = new HashSet<RoomAllocation>();
        }

        public int ContingentArrivalNo { get; set; }
        public string ContingentLeaderNo { get; set; }
        public DateTime? CreatedOn { get; set; }
        public int? Male { get; set; }
        public int? Female { get; set; }
        public int? MaleOnSpot { get; set; }
        public int? FemaleOnSpot { get; set; }

        public Contingents ContingentLeaderNoNavigation { get; set; }
        public ICollection<RoomAllocation> RoomAllocation { get; set; }
    }
}