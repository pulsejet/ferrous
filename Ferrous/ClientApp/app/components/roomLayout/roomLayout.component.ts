import { Component, Inject, ElementRef, ViewChild } from '@angular/core';
import { ActivatedRoute, Params } from '@angular/router';
import { Room, RoomAllocation } from '../interfaces';
import { Title } from '@angular/platform-browser';
import { DataService } from '../../DataService';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatDialog } from '@angular/material/dialog';
import { SSEService } from '../../SSEService';
import { RoomDialogComponent } from './RoomDialogComponent';
import * as $ from 'jquery';
import { Subscription } from 'rxjs/Subscription';

/* Room layout component */
@Component({
    selector: 'roomLayout',
    templateUrl: './roomLayout.component.html',
    styleUrls: ['../../Custom.css'],
})
export class RoomLayoutComponent {
    public rooms: Room[];                   /* master list of rooms     */
    public roomsInited: boolean = false;    /* rooms already inited     */
    public clno: string;                    /* current CLNo             */
    public cano: number;                    /* current CArrival No      */
    public loc_fullname: string;            /* location full name       */
    public loc_code: string;                /* code of current location */
    @ViewChild('roomsLayout')               /* layout element           */
        roomsLayout: ElementRef;            /*                          */
    public marking: boolean = false;        /* marking status change    */
    private sseStream: Subscription;        /* SSE Subscription         */

    constructor(private activatedRoute: ActivatedRoute,
        private titleService: Title,
        private dataService: DataService,
        public snackBar: MatSnackBar,
        public dialog: MatDialog,
        private sseService: SSEService,
        @Inject('BASE_URL') public baseUrl: string) {

        this.titleService.setTitle("Room Layout");

        /* Get URL parameters */
        this.activatedRoute.params.subscribe((params: Params) => {
            this.loc_code = params['location'];
            this.clno = params['id'];
            this.cano = params['cano'];
        });

        /* Get room layout by location */
        dataService.GetRoomLayout(this.loc_code).subscribe(result => {
            this.roomsLayout.nativeElement.innerHTML = result;

            /* Load rooms */
            this.reloadRooms();
        });
    }

    ngOnInit() {
        this.sseStream = this.sseService.observeMessages('/api/Rooms/buildingSSE/' + this.loc_code)
            .subscribe(message => {
                this.reloadRooms();
            });
    }

    ngOnDestroy() {
        if (this.sseStream) {
            this.sseStream.unsubscribe();
        }
    }

    reloadRooms(fullReload: boolean = false) {
        this.dataService.GetBuilding(this.loc_code).subscribe(result => {
            if (!this.rooms || fullReload)
                /* Perform a full replace */
                this.rooms = result.room;

            else {
                /* Copy over all objects keeping properties created by the client   *
                 * WARNING: This assumes that the number of rooms doesnt change.    */
                let newrooms: Room[] = result.room as Room[];
                let index: number = 0;

                for (let room of newrooms) {
                    let oldroom = this.rooms[index];
                    room.selected = oldroom.selected;
                    room.partialallot = oldroom.partialallot;
                    room.partialsel = oldroom.partialsel;
                    index += Number(1);
                }

                this.rooms = newrooms;
            }

            /* Assign other things */
            this.loc_fullname = result.locationFullName;
            this.AssignRoomsInit();
            this.AssignRooms();

            /* Alert the user */
            this.snackBar.open("Room data updated", "Dismiss", {
                duration: 2000,
            });

        });
    }

    /* Initialize rooms */
    public AssignRoomsInit() {
        /* Prevent initialization twice */
        if (this.roomsInited) return;

        var self = this;
        for (let room of this.rooms) {
            /* Find the room by HTML id */
            let ctrl = this.getRoomId(room)
            let index: number = self.rooms.indexOf(room);

            /* Mark the room selected */
            $(ctrl).click(function () {
                self.HandleRoomClick(index);
            });

            /* Handle right click */
            $(ctrl).contextmenu(function () {
                self.HandleContextMenuRoom(index);
                return false;
            });

        }

        /* Mark initialization done */
        this.roomsInited = true;
    }

    /* Handle right click of room */
    public HandleContextMenuRoom(index: number) {
        let dialog = this.dialog.open(RoomDialogComponent, {
            data: this.rooms[index]
        });
    }

    /* Handle room click */
    public HandleRoomClick(index: number) {
        let room: Room = this.rooms[index];
        if ((room.status == 1 && room.roomAllocation.length == 0)
            || (this.CheckPartial(room) && this.GetCapacity(room) > 0)
            || room.selected
            || this.marking) {

            room.selected = !room.selected;
        }

        /* Update room graphic */
        this.AssignRoom(room);
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
                }, error => {
                    /* Show error */
                    this.snackBar.open("Mark failed for " + room.room1, "Dismiss", {
                        duration: 2000,
                    });
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

        /* Validate */
        for (let room of this.rooms) {
            if (room.selected) {

                /* Status */
                if (room.status != 1) {
                    /* Show error */
                    this.snackBar.open("Non-allocable room " + room.room1, "Dismiss", {
                        duration: 2000,
                    });
                    console.log("Non-allocable room " + room.room1);
                    return;
                }

                /* Partial */
                if ((room.partialallot || this.CheckPartial(room)) &&
                    (!this.dataService.CheckValidNumber(room.partialsel, 1))) {

                    /* Show error */
                    this.snackBar.open("Invalid partial capacity for " + room.room1, "Dismiss", {
                        duration: 2000,
                    });
                    console.log("Invalid partial capacity for " + room.room1);
                    return;
                }
            }
        }

        for (let room of this.rooms) {
            if (room.selected) {
                this.dataService.AllotRoom(room, this.clno, this.cano).subscribe(result => {
                    /* Add new allocation */
                    room.roomAllocation.push(result);

                    /* Unmark the room and update graphic */
                    room.selected = false;
                    this.AssignRoom(room);
                }, error => {
                    /* Show error */
                    this.snackBar.open("Allotment failed for " + room.room1, "Dismiss", {
                        duration: 2000,
                    });
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
