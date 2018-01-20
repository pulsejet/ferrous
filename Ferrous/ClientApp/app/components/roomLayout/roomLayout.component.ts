import { Component, Inject, ElementRef, ViewChild } from '@angular/core';
import { ActivatedRoute, Params } from '@angular/router';
import { Room, RoomAllocation } from '../interfaces';
import { Title } from '@angular/platform-browser';
import { DataService } from '../../DataService';
import * as $ from 'jquery';

@Component({
    selector: 'home',
    templateUrl: './roomLayout.component.html',
    styleUrls: ['../../Custom.css'],
})
export class RoomLayoutComponent {
    public rooms: Room[];
    public clno: string;
    public Location: string;
    @ViewChild('roomsLayout') roomsLayout: ElementRef;
    public marking: boolean = false;

    constructor(private activatedRoute: ActivatedRoute,
        private titleService: Title,
        private dataService: DataService,
        @Inject('BASE_URL') public baseUrl: string) {

        this.titleService.setTitle("Room Layout");

        this.activatedRoute.params.subscribe((params: Params) => {
            this.clno = params['id'];
        });

        dataService.GetRoomLayout('H7').subscribe(result => {
            this.roomsLayout.nativeElement.innerHTML = result;
            this.Location = $("#LocationName").html();
        });

        this.reloadRooms();
    }

    public AssignRoomsInit() {
        var self = this;
        for (let room of this.rooms) {
            let str = "#room-" + room.location + "-" + room.room1;
            let ctrl = str.replace(/\s+/, "");

            $(ctrl).click(function () {
                if ((room.status == 1 && room.roomAllocation.length == 0)
                    || self.CheckPartial(room)
                    || room.selected
                    || self.marking) { 

                    room.selected = !room.selected;
                }
                self.AssignRooms();
            });
        }

        this.AssignRooms();
    }

    public AssignRooms() {
        for (let room of this.rooms) {
            this.AssignRoom(room);
        }
    }

    public AssignRoom(room: Room){
        let str = "#room-" + room.location + "-" + room.room1;
        let ctrl = str.replace(/\s+/, "");

        let clss: string = this.GetRoomClass(room);

        $(ctrl).html(this.GetCapacity(room) + "<br><span>" + room.room1.toString() + "</span>");
        $(ctrl).attr("class", clss);
    }

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

    public GetCapacity(room: Room): number {
        if (this.CheckOccupied(room)) {
            return 0;
        } else if (this.CheckPartial(room)) {
            return (room.capacity - this.GetPartialNo(room));
        } else {
            return room.capacity;
        }
    }

    public Allot() {
        for (let room of this.rooms) {
            if (room.selected) {
                this.dataService.AllotRoom(room, this.clno).subscribe(result => {
                    /* Create new allocator */
                    let roomA: RoomAllocation = {} as RoomAllocation;
                    roomA.sno = result;
                    roomA.contingentLeaderNo = this.clno;
                    roomA.roomId = room.id;

                    if (room.partialallot || this.CheckPartial(room)) {
                        roomA.partial = room.partialsel;
                    }
                    else
                        roomA.partial = -1;

                    room.roomAllocation.push(roomA);

                    room.selected = false;
                    this.AssignRoom(room);
                });
            }
        }
    }

    public CheckOccupied(room: Room): boolean {
        return this.dataService.RoomCheckOccupied(room);
    }

    public CheckPartial(room: Room): boolean {
        return this.dataService.RoomCheckPartial(room);
    }

    public GetPartialNo(room: Room): number {
        return this.dataService.RoomGetPartialNo(room);
    }

    public GetRoomClass(room: Room): string {
        let status = room.status;
        if (room.selected) return "room sel";

        let containsThis: boolean = false;
        let containsOther: boolean = false;
        let filled: number = 0;

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

        if (status == 0) return "room unavailable";
        else if (status == 1) return "room empty";
        else if (status == 2) return "room occupied";
        else if (status == 3) return "room partial";
        else if (status == 4) return "room notready";
        return "room";
    }

    public unallocateRoom(roomA: RoomAllocation, room: Room) {
        this.dataService.UnllocateRoom(roomA.sno).subscribe(result => {
            var index = room.roomAllocation.indexOf(roomA, 0);
            room.roomAllocation.splice(index, 1)
            this.AssignRoom(room);
        });
    }

    public canAllocate(room: Room): boolean {
        return ((room.status == 1) || (room.status == 3)) && (!this.CheckOccupied(room));
    }

    reloadRooms() {
        this.dataService.GetRoomsByLocation('H7').subscribe(result => {
            this.rooms = result;
            this.AssignRoomsInit();
        });
    }

}
