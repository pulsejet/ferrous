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

        [StringLength(1)]
        public string Sex { get; set; }

        public Contingents ContingentLeaderNoNavigation { get; set; }

        public List<Link> links;
    }
}
