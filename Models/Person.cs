using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Ferrous.Models
{
    public partial class Person
    {
        [Key]
        public string Mino { get; set; }
        public string College { get; set; }
        public string ContingentLeaderNo { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }

        [EmailAddress]
        public string Email { get; set; }

        [StringLength(1)]
        public string Sex { get; set; }

        [Newtonsoft.Json.JsonIgnore]
        public Contingent ContingentLeaderNoNavigation { get; set; }

        public ContingentArrival allottedCA { get; set; }

        public List<Misc.Link> links;
    }
}
