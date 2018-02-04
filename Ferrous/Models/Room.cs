using System;
using System.Collections.Generic;

namespace Ferrous.Models
{
    public partial class Room
    {
        public Room()
        {
            RoomAllocation = new HashSet<RoomAllocation>();
        }

        public long Id { get; set; }
        public long Capacity { get; set; }
        public string Location { get; set; }
        public string LocationExtra { get; set; }
        public string LockNo { get; set; }
        public string Remark { get; set; }
        public string Room1 { get; set; }
        public long? Status { get; set; }

        public Building LocationNavigation { get; set; }
        public ICollection<RoomAllocation> RoomAllocation { get; set; }
    }
}
