using Microsoft.EntityFrameworkCore;

namespace Ferrous.Models
{
    public partial class ferrousContext : DbContext
    {
        public virtual DbSet<Building> Building { get; set; }
        public virtual DbSet<ContingentArrival> ContingentArrival { get; set; }
        public virtual DbSet<Contingent> Contingents { get; set; }
        public virtual DbSet<Person> Person { get; set; }
        public virtual DbSet<Room> Room { get; set; }
        public virtual DbSet<RoomAllocation> RoomAllocation { get; set; }
        public virtual DbSet<CAPerson> CAPerson { get; set; }

        public ferrousContext(DbContextOptions<ferrousContext> options)
            : base(options)
        { }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        { }
    }
}
