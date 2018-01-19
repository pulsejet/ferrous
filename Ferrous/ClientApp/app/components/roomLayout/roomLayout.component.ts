import { Component, Inject, ElementRef, ViewChild } from '@angular/core';
import { Http, Headers, RequestOptions } from '@angular/http';
import { ActivatedRoute, Params, Routes, Route, Router } from '@angular/router';
import { Room, RoomAllocation } from '../interfaces';
import * as $ from 'jquery';

@Component({
    selector: 'home',
    templateUrl: './roomLayout.component.html'
})
export class RoomLayoutComponent {
    public rooms: Room[];
    public clno: string;
    @ViewChild('roomsLayout') roomsLayout: ElementRef;
    public marking: boolean = false;

    constructor(private activatedRoute: ActivatedRoute,
        public router: Router, public http: Http, @Inject('BASE_URL') public baseUrl: string) {

        this.activatedRoute.params.subscribe((params: Params) => {
            this.clno = params['id'];
        });

        http.get(baseUrl + '/roomTemplates/H1.html').subscribe(result => {
            this.roomsLayout.nativeElement.innerHTML = result.text();
        });

        http.get(baseUrl + '/api/Rooms/ByLoc/H1').subscribe(result => {
            this.rooms = result.json() as Room[];
            this.AssignRoomsInit();
        });

    }

    public AssignRoomsInit() {
        var self = this;
        for (let room of this.rooms) {
            let str = "#room-" + room.location + "-" + room.room1;
            let ctrl = str.replace(/\s+/, "");

            $(ctrl).click(function () {
                if ((room.status == 1 && room.roomAllocation.length == 0)
                    || self.GetRoomClass(room) == "room partial"
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
                this.http.get(this.baseUrl + '/api/Rooms/mark/' + room.id + "/" + Status).subscribe(result => {
                    if (result.ok) {
                        room.status = Status; room.selected = false; this.AssignRoom(room);
                    }
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
                let url = this.baseUrl + '/api/Rooms/allot/' + room.id + '/' + this.clno;

                if (room.partialallot || this.CheckPartial(room)) {
                    if (room.partialsel == null) continue;
                    url += '/' + room.partialsel;
                }

                this.http.get(url).subscribe(result => {
                    if (result.ok) {
                        room.status = 5; room.selected = false; this.AssignRoom(room);
                    }
                });
            }
        }
    }

    public CheckOccupied(room: Room): boolean {
        return this.GetPartialNo(room) < 0;
    }

    public CheckPartial(room: Room): boolean {
        return this.GetPartialNo(room) > 0;
    }

    public GetPartialNo(room: Room): number {
        let count: number = 0;
        for (let roomA of room.roomAllocation) {
            if (roomA.partial != null) count += roomA.partial;
        }
        return count;
    }

    public GetRoomClass(room: Room): string {
        let status = room.status;
        if (room.selected) return "room sel";
        if (status == 5) return "room already";

        let partial = false;
        for (let roomA of room.roomAllocation) {
            if (roomA.contingentLeaderNo == this.clno) return "room already";
            if (roomA.partial <= 0 || roomA.partial == null) return "room occupied";
            partial = true;
        }
        if (partial) return "room partial";

        if (status == 0) return "room unavailable";
        else if (status == 1) return "room empty";
        else if (status == 2) return "room occupied";
        else if (status == 3) return "room partial";
        else if (status == 4) return "room notready";
        return "room";
    }

    public unallocateRoom(roomA: RoomAllocation, room: Room) {
        this.http.delete('/api/RoomAllocations/' + roomA.sno).subscribe(result => {
            var index = room.roomAllocation.indexOf(roomA, 0);
            room.roomAllocation.splice(index, 1)
            this.AssignRoom(room);
        });
    }

    public canAllocate(room: Room): boolean {
        return ((room.status == 1) || (room.status == 3)) && (!this.CheckOccupied(room));
    }

}
