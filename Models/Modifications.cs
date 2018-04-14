namespace Ferrous.Models
{
    public partial class Building
    {
        public int CapacityEmpty;
        public int CapacityFilled;
        public int CapacityNotReady;
        public int AlreadyAllocated;
    }

    public partial class Contingent
    {
        public string Male;
        public string Female;
        public string ArrivedM;
        public string ArrivedF;
    }
}