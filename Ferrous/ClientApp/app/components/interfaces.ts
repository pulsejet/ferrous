export interface Contingent {
    contingentLeaderNo: string;
    male: number;
    female: number;
    arrivedM: number;
    arrivedF: number;
    allocatedRooms: string;
    person: Person[];
    roomAllocation: RoomAllocation[];
    contingentArrival: ContingentArrival[];
}

export interface Person {
    name: string;
    mino: string;
    college: string;
    contingentLeaderNo: string;
    contingentLeaderNoNavigation: string;
    sex: string;
}

export interface Room {
    id: number;
    location: string;
    locationExtra: string;
    room1: string;
    lockNo: string;
    capacity: number;
    allocated: string;
    status: number;
    allocatedTo: string;
    remark1: string;
    remark2: string;
    reasonUnavailable: string;
    roomAllocation: RoomAllocation[];

    selected: boolean;
    partialsel: number;
    partialallot: boolean;
}

export interface RoomAllocation {
    sno: number;
    roomId: number;
    contingentLeaderNo: string;
    contingentLeaderNoNavigation: Contingent;
    room: Room;
    partial: number;
    contingentArrivalNo: string;
    contingentArrivalNoNavigation: ContingentArrival[];
}

export interface Building {
    location: string;
    locationFullName: string;
    defaultCapacity: number;
    room: Room[];
    capacityEmpty: number;
    capacityFilled: number;
    capacityNotReady: number;
}

export interface ContingentArrival {
    contingentArrivalNo: number;
    contingentLeaderNo: string;
    createdOn: Date;
    male: number;
    female: number;
    maleOnSpot: number;
    femaleOnSpot: number;
    contingentLeaderNoNavigation: Contingent[];
    roomAllocation: RoomAllocation[];
}