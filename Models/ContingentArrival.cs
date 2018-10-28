using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace Ferrous.Models
{
    public partial class ContingentArrival
    {
        public ContingentArrival()
        {
            RoomAllocation = new HashSet<RoomAllocation>();
            CAPeople = new HashSet<CAPerson>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ContingentArrivalNo { get; set; }

        public string ContingentLeaderNo { get; set; }
        public DateTime CreatedOn { get; set; }

        public int? MaleOnSpot { get; set; }
        public int? FemaleOnSpot { get; set; }

        public int? MaleOnSpotDemand { get; set; }
        public int? FemaleOnSpotDemand { get; set; }

        public Contingent ContingentLeaderNoNavigation { get; set; }
        public ICollection<RoomAllocation> RoomAllocation { get; set; }

        public int? Female { get; set; }
        public int? Male { get; set; }
        public int PeopleFemale = 0;
        public int PeopleMale = 0;
        public int AllottedMale = 0;
        public int AllottedFemale = 0;

        public ICollection<CAPerson> CAPeople { get; set; }
        public bool Approved { get; set; }
        public string Remark { get; set; }

        public List<Misc.Link> Links;
    }

    public class CAPerson
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Sno { get; set; }

        [JsonIgnore]
        public ContingentArrival CANav { get; set; }
        public string Mino { get; set; }

        [StringLength(1)]
        public string Sex { get; set; }

        public Person person;
        public string flags = String.Empty;

        public List<Misc.Link> Links;
    }
}
