namespace Ferrous.Models
{
    public partial class Building
    {
        public long CapacityEmpty;
        public long CapacityFilled;
        public long CapacityNotReady;
        public long AlreadyAllocated;
    }

    public partial class Contingents
    {
        public string Male;
        public string Female;
        public string ArrivedM;
        public string ArrivedF;
    }
}