using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ferrous.Models
{
    public partial class Room
    {
        public Room()
        {
            RoomAllocation = new HashSet<RoomAllocation>();
        }
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int RoomId { get; set; }

        public int Capacity { get; set; }
        public string Location { get; set; }
        public string LocationExtra { get; set; }
        public string LockNo { get; set; }
        public string Remark { get; set; }
        public string RoomName { get; set; }
        public int? Status { get; set; }

        public Building LocationNavigation { get; set; }
        public ICollection<RoomAllocation> RoomAllocation { get; set; }
    }
}
