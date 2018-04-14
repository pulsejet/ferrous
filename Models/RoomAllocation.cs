using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

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

        [JsonIgnore]
        public ContingentArrival ContingentArrivalNoNavigation { get; set; }
        public Contingent ContingentLeaderNoNavigation { get; set; }
        public Room Room { get; set; }

        public List<Misc.Link> Links;
    }
}
