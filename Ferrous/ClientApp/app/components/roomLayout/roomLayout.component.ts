import { Component, Inject, ElementRef, ViewChild } from '@angular/core';
import { ActivatedRoute, Params } from '@angular/router';
import { Room, RoomAllocation, Link } from '../interfaces';
import { Title } from '@angular/platform-browser';
import { DataService } from '../../DataService';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatDialog } from '@angular/material/dialog';
import { RoomDialogComponent } from './RoomDialogComponent';
import { Subscription } from 'rxjs/Subscription';
import { HubConnection } from '@aspnet/signalr-client';
import * as $ from 'jquery';
import { TimerObservable } from "rxjs/observable/TimerObservable";

/* Room layout component */
@Component({
    selector: 'roomLayout',
    templateUrl: './roomLayout.component.html',
    styleUrls: ['../../Custom.css'],
})
export class RoomLayoutComponent {
    /** Master list of rooms */
    public rooms: Room[];
    /** Always true after initialization */
    public roomsInited: boolean = false;
    /** Current CLNo */
    public clno: string;
    /** Current contingentArrivalNo */
    public cano: number;
    /** Full name of current location (e.g. Hostel 7) */
    public locFullname: string;
    /** Code of current location (e.g. H7) */
    public locCode: string;
    /** Layout element */
    @ViewChild('roomsLayout') roomsLayout: ElementRef;
    /** true if marking */
    public marking: boolean = false;
    /** WebSocket connection for layout */
    private hubConnection: HubConnection;
    /** Layout update Subscription */
    private updateSubscription: Subscription;
    /** Mark necessity of updation for next timer check */
    private updateNeeded: boolean = false;

    public urlLink: Link;
    public links: Link[];

    /** constructor for RoomLayoutComponent */
    constructor(
        private activatedRoute: ActivatedRoute,
        private titleService: Title,
        private dataService: DataService,
        public snackBar: MatSnackBar,
        public dialog: MatDialog,
        @Inject('BASE_URL') public baseUrl: string) {

        this.titleService.setTitle("Room Layout");

        /* Get URL parameters */
        this.activatedRoute.params.subscribe((params: Params) => {
            this.urlLink = dataService.DecodeObject(params['link']);
            this.locCode = params['location'];
        });

        /* Get room layout by location */
        dataService.GetRoomLayout("H7").subscribe(result => {
            this.roomsLayout.nativeElement.innerHTML = result;

            /* Load rooms */
            this.reloadRooms();
        });
    }

    ngOnInit() {
        /* Connect to the websocket   *
         * TODO: Use HATEOAS for this */
        this.hubConnection = new HubConnection('/api/websocket/building');

        /* Mark pending update on event 'updated' */
        this.hubConnection.on('updated', () => {
            this.updateNeeded = true;
        });

        /* Timer checks for updates every 2 seconds */
        this.updateSubscription = TimerObservable.create(0, 2000)
            .subscribe(() => {
                if (this.updateNeeded) {
                    this.reloadRooms();
                    this.updateNeeded = false;
                }
            });

        /* Start the connection */
        this.hubConnection.start()
            .then(() => {
                this.hubConnection.invoke('JoinBuilding', this.locCode);
                console.log('Hub connection started');
            })
            .catch(() => {
                console.log('Error while establishing connection');
            });
    }

    ngOnDestroy() {
        /* Unsubscribe if connected */
        if (this.updateSubscription)
            this.updateSubscription.unsubscribe();
    }

    /**
     * Reload all room data partially or fully
     * @param fullReload true to fully reload data
     */
    reloadRooms(fullReload: boolean = false) {
        this.dataService.FireLink(this.urlLink).subscribe(result => {
            this.links = result.links;

            if (!this.rooms || fullReload)
                /* Perform a full replace */
                this.rooms = result.room;

            else {
                /* Copy over all objects keeping properties created by the client   *
                 * WARNING: This assumes that the number of rooms doesnt change.    */
                const newrooms = result.room as Room[];
                let index: number = 0;

                for (let room of newrooms) {
                    const oldroom = this.rooms[index];
                    room.selected = oldroom.selected;
                    room.partialallot = oldroom.partialallot;
                    room.partialsel = oldroom.partialsel;
                    index += Number(1);
                }

                this.rooms = newrooms;
            }

            /* Assign other things */
            this.locFullname = result.locationFullName;
            this.assignRoomsInit();
            this.assignRooms();

            /* Alert the user */
            this.snackBar.open("Room data updated", "Dismiss", {
                duration: 2000,
            });

        });
    }

    /** Initialize Rooms */
    public assignRoomsInit() {
        /* Prevent initialization twice */
        if (this.roomsInited) return;

        const self = this;
        for (let room of this.rooms) {
            /* Find the room by HTML id */
            const ctrl = this.getRoomId(room);
            const index = self.rooms.indexOf(room);

            /* Mark the room selected */
            $(ctrl).click(() => {
                this.handleRoomClick(index);
            });

            /* Handle right click */
            $(ctrl).contextmenu(() => {
                this.handleContextMenuRoom(index);
                return false;
            });

        }

        /* Mark initialization done */
        this.roomsInited = true;
    }

    /**
     * Handle right click of room
     * @param index Index of Room in master
     */
    public handleContextMenuRoom(index: number) {
        this.dialog.open(RoomDialogComponent, {
            data: this.rooms[index]
        });
    }

