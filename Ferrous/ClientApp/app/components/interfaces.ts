export interface EnumContainer {
    links: Link[];
    data: any;
}

export interface Link {
    rel: string;
    href: string;
    method: string;
}

export interface Contingent {
    contingentLeaderNo: string;
    male: string;
    female: string;
    arrivedM: string;
    arrivedF: string;
    person: Person[];
    roomAllocation: RoomAllocation[];
    contingentArrival: ContingentArrival[];

    links: Link[];
}

export interface Person {
    name: string;
    mino: string;
    college: string;
    contingentLeaderNo: string;
    contingentLeaderNoNavigation: string;
    sex: string;

    links: Link[];
}

export interface Room {
    roomId: number;
    location: string;
    locationExtra: string;
    roomName: string;
    lockNo: string;
    capacity: number;
    allocated: string;
    status: number;
    remark: string;
    roomAllocation: RoomAllocation[];

    selected: boolean;
    partialsel: number;
    partialallot: boolean;

    links: Link[];
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

    links: Link[];
}

export interface Building {
    location: string;
    locationFullName: string;
    defaultCapacity: number;
    room: Room[];
    capacityEmpty: number;
    capacityFilled: number;
    capacityNotReady: number;

    links: Link[];
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

    links: Link[];
}