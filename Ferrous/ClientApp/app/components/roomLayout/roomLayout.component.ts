import { Component, Inject, ElementRef, ViewChild } from '@angular/core';
import { ActivatedRoute, Params } from '@angular/router';
import { Room, RoomAllocation } from '../interfaces';
import { Title } from '@angular/platform-browser';
import { DataService } from '../../DataService';
import * as $ from 'jquery';

/* Room layout component */
@Component({
    selector: 'roomLayout',
    templateUrl: './roomLayout.component.html',
    styleUrls: ['../../Custom.css'],
})
export class RoomLayoutComponent {
    public rooms: Room[];                   /* master list of rooms     */
    public clno: string;                    /* current CLNo             */
    public loc_fullname: string;            /* location full name       */
    public loc_code: string;                /* code of current location */
    @ViewChild('roomsLayout')               /* layout element           */
        roomsLayout: ElementRef;            /*                          */
    public marking: boolean = false;        /* marking status change    */

    constructor(private activatedRoute: ActivatedRoute,
        private titleService: Title,
        private dataService: DataService,
        @Inject('BASE_URL') public baseUrl: string) {

        this.titleService.setTitle("Room Layout");

        /* Get URL parameters */
        this.activatedRoute.params.subscribe((params: Params) => {
            this.loc_code = params['location'];
            this.clno = params['id'];
        });

        /* Get room layout by location */
        dataService.GetRoomLayout(this.loc_code).subscribe(result => {
            this.roomsLayout.nativeElement.innerHTML = result;

            /* Load rooms */
            this.reloadRooms();
        });
    }

    reloadRooms() {
        this.dataService.GetBuilding(this.loc_code).subscribe(result => {
            this.rooms = result.room;
            this.loc_fullname = result.locationFullName;
            this.AssignRoomsInit();
        });
    }

    /* Initialize rooms */
    public AssignRoomsInit() {
        var self = this;
        for (let room of this.rooms) {
            /* Find the room by HTML id */
            let ctrl = this.getRoomId(room)

            /* Mark the room selected */
            $(ctrl).click(function () {
                if ((room.status == 1 && room.roomAllocation.length == 0)
                    || self.CheckPartial(room)
                    || room.selected
                    || self.marking) { 

                    room.selected = !room.selected;
                }

                /* Update room graphic */
                self.AssignRoom(room);
            });

            /* Initial room graphic update */
            this.AssignRoom(room);
        }
    }

    /* Update graphic for all rooms */
    public AssignRooms() {
        for (let room of this.rooms) {
            this.AssignRoom(room);
        }
    }

    /* Update graphic for one room */
    public AssignRoom(room: Room) {
        let ctrl = this.getRoomId(room);
        let clss: string = this.GetRoomClass(room);

        /* Show capacity and room number */
        $(ctrl).html(this.GetCapacity(room) + "<br><span>" + room.room1.toString() + "</span>");

        /* Assign CSS class */
        $(ctrl).attr("class", clss);
    }

    /* Mark all selected rooms */
    public Mark(Status: number) {
        for (let room of this.rooms) {
            if (room.selected) {
                this.dataService.MarkRoom(room.id,Status).subscribe(result => {
                    room.status = Status;
                    room.selected = false;
                    this.AssignRoom(room);
                });
            }
        }
    }

    /* Get number of free spaces in room */
    public GetCapacity(room: Room): number {
        if (this.CheckOccupied(room)) {
            return 0;
        } else if (this.CheckPartial(room)) {
            return (room.capacity - this.GetPartialNo(room));
        } else {
            return room.capacity;
        }
    }

    /* Allot all selected rooms */
    public Allot() {
        for (let room of this.rooms) {
            if (room.selected) {
                this.dataService.AllotRoom(room, this.clno).subscribe(result => {
                    /* Add new allocation */
                    room.roomAllocation.push(result);

                    /* Unmark the room and update graphic */
                    room.selected = false;
                    this.AssignRoom(room);
                });
            }
        }
    }

    /* Check if room is full */
    public CheckOccupied(room: Room): boolean {
        return this.dataService.RoomCheckOccupied(room);
    }

    /* Check if partially filled */
    public CheckPartial(room: Room): boolean {
        return this.dataService.RoomCheckPartial(room);
    }

    /* Get partial number of people in room */
    public GetPartialNo(room: Room): number {
        return this.dataService.RoomGetPartialNo(room);
    }

    /* Get CSS class for room */
    public GetRoomClass(room: Room): string {
        /* Selected has top priority */
        if (room.selected) return "room sel";

        let containsThis: boolean = false;      /* Room contains current contingent     */
        let containsOther: boolean = false;     /* Room contains another contingent     */
        let filled: number = 0;                 /* Number of people currently in room   */

        /* Fill up local data */
        for (let roomA of room.roomAllocation) {
            if (roomA.contingentLeaderNo == this.clno)
                containsThis = true;
            else
                containsOther = true;

            if (roomA.partial != null && roomA.partial <= 0)
                filled += Number(room.capacity);
            else
                filled += Number(roomA.partial);
        }

        /* Assign classes */
        if (filled < room.capacity) {
            if (containsOther && !containsThis) {
                return "room partial"
            } else if (containsThis) {
                return "room already-partial"
            }
        } else {
            if (containsOther && !containsThis) {
                return "room occupied"
            } else if (!containsOther && containsThis) {
                return "room already"
            } else if (containsOther && containsThis) {
                return "room already-fullshared"
            }
        }

        /* Fall back to status code */
        let status = room.status;
        if (status == 0) return "room unavailable";
        else if (status == 1) return "room empty";
        else if (status == 2) return "room occupied";
        else if (status == 3) return "room partial";
        else if (status == 4) return "room notready";
        return "room";
    }

    /* Remove a RoomAllocation */
    public unallocateRoom(roomA: RoomAllocation, room: Room) {
        this.dataService.UnllocateRoom(roomA.sno).subscribe(result => {
            var index = room.roomAllocation.indexOf(roomA, 0);
            room.roomAllocation.splice(index, 1)
            this.AssignRoom(room);
        });
    }

    /* Check if room can be allocated */
    public canAllocate(room: Room): boolean {
        return ((room.status == 1) || (room.status == 3)) && (!this.CheckOccupied(room));
    }

    /* Get HTML Id of room */
    public getRoomId(room: Room): string {
        return ("#room-" + room.location + "-" + room.room1).replace(/\s+/, "");
    }

}