    /**
     * Handle room click
     * @param index Index of Room in master
     */
    public handleRoomClick(index: number) {
        let room: Room = this.rooms[index];
        if ((room.status === 1 && room.roomAllocation.length === 0)
            || (this.checkPartial(room) && this.getCapacity(room) > 0)
            || room.selected
            || this.marking) {

            room.selected = !room.selected;
        }

        /* Update room graphic */
        this.assignRoom(room);
    }

    /** Update graphics for all rooms */
    public assignRooms() {
        for (let room of this.rooms) {
            this.assignRoom(room);
        }
    }

    /**
     * Update graphic for one room
     * @param room Room to be updated
     */
    public assignRoom(room: Room) {
        let ctrl = this.getRoomId(room);
        let clss: string = this.getRoomClass(room);

        /* Show capacity and room number */
        $(ctrl).html(this.getCapacity(room) + "<br><span>" + room.roomName.toString() + "</span>");

        /* Assign CSS class */
        $(ctrl).attr("class", clss);
    }

    /**
     * Try to mark all selected rooms with status
     * @param status Status to mark
     */
    public mark(status: number) {
        for (let room of this.rooms) {
            if (room.selected) {
                this.dataService.MarkRoom(room,status).subscribe(() => {
                    room.status = status;
                    room.selected = false;
                    this.assignRoom(room);
                }, () => {
                    /* Show error */
                    this.snackBar.open("Mark failed for " + room.roomName, "Dismiss", {
                        duration: 2000,
                    });
                });
            }
        }
    }

    /**
     * Get number of free spaces in room
     * @param room Room object
     */
    public getCapacity(room: Room): number {
        if (this.checkOccupied(room)) {
            return 0;
        } else if (this.checkPartial(room)) {
            return (room.capacity - this.getPartialNo(room));
        } else {
            return room.capacity;
        }
    }

    /** Allot all selected rooms to current Contingent */
    public allot() {

        /* Validate */
        for (let room of this.rooms) {
            if (room.selected) {

                /* Status */
                if (room.status !== 1) {
                    /* Show error */
                    this.snackBar.open("Non-allocable room " + room.roomName, "Dismiss", {
                        duration: 2000,
                    });
                    console.log("Non-allocable room " + room.roomName);
                    return;
                }

                /* Partial */
                if ((room.partialallot || this.checkPartial(room)) &&
                    (!this.dataService.CheckValidNumber(room.partialsel, 1))) {

                    /* Show error */
                    this.snackBar.open("Invalid partial capacity for " + room.roomName, "Dismiss", {
                        duration: 2000,
                    });
                    console.log("Invalid partial capacity for " + room.roomName);
                    return;
                }
            }
        }

        for (let room of this.rooms) {
            if (room.selected) {
                this.dataService.AllotRoom(room).subscribe(result => {
                    /* Add new allocation */
                    room.roomAllocation.push(result);

                    /* Unmark the room and update graphic */
                    room.selected = false;
                    this.assignRoom(room);
                }, (): void => {
                    /* Show error */
                    this.snackBar.open("Allotment failed for " + room.roomName, "Dismiss", {
                        duration: 2000,
                    });
                });
            }
        }
    }

    /** Check if room is full */
    public checkOccupied(room: Room): boolean {
        return this.dataService.RoomCheckOccupied(room);
    }

    /** Check if partially filled */
    public checkPartial(room: Room): boolean {
        return this.dataService.RoomCheckPartial(room);
    }

    /** Get partial number of people in room */
    public getPartialNo(room: Room): number {
        return this.dataService.RoomGetPartialNo(room);
    }

    /**
     * Get CSS class for room
     * @param room Room object
     */
    public getRoomClass(room: Room): string {
        /* Selected has top priority */
        if (room.selected) return "room sel";

        let containsThis: boolean = false;      /* Room contains current contingent     */
        let containsOther: boolean = false;     /* Room contains another contingent     */
        let filled: number = 0;                 /* Number of people currently in room   */

        /* Fill up local data */
        for (let roomA of room.roomAllocation) {
            if (roomA.contingentLeaderNo === this.clno)
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
                return "room partial";
            } else if (containsThis) {
                return "room already-partial";
            }
        } else {
            if (containsOther && !containsThis) {
                return "room occupied";
            } else if (!containsOther && containsThis) {
                return "room already";
            } else if (containsOther && containsThis) {
                return "room already-fullshared";
            }
        }

        /* Fall back to status code */
        let status = room.status;
        if (status === 0) return "room unavailable";
        else if (status === 1) return "room empty";
        else if (status === 2) return "room occupied";
        else if (status === 3) return "room partial";
        else if (status === 4) return "room notready";
        return "room";
    }

    /**
     * DELETE a RoomAllocation
     * @param roomA RoomAllocation object
     * @param room Room object for local removal of allocation
     */
    public unallocateRoom(roomA: RoomAllocation, room: Room) {
        this.dataService.UnallocateRoom(roomA).subscribe(() => {
            var index = room.roomAllocation.indexOf(roomA, 0);
            room.roomAllocation.splice(index, 1);
            this.assignRoom(room);
        });
    }

    /**
     * Check if room can be allocated
     * @param room Room object
     */
    public canAllocate(room: Room): boolean {
        return ((room.status === 1) || (room.status === 3)) && (!this.checkOccupied(room));
    }

    /**
     * Gets the CSS ID selector from a Room
     * @param room Room object
     */
    public getRoomId(room: Room): string {
        return ("#room-" + room.location + "-" + room.roomName).replace(/\s+/, "");
    }

}
