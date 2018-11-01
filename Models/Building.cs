using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Ferrous.Models
{
    public partial class Building
    {
        public Building()
        {
            Room = new HashSet<Room>();
        }

        [Key]
        public string Location { get; set; }

        [StringLength(1)]
        public string Sex { get; set; }
        public int DefaultCapacity { get; set; }
        public string LocationFullName { get; set; }

        public ICollection<Room> Room { get; set; }

        public List<Misc.Link> Links;

        public bool hasAuth(System.Security.Claims.ClaimsPrincipal user) {
            return Ferrous.Misc.Authorization.hasBuildingAuth(user.Identity.Name, Location);
        }
    }
}
