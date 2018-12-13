namespace Ferrous.Models
{
    public partial class Building
    {
        public int CapacityEmpty;
        public int CapacityFilled;
        public int CapacityNotReady;
        public int CapacityMaintainance;
        public int CapacityReserved;
        public int AlreadyAllocated;
        public int RoomsEmpty = 0;
        public int RoomsFilled = 0;
        public int RoomsNotReady = 0;
        public int RoomsMaintainance;
        public int RoomsReserved;
        public int RoomsPartial = 0;
        public int RoomsTotal = 0;
    }

    public partial class Contingent
    {
        public string Male;
        public string Female;
        public string ArrivedM;
        public string ArrivedF;
    }
}