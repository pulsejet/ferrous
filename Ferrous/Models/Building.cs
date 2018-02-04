using System;
using System.Collections.Generic;

namespace Ferrous.Models
{
    public partial class Building
    {
        public Building()
        {
            Room = new HashSet<Room>();
        }

        public string Location { get; set; }
        public long DefaultCapacity { get; set; }
        public string LocationFullName { get; set; }

        public ICollection<Room> Room { get; set; }
    }
}
