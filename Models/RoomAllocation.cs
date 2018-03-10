using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ferrous.Models
{
    public partial class RoomAllocation
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Sno { get; set; }

        public int? ContingentArrivalNo { get; set; }
        public string ContingentLeaderNo { get; set; }
        public int Partial { get; set; }
        public int RoomId { get; set; }

        public ContingentArrival ContingentArrivalNoNavigation { get; set; }
        public Contingents ContingentLeaderNoNavigation { get; set; }
        public Room Room { get; set; }

        public List<Misc.Link> Links;
    }
}
