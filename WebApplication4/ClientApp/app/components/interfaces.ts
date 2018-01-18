export interface Contingent {
    contingentLeaderNo: string;
    male: number;
    female: number;
    arrivedM: number;
    arrivedF: number;
    allocatedRooms: string;
    person: Person[];
    roomAllocation: RoomAllocation[];
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
}