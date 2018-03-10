using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

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

        [Key]
        public string ContingentLeaderNo { get; set; }
        public string Remark { get; set; }

        public ICollection<ContingentArrival> ContingentArrival { get; set; }
        public ICollection<Person> Person { get; set; }
        public ICollection<RoomAllocation> RoomAllocation { get; set; }

        public List<Misc.Link> Links;
    }
}
