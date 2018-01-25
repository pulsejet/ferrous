using System.Collections.Generic;

namespace Ferrous.Models
{
    public partial class Room
    {
        public Room()
        {
            RoomAllocation = new HashSet<RoomAllocation>();
        }

        public int Id { get; set; }
        public string Location { get; set; }
        public string LocationExtra { get; set; }
        public string Room1 { get; set; }
        public string LockNo { get; set; }
        public int Capacity { get; set; }
        public short? Status { get; set; }
        public string Remark { get; set; }

        public Building LocationNavigation { get; set; }
        public ICollection<RoomAllocation> RoomAllocation { get; set; }
    }
}
