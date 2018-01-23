using System;
using System.Collections.Generic;

namespace Ferrous.Models
{
    public partial class Contingents
    {
        public Contingents()
        {
            ContingentArrival = new HashSet<ContingentArrival>();
            Person = new HashSet<Person>();
            RoomAllocation = new HashSet<RoomAllocation>();
        }

        public string ContingentLeaderNo { get; set; }
        public int? Male { get; set; }
        public int? Female { get; set; }
        public int? ArrivedM { get; set; }
        public int? ArrivedF { get; set; }
        public string AllocatedRooms { get; set; }

        public ICollection<ContingentArrival> ContingentArrival { get; set; }
        public ICollection<Person> Person { get; set; }
        public ICollection<RoomAllocation> RoomAllocation { get; set; }
    }
}
